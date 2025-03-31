using System.IO;
using NUnit.Framework;
using TinyBCSharp;

namespace TinyBCSharpTests
{
    public class BC4UDecoderTest
    {
        [Test]
        public void TestBC4U()
        {
            var decoder = BlockDecoder.Create(BlockFormat.BC4U);
            var src = File.ReadAllBytes("images/bc4u.dds")[BCTestUtils.DdsHeaderSize..];
            var actual = decoder.Decode(256, 256, src);
            var expected = BCTestUtils.ReadPng("images/bc4u.png");
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}