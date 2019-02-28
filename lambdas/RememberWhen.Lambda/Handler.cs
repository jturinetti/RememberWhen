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
        private IDictionary<string, string> _parameterDictionary;
        private bool _isProduction;

        private IApplicationService _application;

        public Handler()
        {
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
            // TODO: move what we can from here to constructor?
            var serviceProvider = InitializeApplication();
            _application = serviceProvider.GetService<IApplicationService>();

            // retrieve all sensitive parameters
            var parameterService = serviceProvider.GetService<IParameterManagementService>();
            _parameterDictionary = await parameterService.RetrieveParameters(Constants.ParameterKeys);

            // determine environment
            var environmentService = serviceProvider.GetService<IEnvironmentManagementService>();
            _isProduction = environmentService.EnvironmentType == Environments.Production;

            // select memory
            var memoryService = serviceProvider.GetService<IMemoryService>();
            var memoryToSend = memoryService.RetrieveRandomMemory();

            // find email addresses to send memory to
            var emailsToSendTo = new List<string> { _parameterDictionary[Constants.HusbandEmailKey] };
            if (_isProduction)
            {
                emailsToSendTo.Add(_parameterDictionary[Constants.WifeEmailKey]);
            }

            // send memory to email addresses
            var emailService = serviceProvider.GetService<IEmailService>();
            await emailService.SendMemory(memoryToSend, _parameterDictionary[Constants.HusbandEmailKey], emailsToSendTo);

            if (_isProduction)
            {
                // send memory to phones via text
                await SendMemoryViaText(memoryToSend);
            }

            return new RememberWhenResponseModel(memoryToSend);
        }

        private async Task SendMemoryViaText(string memoryToSend)
        {
            // send text(s)
            TwilioClient.Init(_parameterDictionary[Constants.TwilioAccountSidKey], _parameterDictionary[Constants.TwilioAuthTokenKey]);

            // send to husband
            await MessageResource.CreateAsync(
                body: memoryToSend,
                from: new Twilio.Types.PhoneNumber(_parameterDictionary[Constants.TwilioPhoneNumberKey]),
                to: new Twilio.Types.PhoneNumber(_parameterDictionary[Constants.HusbandPhoneNumberKey])
            );

            // send to wife
            await MessageResource.CreateAsync(
                body: memoryToSend,
                from: new Twilio.Types.PhoneNumber(_parameterDictionary[Constants.TwilioPhoneNumberKey]),
                to: new Twilio.Types.PhoneNumber(_parameterDictionary[Constants.WifePhoneNumberKey])
            );
        }
    }
}
