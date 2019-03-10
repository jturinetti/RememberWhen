using RememberWhen.Lambda.Services;
using Xunit;

namespace RememberWhen.Lambda.Tests
{
    public class StaticMemoryServiceTests
    {
        private readonly IMemoryService _sut;

        public StaticMemoryServiceTests()
        {
            _sut = new StaticMemoryService();
        }

        [Fact]
        public void RetrieveRandomMemory_ReturnsPopulatedMemoryString()
        {
            var result = _sut.RetrieveRandomMemory();

            Assert.False(string.IsNullOrEmpty(result));
        }
    }
}
