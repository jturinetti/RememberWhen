using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AwsDotnetCsharp
{
    public class Handler
    {
        const string MyEmail = "jeremy.turinetti@gmail.com";

       public async Task<Response> Reminisce()
       {
            var memoryToSend = SelectMemory();

            using (var ses = new AmazonSimpleEmailServiceClient(RegionEndpoint.USWest2))
            {
                // TODO: verify email identities here

                await ses.SendEmailAsync(new SendEmailRequest
                {
                    Source = MyEmail,
                    Destination = new Destination(new List<string> {  MyEmail }),
                    Message = new Message
                    {
                        Body = new Body(new Content(memoryToSend)),
                        Subject = new Content("Thinking of you")
                    }
                });
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
