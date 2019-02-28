using System.Collections.Generic;
using System.Threading.Tasks;
using RememberWhen.Lambda.Models;

namespace RememberWhen.Lambda.Services
{
    public interface IApplicationService
    {
        Task<RememberWhenResponseModel> Run();
    }

    public class RememberWhenApplicationService : IApplicationService
    {
        private readonly IEnvironmentManagementService _environmentService;
        private readonly IParameterManagementService _parameterManagementService;
        private readonly IMemoryService _memoryService;
        private readonly IEmailService _emailService;
        private readonly ITextMessageService _textMessageService;

        public RememberWhenApplicationService(            
            IParameterManagementService parameterManagementService,
            IEnvironmentManagementService environmentService,
            IMemoryService memoryService,
            IEmailService emailService, 
            ITextMessageService textMessageService)
        {
            _environmentService = environmentService;
            _parameterManagementService = parameterManagementService;
            _memoryService = memoryService;
            _emailService = emailService;
            _textMessageService = textMessageService;
        }

        public async Task<RememberWhenResponseModel> Run()
        {
            // retrieve all sensitive parameters
            var parameterDictionary = await _parameterManagementService.RetrieveParameters(Constants.ParameterKeys);

            // determine environment
            var isProduction = _environmentService.EnvironmentType == Environments.Production;

            // select memory
            var memoryToSend = _memoryService.RetrieveRandomMemory();

            // find email addresses to send memory to
            var emailsToSendTo = new List<string> { parameterDictionary[Constants.HusbandEmailKey] };
            if (isProduction)
            {
                emailsToSendTo.Add(parameterDictionary[Constants.WifeEmailKey]);
            }

            // send memory to email addresses
            await _emailService.SendMemory(memoryToSend, parameterDictionary[Constants.HusbandEmailKey], emailsToSendTo);

            if (isProduction)
            {
                // send memory to phones via text
                await _textMessageService.SendMemory(
                    memoryToSend,
                    parameterDictionary[Constants.TwilioPhoneNumberKey],
                    new string[2] { parameterDictionary[Constants.HusbandPhoneNumberKey], parameterDictionary[Constants.WifePhoneNumberKey] }
                );
            }

            return new RememberWhenResponseModel(memoryToSend);
        }
    }
}
