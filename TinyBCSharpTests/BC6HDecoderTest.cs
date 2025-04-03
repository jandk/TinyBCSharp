using System;
using System.Buffers.Binary;
using System.IO;
using NUnit.Framework;
using TinyBCSharp;

namespace TinyBCSharpTests;

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

    [Test]
    public void TestBC6HInvalidBlockF16()
    {
        byte[] invalidModes = { 0b10011, 0b10111, 0b11011, 0b11111 };

        var expected = new byte[16 * 8];
        var span = expected.AsSpan()[6..];
        for (var i = 0; i < expected.Length; i += 8)
        {
            BinaryPrimitives.WriteInt16LittleEndian(span[i..], 0x3C00);
        }

        var src = new byte[16];
        foreach (var invalidMode in invalidModes)
        {
            src[0] = invalidMode;
            var actual = BlockDecoder.Create(BlockFormat.BC6HUf16).Decode(4, 4, src);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }

    [Test]
    public void TestBC6HInvalidBlockF32()
    {
        byte[] invalidModes = { 0b10011, 0b10111, 0b11011, 0b11111 };

        var expected = new byte[16 * 16];
        var span = expected.AsSpan()[12..];
        for (var i = 0; i < expected.Length; i += 16)
        {
            BinaryPrimitives.WriteInt32LittleEndian(span[i..], 0x3F800000);
        }

        var src = new byte[16];
        foreach (var invalidMode in invalidModes)
        {
            src[0] = invalidMode;
            var actual = BlockDecoder.Create(BlockFormat.BC6HUf32).Decode(4, 4, src);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}