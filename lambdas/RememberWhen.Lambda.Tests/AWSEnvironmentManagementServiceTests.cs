using System;
using Amazon;
using RememberWhen.Lambda.Services;
using Xunit;

namespace RememberWhen.Lambda.Tests
{
    public class AWSEnvironmentManagementServiceTests
    {
        private IEnvironmentManagementService _sut;

        [Fact]
        public void EnvironmentManagementService_EnsureRegionIsUSWest2()
        {
            MockDevEnvironment();
            _sut = new AWSEnvironmentManagementService();

            Assert.Equal(RegionEndpoint.USWest2, _sut.AWSRegion);
        }

        [Fact]
        public void EnvironmentManagementService_GivenDevEnvironment_ReturnsCorrectEnvironmentType()
        {
            MockDevEnvironment();
            _sut = new AWSEnvironmentManagementService();

            Assert.Equal(Constants.DevEnvironmentName, _sut.RetrieveEnvironmentName());
            Assert.Equal(Environments.Dev, _sut.EnvironmentType);
        }

        [Fact]
        public void EnvironmentManagementService_GivenProdEnvironment_ReturnsCorrectEnvironmentType()
        {
            MockProdEnvironment();
            _sut = new AWSEnvironmentManagementService();

            Assert.Equal(Constants.ProdEnvironmentName, _sut.RetrieveEnvironmentName());
            Assert.Equal(Environments.Production, _sut.EnvironmentType);
        }

        [Fact]
        public void EnvironmentManagementService_GivenUnknownEnvironment_Throws()
        {
            Environment.SetEnvironmentVariable(Constants.AWSEnvironmentEnvironmentVariableName, "jibberish!");

            Assert.Throws<ArgumentException>(() =>
            {
                _sut = new AWSEnvironmentManagementService();
            });
        }

        private void MockDevEnvironment()
        {
            Environment.SetEnvironmentVariable(Constants.AWSEnvironmentEnvironmentVariableName, Constants.DevEnvironmentName);
        }

        private void MockProdEnvironment()
        {
            Environment.SetEnvironmentVariable(Constants.AWSEnvironmentEnvironmentVariableName, Constants.ProdEnvironmentName);
        }
    }
}
