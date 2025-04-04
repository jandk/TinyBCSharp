using NUnit.Framework;

namespace TinyBCSharp.Tests;

public class BlockDecoderTest
{
    [Test]
    public void TestPartialBlock()
    {
        var src = File.ReadAllBytes("images/bc4u-part.dds")[BCTestUtils.DdsHeaderSize..];

        var actual = BlockDecoder.Create(BlockFormat.BC4U)
            .Decode(157, 119, src);
        var expected = BCTestUtils.ReadPng("images/bc4u-part.png");

        Assert.That(actual, Is.EqualTo(expected));
    }
}