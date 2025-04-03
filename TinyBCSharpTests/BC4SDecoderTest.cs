using System.IO;
using NUnit.Framework;
using TinyBCSharp;

namespace TinyBCSharpTests;

public class BC4SDecoderTest
{
    [Test]
    public void TestBC4S()
    {
        var decoder = BlockDecoder.Create(BlockFormat.BC4S);
        var src = File.ReadAllBytes("images/bc4s.dds")[BCTestUtils.DdsHeaderSize..];
        var actual = decoder.Decode(256, 256, src);
        var expected = BCTestUtils.ReadPng("images/bc4s.png");
        Assert.That(actual, Is.EqualTo(expected));
    }
}