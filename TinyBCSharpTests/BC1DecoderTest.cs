using System.IO;
using NUnit.Framework;
using TinyBCSharp;

namespace TinyBCSharpTests;

public class BC1DecoderTest
{
    [Test]
    public void TestBC1()
    {
        var decoder = BlockDecoder.Create(BlockFormat.BC1);
        var src = File.ReadAllBytes("images/bc1a.dds")[BCTestUtils.DdsHeaderSize..];
        var actual = decoder.Decode(256, 256, src);
        var expected = BCTestUtils.ReadPng("images/bc1a.png");
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void TestBC1NoAlpha()
    {
        var decoder = BlockDecoder.Create(BlockFormat.BC1);
        var src = File.ReadAllBytes("images/bc1.dds")[BCTestUtils.DdsHeaderSize..];
        var actual = decoder.Decode(256, 256, src);
        var expected = BCTestUtils.ReadPng("images/bc1.png");
        Assert.That(actual, Is.EqualTo(expected));
    }
}