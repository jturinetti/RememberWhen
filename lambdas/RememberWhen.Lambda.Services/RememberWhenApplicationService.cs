using System;
using System.Collections.Generic;
using System.Text;

namespace RememberWhen.Lambda.Services
{
    public interface IApplicationService
    {

    }

    public class RememberWhenApplicationService : IApplicationService
    {
        private readonly IMemoryService _memoryService;
        private readonly IParameterManagementService _parameterManagementService;
        private readonly IEmailService _emailService;
        private readonly ITextMessageService _textMessageService;

        public RememberWhenApplicationService(
            IMemoryService memoryService,
            IParameterManagementService parameterManagementService, 
            IEmailService emailService, 
            ITextMessageService textMessageService)
        {
            _memoryService = memoryService;
            _parameterManagementService = parameterManagementService;
            _emailService = emailService;
            _textMessageService = textMessageService;
        }
    }
}
