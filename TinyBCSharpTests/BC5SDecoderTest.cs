using System;
using System.IO;
using NUnit.Framework;
using TinyBCDec;

namespace TinyBCSharpTests
{
    public class BC5SDecoderTest
    {
        [Test]
        public void TestBC5S()
        {
            var decoder = BlockDecoder.Create(BlockFormat.BC5S);
            var src = File.ReadAllBytes("images/bc5s.dds")[BCTestUtils.DdsHeaderSize..];
            var actual = decoder.Decode(256, 256, src);
            var expected = BCTestUtils.ReadPng("images/bc5s.png", 3);

            for (var i = 0; i < expected.Length; i += 3)
            {
                Assert.That(Math.Abs(actual[i + 0] - expected[i + 0]), Is.LessThanOrEqualTo(1));
                Assert.That(Math.Abs(actual[i + 1] - expected[i + 1]), Is.LessThanOrEqualTo(1));
                Assert.That(actual[i + 2], Is.Zero);
            }
        }

        [Test]
        public void TestBC5SReconstructZ()
        {
            var decoder = BlockDecoder.Create(BlockFormat.BC5SReconstructZ);
            var src = File.ReadAllBytes("images/bc5s.dds")[BCTestUtils.DdsHeaderSize..];
            var actual = decoder.Decode(256, 256, src);
            var expected = BCTestUtils.ReadPng("images/bc5s_reconstructed.png", 3);

            for (var i = 0; i < expected.Length; i += 3)
            {
                Assert.That(Math.Abs(actual[i + 0] - expected[i + 0]), Is.LessThanOrEqualTo(1));
                Assert.That(Math.Abs(actual[i + 1] - expected[i + 1]), Is.LessThanOrEqualTo(1));
                if (expected[i + 2] != 0)
                {
                    // texconv sets the channel to 0 outside of range, while I clamp, so I need to do the same
                    Assert.That(Math.Abs((actual[i + 2] & 0xFF) - (expected[i + 2] & 0xFF)), Is.LessThanOrEqualTo(1));
                }
            }
        }
    }
}