using FileSignature.Logic.Internal;
using FluentAssertions;

namespace FileSignature.UnitTests
{
    [TestFixture]
    public class SignatureGeneratorTests
    {
        [Test]
        [TestCase(1024, 1024, 1)]
        [TestCase(1024, 1025, 2)]
        [TestCase(4096, 0, 0)]
        public void Test(int blockSize, long fileSize, long blocksCount)
        {
            var reader = new SignatureGenerator(string.Empty, blockSize);

            var actualBlocksCount = reader.CalcBlocksCount(fileSize);

            actualBlocksCount.Should().Be(blocksCount);
        }
    }
}
