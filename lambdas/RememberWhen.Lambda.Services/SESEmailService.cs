using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

namespace RememberWhen.Lambda.Services
{
    public interface IEmailService
    {
        Task SendMemory(string memoryToSend, string fromEmailAddress, List<string> targetEmailAddresses);
    }

    public class SESEmailService : IEmailService
    {
        private readonly IAmazonSimpleEmailService _ses;

        public SESEmailService(IAmazonSimpleEmailService ses)
        {
            _ses = ses;
        }

        public async Task SendMemory(string memoryToSend, string fromEmailAddress, List<string> targetEmailAddresses)
        {            
            // check to see if targeted emails are verified
            var verificationAttributesResponse = await _ses.GetIdentityVerificationAttributesAsync(new GetIdentityVerificationAttributesRequest
            {
                Identities = targetEmailAddresses
            });

            // ensure emails are verified, then send to all verified email addresses
            var emailIndex = 0;
            while (emailIndex < targetEmailAddresses.Count)
            {
                var email = targetEmailAddresses[emailIndex];
                if (!verificationAttributesResponse.VerificationAttributes.ContainsKey(email)
                    || verificationAttributesResponse.VerificationAttributes[email].VerificationStatus.Value != "Success")
                {
                    // send request to verify email
                    await _ses.VerifyEmailIdentityAsync(new VerifyEmailIdentityRequest
                    {
                        EmailAddress = email
                    });

                    // remove from list of emails to send to this time
                    targetEmailAddresses.RemoveAt(emailIndex);
                }
                else
                {
                    emailIndex++;
                }
            }

            // send email(s)
            await _ses.SendEmailAsync(new SendEmailRequest
            {
                Source = fromEmailAddress,
                Destination = new Destination(targetEmailAddresses),
                Message = new Message
                {
                    Body = new Body(new Content(memoryToSend)),
                    Subject = new Content("thinking of you")
                }
            });
        }
    }
}
