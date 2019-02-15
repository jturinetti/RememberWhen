using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AwsDotnetCsharp
{
    public class Handler
    {
        string _wifeEmail = "";
        string _wifePhone = "";
        string _husbandPhone = Environment.GetEnvironmentVariable("HusbandPhoneNumber");
        string _husbandEmail = Environment.GetEnvironmentVariable("HusbandEmail");

        string _twilioAccountSid = Environment.GetEnvironmentVariable("TwilioAccountSid");
        string _twilioAuthToken = Environment.GetEnvironmentVariable("TwilioAuthToken");
        string _twilioPhoneNumber = Environment.GetEnvironmentVariable("TwilioPhoneNumber");

        public async Task<Response> Reminisce()
        {
            var memoryToSend = SelectMemory();

            using (var ses = new AmazonSimpleEmailServiceClient(RegionEndpoint.USWest2))
            {
                // TODO: verify email identities here

                // send email(s)
                await ses.SendEmailAsync(new SendEmailRequest
                {
                    Source = _husbandEmail,
                    Destination = new Destination(new List<string> { _husbandEmail }),
                    Message = new Message
                    {
                        Body = new Body(new Content(memoryToSend)),
                        Subject = new Content("thinking of you")
                    }
                });

                // send text(s)
                TwilioClient.Init(_twilioAccountSid, _twilioAuthToken);

                var textMessage = MessageResource.Create(
                    body: memoryToSend,
                    from: new Twilio.Types.PhoneNumber(_twilioPhoneNumber),
                    to: new Twilio.Types.PhoneNumber(_husbandPhone)
                );
            }

            return new Response(memoryToSend);
        }

        private string SelectMemory()
        {

            var memories = new List<string>
            {
                "Remember that time we got married?",
                "Remember that time we took a trip to Europe to see Italy and Greece?",
                "Remember the night we met at Kara's party?"
            };

            var randomizer = new Random(DateTime.UtcNow.DayOfYear + DateTime.UtcNow.Second); // this could be better
            var randomIndex = randomizer.Next(memories.Count);

            return memories[randomIndex];
        }
    }

    public class Response
    {
        public string Message {get; set;}

        public Response(string message)
        {
            Message = message;
        }
    }
}
