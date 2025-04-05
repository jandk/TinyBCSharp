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

    [Test]
    public void TestPartialBlockCrop()
    {
        var src = File.ReadAllBytes("images/bc4u-part.dds")[BCTestUtils.DdsHeaderSize..];
        var expected = BCTestUtils.ReadPng("images/bc4u-part.png");

        const int srcWidth = 157;
        const int srcHeight = 119;

        var dst = new byte[8 * 8 * 4];
        var decoder = BlockDecoder.Create(BlockFormat.BC4U);
        for (var h = 1; h <= 8; h++)
        {
            for (var w = 1; w <= 8; w++)
            {
                decoder.Decode(src, srcWidth, srcHeight, dst, w, h);

                for (var y = 0; y < h; y++)
                {
                    for (var x = 0; x < w; x++)
                    {
                        for (var c = 0; c < 4; c++)
                        {
                            Assert.That(dst[(y * w + x) * 4 + c], Is.EqualTo(expected[(y * srcWidth + x) * 4 + c]));
                        }
                    }
                }
            }
        }
    }
}