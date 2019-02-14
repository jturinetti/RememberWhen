using System;
using Amazon.Lambda.Core;

[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AwsDotnetCsharp
{
    public class Handler
    {
       public Response Reminisce()
       {
           return new Response("Go Serverless v1.0! Your function executed successfully!");
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
