using System.Collections.Generic;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace RememberWhen.Lambda.Services
{
    public interface ITextMessageService
    {
        Task SendMemory(string memoryToSend, string fromPhoneNumber, IEnumerable<string> phoneNumbers);
    }

    public class TwilioTextMessageService : ITextMessageService
    {
        private readonly IParameterManagementService _parameterManagementService;

        public TwilioTextMessageService(IParameterManagementService parameterManagementService)
        {
            _parameterManagementService = parameterManagementService;
        }

        public async Task SendMemory(string memoryToSend, string fromPhoneNumber, IEnumerable<string> phoneNumbers)
        {
            if (string.IsNullOrEmpty(memoryToSend)
                || string.IsNullOrEmpty(fromPhoneNumber))
            {
                return;
            }

            var parameterDictionary = await _parameterManagementService.RetrieveParameters(new List<string>
            {
                Constants.TwilioAccountSidKey,
                Constants.TwilioAuthTokenKey,
                Constants.TwilioPhoneNumberKey
            });

            // initialize client
            TwilioClient.Init(parameterDictionary[Constants.TwilioAccountSidKey], parameterDictionary[Constants.TwilioAuthTokenKey]);
            
            // send text(s)
            foreach (var phoneNumber in phoneNumbers)
            {
                await MessageResource.CreateAsync(
                    body: memoryToSend,
                    from: new Twilio.Types.PhoneNumber(parameterDictionary[Constants.TwilioPhoneNumberKey]),
                    to: new Twilio.Types.PhoneNumber(phoneNumber)
                );
            }
        }
    }
}
