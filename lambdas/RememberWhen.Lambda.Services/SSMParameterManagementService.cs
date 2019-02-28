using System;
using System.Collections.Generic;
using System.Linq;
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

        public SSMParameterManagementService(IEnvironmentManagementService environmentManagementService)
        {
            _environmentManagementService = environmentManagementService;
            
            _parameterDictionary = new Dictionary<string, string>();
        }

        public async Task<IDictionary<string, string>> RetrieveParameters(IEnumerable<string> keys)
        {
            var needToLoadFromAWS = false;

            // check for existence of keys in dictionary already
            foreach (var key in keys)
            {
                needToLoadFromAWS |= !_parameterDictionary.ContainsKey(key);
            }

            // if any parameters need to be loaded, initialize SSM client and retrieve parameters from AWS
            if (needToLoadFromAWS)
            {
                try
                {
                    using (var ssm = new AmazonSimpleSystemsManagementClient(_environmentManagementService.AWSRegion))
                    {
                        foreach (var key in keys)
                        {
                            // check again, as we might only be loading a subset of the requested parameters
                            if (!_parameterDictionary.ContainsKey(key))
                            {
                                var response = await ssm.GetParameterAsync(new GetParameterRequest
                                {
                                    Name = key,
                                    WithDecryption = true
                                });

                                _parameterDictionary.Add(key, response.Parameter.Value);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _parameterDictionary.Clear();
                    throw ex;
                }
            }

            // select dictionary subset based on what's being requested and return it
            return _parameterDictionary.Where(pd => keys.Contains(pd.Key))
                .ToDictionary(dict => dict.Key, dict => dict.Value);
        }
    }
}
