using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using RememberWhen.Lambda.Models;
using RememberWhen.Lambda.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace RememberWhen.Lambda
{
    public class Handler
    {
        private readonly IApplicationService _application;

        public Handler()
        {
            var serviceProvider = InitializeApplication();
            _application = serviceProvider.GetService<IApplicationService>();
        }

        private ServiceProvider InitializeApplication()
        {
            var services = new ServiceCollection();

            services.AddTransient<IMemoryService, StaticMemoryService>();
            services.AddTransient<IEmailService, SESEmailService>();
            services.AddTransient<IParameterManagementService, SSMParameterManagementService>();
            services.AddTransient<ITextMessageService, TwilioTextMessageService>();
            services.AddTransient<IEnvironmentManagementService, AWSEnvironmentManagementService>();
            services.AddTransient<IApplicationService, RememberWhenApplicationService>();

            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider;
        }

        /// <summary>
        /// Lambda handler method called by AWS at runtime.
        /// </summary>
        /// <returns></returns>
        public async Task<RememberWhenResponseModel> Reminisce()
        {
            var result = await _application.Run();
            return result;
        }
    }
}
