using NUnit.Framework;

namespace TinyBCSharp.Tests;

public class BC7DecoderTest
{
    private readonly BlockDecoder _decoder = BlockDecoder.Create(BlockFormat.BC7);

    [Test]
    public void TestBC7()
    {
        var src = File.ReadAllBytes("images/bc7.dds")[BCTestUtils.DdsHeaderSize..];
        var actual = _decoder.Decode(256, 256, src);
        var expected = BCTestUtils.ReadPng("images/bc7.png");
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void TestBC7InvalidBlock()
    {
        var src = new byte[16];
        var actual = _decoder.Decode(4, 4, src);
        var expected = new byte[16 * 4];
        Assert.That(actual, Is.EqualTo(expected));
    }
}