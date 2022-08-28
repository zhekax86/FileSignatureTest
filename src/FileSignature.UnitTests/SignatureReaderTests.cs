using FileSignature.Logic.Internal;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSignature.UnitTests
{
    [TestFixture]
    public class SignatureReaderTests
    {
        [Test]
        [TestCase(1024, 1024, 1)]
        [TestCase(1024, 1025, 2)]
        [TestCase(4096, 0, 0)]
        public void Test(int blockSize, long fileSize, long blocksCount)
        {
            var reader = new SignatureReader(string.Empty, blockSize);

            var actualBlocksCount = reader.CalcBlocksCount(fileSize);

            actualBlocksCount.Should().Be(blocksCount);
        }
    }
}
