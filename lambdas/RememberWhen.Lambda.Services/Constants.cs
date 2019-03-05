using System.Collections.Generic;

namespace RememberWhen.Lambda.Services
{
    public static class Constants
    {
        // AWS constants
        public const string DevEnvironmentName = "dev";
        public const string ProdEnvironmentName = "prod";
        public const string AWSEnvironmentEnvironmentVariableName = "STAGE";

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
