using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.DependencyInjection;
using RememberWhen.Lambda.Models;
using RememberWhen.Lambda.Services;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace RememberWhen.Lambda
{
    public class Handler
    {
        const string HusbandPhoneNumberKey = "HusbandPhoneNumber";
        const string HusbandEmailKey = "HusbandEmail";
        const string WifePhoneNumberKey = "WifePhoneNumber";
        const string WifeEmailKey = "WifeEmail";
        const string TwilioAccountSidKey = "TwilioAccountSid";
        const string TwilioAuthTokenKey = "TwilioAuthToken";
        const string TwilioPhoneNumberKey = "TwilioPhoneNumber";

        private readonly IDictionary<string, string> _parameterDictionary;
        private readonly bool _isProduction;

        private readonly IApplicationService _application;

        public Handler()
        {
            _parameterDictionary = new Dictionary<string, string>
            {
                { HusbandPhoneNumberKey, "" },
                { HusbandEmailKey, "" },
                { WifePhoneNumberKey, "" },
                { WifeEmailKey, "" },
                { TwilioAccountSidKey, "" },
                { TwilioAuthTokenKey, "" },
                { TwilioPhoneNumberKey, "" }
            };

            var serviceProvider = InitializeApplication();
            _application = serviceProvider.GetService<IApplicationService>();
            var environmentService = serviceProvider.GetService<IEnvironmentManagementService>();

            _isProduction = environmentService.EnvironmentType == Environments.Production;
        }

        private ServiceProvider InitializeApplication()
        {
            var services = new ServiceCollection();

            services.AddTransient<IMemoryService, StaticMemoryService>();
            services.AddTransient<IEmailService, SESEmailService>();
            services.AddTransient<IParameterManagementService, SSMParameterManagementService>();
            services.AddTransient<ITextMessageService, TwilioTextMessageService>();
            services.AddTransient<IEnvironmentManagementService, AWSEnvironmentManagementService>();
            services.AddTransient<IApplicationService, RememberWhenApplicationService>();

            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider;
        }

        public async Task<RememberWhenResponseModel> Reminisce()
        {
            // retrieve all sensitive parameters
            await RetrieveAmazonSSMParameters();

            // randomly pick memory to send out
            var memoryToSend = SelectMemory();

            // find email addresses to send memory to
            var emailsToSendTo = new List<string> { _parameterDictionary[HusbandEmailKey] };
            if (_isProduction)
            {
                emailsToSendTo.Add(_parameterDictionary[WifeEmailKey]);
            }

            // send memory to email addresses
            await SendMemoryViaEmail(memoryToSend, emailsToSendTo);

            if (_isProduction)
            {
                // send memory to phones via text
                await SendMemoryViaText(memoryToSend);
            }

            return new RememberWhenResponseModel(memoryToSend);
        }

        private async Task RetrieveAmazonSSMParameters()
        {
            using (var ssm = new AmazonSimpleSystemsManagementClient(RegionEndpoint.USWest2))
            {
                var keyList = _parameterDictionary.Keys.ToList();
                foreach (var key in keyList)
                {
                    var response = await ssm.GetParameterAsync(new GetParameterRequest
                    {
                        Name = key,
                        WithDecryption = true
                    });

                    _parameterDictionary[key] = response.Parameter.Value;
                }
            }
        }

        private string SelectMemory()
        {

            var memories = new List<string>
            {
                "Remember that time we got married?",
                "Remember that time we took a trip to Europe to see Italy and Greece?",
                "Remember the night we met at Kara's party?",
                "Remember the chair in Hawaii?",
                "Remember that time at our friend's wedding when I made you walk to the restroom by yourself?"
            };

            var randomizer = new Random(DateTime.UtcNow.DayOfYear + DateTime.UtcNow.Second); // this could be better
            var randomIndex = randomizer.Next(memories.Count);

            return memories[randomIndex];
        }

        private async Task SendMemoryViaEmail(string memoryToSend, List<string> emailsToSendTo)
        {
            using (var ses = new AmazonSimpleEmailServiceClient(RegionEndpoint.USWest2))
            {
                // check to see if targeted emails are verified
                var verificationAttributesResponse = await ses.GetIdentityVerificationAttributesAsync(new GetIdentityVerificationAttributesRequest
                {
                    Identities = emailsToSendTo
                });

                // ensure emails are verified, then send to all verified email addresses
                var emailIndex = 0;
                while (emailIndex < emailsToSendTo.Count)
                {
                    var email = emailsToSendTo[emailIndex];
                    if (!verificationAttributesResponse.VerificationAttributes.ContainsKey(email)
                        || verificationAttributesResponse.VerificationAttributes[email].VerificationStatus.Value != "Success")
                    {
                        // send request to verify email
                        await ses.VerifyEmailIdentityAsync(new VerifyEmailIdentityRequest
                        {
                            EmailAddress = email
                        });

                        // remove from list of emails to send to this time
                        emailsToSendTo.RemoveAt(emailIndex);
                    }
                    else
                    {
                        emailIndex++;
                    }
                }

                // send email(s)
                await ses.SendEmailAsync(new SendEmailRequest
                {
                    Source = _parameterDictionary[HusbandEmailKey],
                    Destination = new Destination(emailsToSendTo),
                    Message = new Message
                    {
                        Body = new Body(new Content(memoryToSend)),
                        Subject = new Content("thinking of you")
                    }
                });
            }
        }        

        private async Task SendMemoryViaText(string memoryToSend)
        {
            // send text(s)
            TwilioClient.Init(_parameterDictionary[TwilioAccountSidKey], _parameterDictionary[TwilioAuthTokenKey]);

            // send to husband
            await MessageResource.CreateAsync(
                body: memoryToSend,
                from: new Twilio.Types.PhoneNumber(_parameterDictionary[TwilioPhoneNumberKey]),
                to: new Twilio.Types.PhoneNumber(_parameterDictionary[HusbandPhoneNumberKey])
            );

            // send to wife
            await MessageResource.CreateAsync(
                body: memoryToSend,
                from: new Twilio.Types.PhoneNumber(_parameterDictionary[TwilioPhoneNumberKey]),
                to: new Twilio.Types.PhoneNumber(_parameterDictionary[WifePhoneNumberKey])
            );
        }
    }
}
