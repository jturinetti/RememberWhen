using System;
using System.Collections.Generic;
using System.Text;

namespace RememberWhen.Lambda.Models
{
    public class RememberWhenResponseModel
    {
        public string Message { get; }

        public RememberWhenResponseModel(string message)
        {
            Message = message;
        }
    }
}
