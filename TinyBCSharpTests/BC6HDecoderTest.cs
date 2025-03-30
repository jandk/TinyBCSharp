using TinyBCDec;

namespace TinyBCDecTests;

public class BC6HDecoderTest
{
    [Test]
    public void TestBC6HUf16()
    {
        var src = File.ReadAllBytes("images/bc6h_uf16.dds")[BCTestUtils.DdsHeaderSize..];
        var actual = BlockDecoder.Create(BlockFormat.BC6HUf16).Decode(256, 256, src);
        var expected = BCTestUtils.ReadDdsFp16("images/bc6h_uf16_16.dds", 256, 256);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void TestBC6HSf16()
    {
        var src = File.ReadAllBytes("images/bc6h_sf16.dds")[BCTestUtils.DdsHeaderSize..];
        var actual = BlockDecoder.Create(BlockFormat.BC6HSf16).Decode(256, 256, src);
        var expected = BCTestUtils.ReadDdsFp16("images/bc6h_sf16_16.dds", 256, 256);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void TestBC6HUf32()
    {
        var src = File.ReadAllBytes("images/bc6h_uf16.dds")[BCTestUtils.DdsHeaderSize..];
        var actual = BlockDecoder.Create(BlockFormat.BC6HUf32).Decode(256, 256, src);
        var expected = BCTestUtils.ReadDdsFp32("images/bc6h_uf16_32.dds", 256, 256);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void TestBC6HSf32()
    {
        var src = File.ReadAllBytes("images/bc6h_sf16.dds")[BCTestUtils.DdsHeaderSize..];
        var actual = BlockDecoder.Create(BlockFormat.BC6HSf32).Decode(256, 256, src);
        var expected = BCTestUtils.ReadDdsFp32("images/bc6h_sf16_32.dds", 256, 256);
        Assert.That(actual, Is.EqualTo(expected));
    }

    // [Test]
    // public void TestBC6HInvalidBlock()
    // {
    //     var src = new byte[16];
    //     var actual = BlockDecoder.Create(BlockFormat.BC6H).Decode(4, 4, src);
    //     var expected = new byte[16 * 4];
    //     Assert.That(actual, Is.EqualTo(expected));
    // }
}