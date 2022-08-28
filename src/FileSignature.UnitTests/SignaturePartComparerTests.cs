using FileSignature.Logic;
using FileSignature.Logic.Internal.Models;
using FluentAssertions;

namespace FileSignature.UnitTests
{
    [TestFixture]
    public class SignaturePartComparerTests
    {
        [Test]
        public void FirstLess()
        {
            var x = new SignaturePart
            {
                PartNumber = 10
            };

            var y = new SignaturePart
            {
                PartNumber = 20
            };

            var result = new SignaturePartComparer().Compare(x, y);

            result.Should().BeLessThan(0);
        }

        [Test]
        public void FirstGreater()
        {
            var x = new SignaturePart
            {
                PartNumber = 21
            };

            var y = new SignaturePart
            {
                PartNumber = 20
            };

            var result = new SignaturePartComparer().Compare(x, y);

            result.Should().BeGreaterThan(0);
        }

        [Test]
        public void BothAreEqual()
        {
            var x = new SignaturePart
            {
                PartNumber = 30
            };

            var y = new SignaturePart
            {
                PartNumber = 30
            };

            var result = new SignaturePartComparer().Compare(x, y);

            result.Should().Be(0);
        }
    }
}
