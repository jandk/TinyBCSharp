using System.IO;
using NUnit.Framework;
using TinyBCSharp;

namespace TinyBCSharpTests
{
    public class BC5UDecoderTest
    {
        [Test]
        public void TestBC5U()
        {
            var decoder = BlockDecoder.Create(BlockFormat.BC5U);
            var src = File.ReadAllBytes("images/bc5u.dds")[BCTestUtils.DdsHeaderSize..];
            var actual = decoder.Decode(256, 256, src);
            var expected = BCTestUtils.ReadPng("images/bc5u.png", 3);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void TestBC5UReconstructZ()
        {
            var decoder = BlockDecoder.Create(BlockFormat.BC5UReconstructZ);
            var src = File.ReadAllBytes("images/bc5u.dds")[BCTestUtils.DdsHeaderSize..];
            var actual = decoder.Decode(256, 256, src);
            var expected = BCTestUtils.ReadPng("images/bc5u_reconstructed.png", 3);

            for (var i = 0; i < expected.Length; i += 3)
            {
                Assert.That(actual[i + 0], Is.EqualTo(expected[i + 0]));
                Assert.That(actual[i + 1], Is.EqualTo(expected[i + 1]));
                if (expected[i + 2] != 0)
                {
                    // texconv sets the channel to 0 outside of range, while I clamp, so I need to do the same
                    Assert.That(actual[i + 2], Is.EqualTo(expected[i + 2]));
                }
            }
        }
    }
}