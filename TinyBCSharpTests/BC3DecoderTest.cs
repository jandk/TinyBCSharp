using TinyBCDec;

namespace TinyBCDecTests;

public class BC3DecoderTest
{
    [Test]
    public void TestBC3()
    {
        var decoder = BlockDecoder.Create(BlockFormat.BC3);
        var src = File.ReadAllBytes("images/bc3.dds")[BCTestUtils.DdsHeaderSize..];
        var actual = decoder.Decode(256, 256, src);
        var expected = BCTestUtils.ReadPng("images/bc3.png", 4);
        Assert.That(actual, Is.EqualTo(expected));
    }
}