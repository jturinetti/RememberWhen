using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

namespace RememberWhen.Lambda.Services
{
    public interface IParameterManagementService
    {
        Task<IDictionary<string, string>> RetrieveParameters(IEnumerable<string> keys);
    }

    public class SSMParameterManagementService : IParameterManagementService
    {
        private readonly IEnvironmentManagementService _environmentManagementService;
        private readonly IDictionary<string, string> _parameterDictionary;

        private bool _hasLoadedParameters;

        public SSMParameterManagementService(IEnvironmentManagementService environmentManagementService)
        {
            _environmentManagementService = environmentManagementService;

            _hasLoadedParameters = false;
            _parameterDictionary = new Dictionary<string, string>();
        }

        public async Task<IDictionary<string, string>> RetrieveParameters(IEnumerable<string> keys)
        {
            if (_hasLoadedParameters)
            {
                return _parameterDictionary;
            }

            try
            {
                using (var ssm = new AmazonSimpleSystemsManagementClient(_environmentManagementService.AWSRegion))
                {
                    foreach (var key in keys)
                    {
                        var response = await ssm.GetParameterAsync(new GetParameterRequest
                        {
                            Name = key,
                            WithDecryption = true
                        });

                        _parameterDictionary.Add(key, response.Parameter.Value);
                    }
                }

                _hasLoadedParameters = true;
                return _parameterDictionary;
            }
            catch (Exception ex)
            {
                _parameterDictionary.Clear();
                _hasLoadedParameters = false;
                throw ex;
            }
        }
    }
}
