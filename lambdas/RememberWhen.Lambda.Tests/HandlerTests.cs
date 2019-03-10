using System;
using System.Linq;
using Amazon;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RememberWhen.Lambda.Services;
using Xunit;

namespace RememberWhen.Lambda.Tests
{
    public class HandlerTests
    {
        private readonly IRememberWhenLambdaHandler _sut;

        public HandlerTests()
        {
            _sut = new Handler();
        }

        [Fact]
        public void Handler_InitializeServiceDependencies_AllDependenciesResolve()
        {
            // initialize dependency registrations
            var services = _sut.InitializeServiceDependencies();

            // replace some concrete registrations with mock ones
            MockEnvironmentManagementService(services);

            // build container and resolve
            var container = services.BuildServiceProvider();
            var app = container.GetService<IApplicationService>();
        }

        private void MockEnvironmentManagementService(ServiceCollection services)
        {
            var mockEnvironmentService = new Mock<IEnvironmentManagementService>();
            mockEnvironmentService.SetupGet(m => m.AWSRegion)
                .Returns(RegionEndpoint.USWest2);
            mockEnvironmentService.Setup(m => m.RetrieveEnvironmentName())
                .Returns(Constants.DevEnvironmentName);
            ReplaceExistingDependencyRegistration(services, mockEnvironmentService);
        }
        
        private void ReplaceExistingDependencyRegistration<TService>(ServiceCollection services, Mock<TService> mockedInstance)
            where TService : class
        {
            var concreteService = services.Single(s => s.ServiceType == typeof(TService));
            var lifetimeScope = concreteService.Lifetime;
            services.Remove(concreteService);
            
            if (lifetimeScope == ServiceLifetime.Singleton)
            {
                services.AddSingleton(mockedInstance.Object);
            }
            else if (lifetimeScope == ServiceLifetime.Transient)
            {
                services.AddTransient(typeof(TService), fac => mockedInstance.Object);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
