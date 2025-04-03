using System.IO;
using NUnit.Framework;
using TinyBCSharp;

namespace TinyBCSharpTests;

public class BC2DecoderTest
{
    [Test]
    public void TestBC2()
    {
        var decoder = BlockDecoder.Create(BlockFormat.BC2);
        var src = File.ReadAllBytes("images/bc2.dds")[BCTestUtils.DdsHeaderSize..];
        var actual = decoder.Decode(256, 256, src);
        var expected = BCTestUtils.ReadPng("images/bc2.png");
        Assert.That(actual, Is.EqualTo(expected));
    }
}