using System;
using System.Buffers.Binary;

namespace TinyBCSharp;

abstract class BC4Decoder(bool grayscale) : BlockDecoder(8, BytesPerPixel)
{
    const int BytesPerPixel = 4;

    readonly bool _grayscale = grayscale;

    protected void Write(Span<byte> dst, int stride, Span<byte> alphas, long indices)
    {
        if (_grayscale)
            WriteRGBA(dst, stride, alphas, indices);
        else
            WriteR(dst, stride, alphas, indices);
    }

    static void WriteR(Span<byte> dst, int stride, Span<byte> alphas, long indices)
    {
        for (var y = 0; y < BlockHeight; y++)
        {
            var dstPos = y * stride;
            for (var x = 0; x < BlockWidth; x++)
            {
                var alpha = alphas[(int)(indices & 7)];
                dst[dstPos + x * BytesPerPixel] = alpha;
                indices >>= 3;
            }
        }
    }

    static void WriteRGBA(Span<byte> dst, int stride, Span<byte> alphas, long indices)
    {
        for (var y = 0; y < BlockHeight; y++)
        {
            var dstPos = y * stride;
            for (var x = 0; x < BlockWidth; x++)
            {
                var alpha = alphas[(int)(indices & 7)];
                var color = (alpha * 0x010101) | unchecked((int)0xFF000000);
                var index = dstPos + x * BytesPerPixel;
                BinaryPrimitives.WriteInt32LittleEndian(dst[index..], color);
                indices >>= 3;
            }
        }
    }
}