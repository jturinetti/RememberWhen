using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Moq;
using RememberWhen.Lambda.Services;
using Xunit;

namespace RememberWhen.Lambda.Tests
{
    public class SESEmailServiceTests
    {
        private readonly IEmailService _sut;
        private readonly Mock<IAmazonSimpleEmailService> _sesMock;
        private readonly List<string> emailList;

        private const string VerifiedEmailAddress = "verified@yopmail.com";
        private const string UnverifiedEmailAddress = "unverified@yopmail.com";
        private const string FailedCallEmailAddress = "fail@yopmail.com";
        private const string FromEmailAddress = "fromemail@yopmail.com";
        private const string MemoryText = "random memory";

        public SESEmailServiceTests()
        {
            emailList = new List<string>
            {
                VerifiedEmailAddress,
                UnverifiedEmailAddress,
                FailedCallEmailAddress
            };

            _sesMock = new Mock<IAmazonSimpleEmailService>();
            _sesMock.Setup(m => m.GetIdentityVerificationAttributesAsync(
                    It.Is<GetIdentityVerificationAttributesRequest>(x => x.Identities.Contains(VerifiedEmailAddress)
                        && x.Identities.Contains(UnverifiedEmailAddress)
                        && x.Identities.Contains(FailedCallEmailAddress)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetIdentityVerificationAttributesResponse
                {
                    HttpStatusCode = HttpStatusCode.OK,
                    VerificationAttributes = new Dictionary<string, IdentityVerificationAttributes>
                    {
                        { VerifiedEmailAddress, new IdentityVerificationAttributes
                            {
                                VerificationStatus = new VerificationStatus("Success")
                            }
                        },
                        {
                            FailedCallEmailAddress, new IdentityVerificationAttributes
                            {
                                VerificationStatus = new VerificationStatus("FAIL")
                            }
                        }
                    }
                });

            _sesMock.Setup(m => m.VerifyEmailIdentityAsync(
                    It.Is<VerifyEmailIdentityRequest>(x => x.EmailAddress == UnverifiedEmailAddress
                        || x.EmailAddress == FailedCallEmailAddress),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new VerifyEmailIdentityResponse
                {
                    HttpStatusCode = HttpStatusCode.OK
                });

            _sesMock.Setup(m => m.SendEmailAsync(
                It.Is<SendEmailRequest>(x => x.Destination.ToAddresses.Count == 1
                    && x.Destination.ToAddresses[0] == VerifiedEmailAddress),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SendEmailResponse
            {
                HttpStatusCode = HttpStatusCode.OK,
                MessageId = Guid.NewGuid().ToString()
            });

            _sut = new SESEmailService(_sesMock.Object);
        }

        [Fact]
        public async Task SendMemory_GetIdentityVerificationAttributes_IsCalled()
        {
            await _sut.SendMemory(MemoryText, FromEmailAddress, new List<string>(emailList));
            
            _sesMock.Verify(m => m.GetIdentityVerificationAttributesAsync(
                It.Is<GetIdentityVerificationAttributesRequest>(x => x.Identities.Contains(VerifiedEmailAddress)
                    && x.Identities.Contains(UnverifiedEmailAddress)
                    && x.Identities.Contains(FailedCallEmailAddress)),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SendMemory_GivenVerificationAttributeMissing_VerifyEmailIdentity_IsCalled_WithUnverifiedEmails()
        {
            await _sut.SendMemory(MemoryText, FromEmailAddress, new List<string>(emailList));

            _sesMock.Verify(m => m.VerifyEmailIdentityAsync(
                It.Is<VerifyEmailIdentityRequest>(x => x.EmailAddress == UnverifiedEmailAddress),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SendMemory_GivenVerificationAttributeCallFails_VerifyEmailIdentity_IsCalled()
        {
            await _sut.SendMemory(MemoryText, FromEmailAddress, new List<string>(emailList));

            _sesMock.Verify(m => m.VerifyEmailIdentityAsync(
                It.Is<VerifyEmailIdentityRequest>(x => x.EmailAddress == FailedCallEmailAddress),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SendMemory_GivenVerificationAttributesReturned_SendEmail_IsCalled_WithVerifiedEmailsAndExpectedMemory()
        {
            await _sut.SendMemory(MemoryText, FromEmailAddress, new List<string>(emailList));

            _sesMock.Verify(m => m.SendEmailAsync(
                It.Is<SendEmailRequest>(x => x.Destination.ToAddresses.Count == 1 
                    && x.Destination.ToAddresses.Contains(VerifiedEmailAddress)),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
