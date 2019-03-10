using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.SimpleEmail;
using Amazon.SimpleSystemsManagement;
using Microsoft.Extensions.DependencyInjection;
using RememberWhen.Lambda.Models;
using RememberWhen.Lambda.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace RememberWhen.Lambda
{
    public interface IRememberWhenLambdaHandler
    {
        ServiceCollection InitializeServiceDependencies();
        Task<RememberWhenResponseModel> Reminisce();
    }

    public class Handler : IRememberWhenLambdaHandler
    {
        public ServiceCollection InitializeServiceDependencies()
        {
            var services = new ServiceCollection();

            // custom service registrations
            services.AddTransient<IMemoryService, StaticMemoryService>();
            services.AddTransient<IEmailService, SESEmailService>();
            services.AddTransient<IParameterManagementService, SSMParameterManagementService>();
            services.AddTransient<ITextMessageService, TwilioTextMessageService>();
            services.AddTransient<IApplicationService, RememberWhenApplicationService>();
            services.AddSingleton<IEnvironmentManagementService, AWSEnvironmentManagementService>();

            // Amazon service registrations
            services.AddTransient<IAmazonSimpleEmailService, AmazonSimpleEmailServiceClient>(container =>
            {
                var environmentService = container.GetService<IEnvironmentManagementService>();
                return new AmazonSimpleEmailServiceClient(environmentService.AWSRegion);
            });
            services.AddTransient<IAmazonSimpleSystemsManagement, AmazonSimpleSystemsManagementClient>(container =>
            {
                var environmentService = container.GetService<IEnvironmentManagementService>();
                return new AmazonSimpleSystemsManagementClient(environmentService.AWSRegion);
            });

            return services;
        }

        /// <summary>
        /// Lambda handler method called by AWS at runtime.
        /// </summary>
        /// <returns></returns>
        public async Task<RememberWhenResponseModel> Reminisce()
        {
            // get dependency registrations
            var serviceRegistrations = InitializeServiceDependencies();

            // build container
            var container = serviceRegistrations.BuildServiceProvider();
            
            // run application
            var application = container.GetService<IApplicationService>();
            var result = await application.Run();
            return result;
        }
    }
}
