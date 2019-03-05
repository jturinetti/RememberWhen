using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using RememberWhen.Lambda.Services;
using Xunit;

namespace RememberWhen.Lambda.Tests
{
    public class RememberWhenApplicationServiceTests
    {
        private IApplicationService _sut;

        private Mock<IEnvironmentManagementService> _environmentManagementServiceMock;
        private Mock<IParameterManagementService> _parameterManagementServiceMock;
        private Mock<IMemoryService> _memoryServiceMock;
        private Mock<IEmailService> _emailServiceMock;
        private Mock<ITextMessageService> _textMessageServiceMock;

        private const string StaticMemory = "this is a memory";
        private const string HusbandEmail = "husband@yopmail.com";
        private const string WifeEmail = "wife@yopmail.com";
        private const string HusbandPhone = "9999999999";
        private const string WifePhone = "1111111111";
        private const string TwilioAccountSid = "123123123";
        private const string TwilioAuthToken = "lkjsdlkfj9";
        private const string TwilioPhoneNumber = "5555555555";

        public RememberWhenApplicationServiceTests()
        {
            _parameterManagementServiceMock = new Mock<IParameterManagementService>();
            _parameterManagementServiceMock.Setup(m => m.RetrieveParameters(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new Dictionary<string, string>
                {
                    { Constants.HusbandEmailKey, HusbandEmail },
                    { Constants.WifeEmailKey, WifeEmail },
                    { Constants.HusbandPhoneNumberKey, HusbandPhone },
                    { Constants.WifePhoneNumberKey, WifePhone },
                    { Constants.TwilioAccountSidKey, TwilioAccountSid },
                    { Constants.TwilioAuthTokenKey, TwilioAuthToken },
                    { Constants.TwilioPhoneNumberKey, TwilioPhoneNumber }
                });

            _memoryServiceMock = new Mock<IMemoryService>();
            _memoryServiceMock.Setup(m => m.RetrieveRandomMemory())
                .Returns(StaticMemory);

            _emailServiceMock = new Mock<IEmailService>();

            _textMessageServiceMock = new Mock<ITextMessageService>();
        }

        private void MockEnvironment(Environments environment)
        {
            _environmentManagementServiceMock = new Mock<IEnvironmentManagementService>();
            _environmentManagementServiceMock.SetupGet(m => m.EnvironmentType).Returns(environment);
        }

        [Fact]
        public async Task Run_GivenDevEnvironment_MakesExpectedCalls()
        {
            MockEnvironment(Environments.Dev);
            _sut = new RememberWhenApplicationService(
                _parameterManagementServiceMock.Object,
                _environmentManagementServiceMock.Object,
                _memoryServiceMock.Object,
                _emailServiceMock.Object,
                _textMessageServiceMock.Object);

            await _sut.Run();

            _parameterManagementServiceMock.Verify(m => m.RetrieveParameters(It.IsAny<IEnumerable<string>>()), Times.Once);
            _memoryServiceMock.Verify(m => m.RetrieveRandomMemory(), Times.Once);
            _emailServiceMock.Verify(m => m.SendMemory(StaticMemory, HusbandEmail, new List<string> { HusbandEmail }), Times.Once);
            _textMessageServiceMock.Verify(m => m.SendMemory(StaticMemory, TwilioPhoneNumber, It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        [Fact]
        public async Task Run_GivenProdEnvironment_MakesExpectedCalls()
        {
            MockEnvironment(Environments.Production);
            _sut = new RememberWhenApplicationService(
                _parameterManagementServiceMock.Object,
                _environmentManagementServiceMock.Object,
                _memoryServiceMock.Object,
                _emailServiceMock.Object,
                _textMessageServiceMock.Object);

            await _sut.Run();

            _parameterManagementServiceMock.Verify(m => m.RetrieveParameters(It.IsAny<IEnumerable<string>>()), Times.Once);
            _memoryServiceMock.Verify(m => m.RetrieveRandomMemory(), Times.Once);
            _emailServiceMock.Verify(m => m.SendMemory(StaticMemory, HusbandEmail, new List<string> { HusbandEmail, WifeEmail }), Times.Once);
            _textMessageServiceMock.Verify(m => m.SendMemory(StaticMemory, TwilioPhoneNumber, new List<string> { HusbandPhone, WifePhone }), Times.Once);
        }

        [Fact]
        public async Task Run_ReturnsSelectedMemory()
        {
            MockEnvironment(Environments.Dev);
            _sut = new RememberWhenApplicationService(
                _parameterManagementServiceMock.Object,
                _environmentManagementServiceMock.Object,
                _memoryServiceMock.Object,
                _emailServiceMock.Object,
                _textMessageServiceMock.Object);

            var result = await _sut.Run();

            Assert.Equal(StaticMemory, result.Message);
        }
    }
}
