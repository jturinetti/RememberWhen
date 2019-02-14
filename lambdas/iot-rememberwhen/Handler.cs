using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using Amazon.Lambda.Core;

[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AwsDotnetCsharp
{
    public class Handler
    {
       public Response Reminisce()
       {
          var memoryToSend = SelectMemory();

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
