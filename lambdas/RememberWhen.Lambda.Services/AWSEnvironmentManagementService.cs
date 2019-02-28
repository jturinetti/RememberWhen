using System;
using Amazon;

namespace RememberWhen.Lambda.Services
{
    public enum Environments
    {
        Dev = 0,
        Production = 1
    }

    public interface IEnvironmentManagementService
    {
        string RetrieveEnvironmentName();

        Environments EnvironmentType { get; }

        RegionEndpoint AWSRegion { get; }
    }

    public class AWSEnvironmentManagementService : IEnvironmentManagementService
    {
        const string DevEnvironmentName = "dev";
        const string ProdEnvironmentName = "prod";
        const string AWSEnvironmentEnvironmentVariableName = "STAGE";

        public AWSEnvironmentManagementService()
        {
            var environment = RetrieveEnvironmentName();
            if (string.Compare(environment, DevEnvironmentName, true) == 0)
            {
                EnvironmentType = Environments.Dev;
            }
            else if (string.Compare(environment, ProdEnvironmentName, true) == 0)
            {
                EnvironmentType = Environments.Production;
            }
            else
            {
                throw new ArgumentException("Environment string not recognized.");
            }
        }

        public string RetrieveEnvironmentName()
        {
            return Environment.GetEnvironmentVariable(AWSEnvironmentEnvironmentVariableName);
        }

        public Environments EnvironmentType { get; }

        public RegionEndpoint AWSRegion => RegionEndpoint.USWest2;
    }
}
