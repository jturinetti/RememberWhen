using System;
using System.Collections.Generic;
using System.Text;

namespace RememberWhen.Lambda.Services
{
    public static class Constants
    {
        // parameter keys
        public const string HusbandPhoneNumberKey = "HusbandPhoneNumber";
        public const string HusbandEmailKey = "HusbandEmail";
        public const string WifePhoneNumberKey = "WifePhoneNumber";
        public const string WifeEmailKey = "WifeEmail";
        public const string TwilioAccountSidKey = "TwilioAccountSid";
        public const string TwilioAuthTokenKey = "TwilioAuthToken";
        public const string TwilioPhoneNumberKey = "TwilioPhoneNumber";

        // list of all parameter keys
        public static IReadOnlyCollection<string> ParameterKeys = new List<string>
        {
            HusbandPhoneNumberKey,
            HusbandEmailKey,
            WifePhoneNumberKey,
            WifeEmailKey,
            TwilioAccountSidKey,
            TwilioAuthTokenKey,
            TwilioPhoneNumberKey
        }.AsReadOnly();
    }
}
