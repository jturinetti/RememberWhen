using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Moq;
using RememberWhen.Lambda.Services;
using Xunit;

namespace RememberWhen.Lambda.Tests
{
    public class SSMParameterManagementServiceTests
    {
        private readonly IParameterManagementService _sut;
        private readonly Mock<IAmazonSimpleSystemsManagement> _ssmMock;

        public SSMParameterManagementServiceTests()
        {
            _ssmMock = new Mock<IAmazonSimpleSystemsManagement>();
            _ssmMock.Setup(m => m.GetParameterAsync(
                It.Is<GetParameterRequest>(x => x.WithDecryption == true), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetParameterRequest req, CancellationToken t) => new GetParameterResponse
                {
                    HttpStatusCode = HttpStatusCode.OK,
                    Parameter = new Parameter
                    {
                        Name = req.Name,
                        Value = req.Name + "Value"
                    }
                });

            _sut = new SSMParameterManagementService(_ssmMock.Object);
        }

        [Fact]
        public async Task RetrieveParameters_GivenEmptyDictionary_MakesExpectedCallsAndReturnsParameters()
        {
            var parameterKeyList = new List<string> { "key1", "key2", "key3" };

            var result = await _sut.RetrieveParameters(parameterKeyList);

            _ssmMock.Verify(m => m.GetParameterAsync(
                It.IsAny<GetParameterRequest>(), 
                It.IsAny<CancellationToken>()), 
                Times.Exactly(parameterKeyList.Count));

            foreach (var key in parameterKeyList)
            {
                Assert.Equal(key + "Value", result[key]);
            }
        }

        [Fact]
        public async Task RetrieveParameters_GivenPopulatedDictionary_AndRequestForSameParameters_MakesNoAdditionalCallsAndReturnsParameters()
        {
            var parameterKeyList = new List<string> { "key1", "key2", "key3" };

            await _sut.RetrieveParameters(parameterKeyList);

            // call again
            var result = await _sut.RetrieveParameters(parameterKeyList);

            _ssmMock.Verify(m => m.GetParameterAsync(
                It.IsAny<GetParameterRequest>(),
                It.IsAny<CancellationToken>()),
                Times.Exactly(parameterKeyList.Count));

            foreach (var key in parameterKeyList)
            {
                Assert.Equal(key + "Value", result[key]);
            }
        }

        [Fact]
        public async Task RetrieveParameters_GivenPopulatedDictionary_AndRequestForSomeNewParameters_MakesCallsForMissingParametersAndReturnsParameters()
        {
            var parameterKeyList1 = new List<string> { "key1", "key2", "key3" };
            var parameterKeyList2 = new List<string> { "key2", "key3", "key4", "key5" };

            var result1 = await _sut.RetrieveParameters(parameterKeyList1);

            // call again with new parameter list
            var result2 = await _sut.RetrieveParameters(parameterKeyList2);

            _ssmMock.Verify(m => m.GetParameterAsync(
                It.IsAny<GetParameterRequest>(),
                It.IsAny<CancellationToken>()),
                Times.Exactly(5));

            Assert.Equal(3, result1.Keys.Count);
            Assert.Equal(4, result2.Keys.Count);

            foreach (var key in parameterKeyList1)
            {
                Assert.Equal(key + "Value", result1[key]);
            }

            foreach (var key in parameterKeyList2)
            {
                Assert.Equal(key + "Value", result2[key]);
            }
        }
    }
}
