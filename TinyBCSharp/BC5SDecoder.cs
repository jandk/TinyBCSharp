using System;

namespace TinyBCDec
{
    internal class BC5SDecoder : BlockDecoder
    {
        private const int BytesPerPixel = 3;

        private readonly BC4SDecoder _decoder = new BC4SDecoder(BytesPerPixel);
        private readonly bool _reconstructZ;

        public BC5SDecoder(bool reconstructZ)
            : base(reconstructZ ? BlockFormat.BC5SReconstructZ : BlockFormat.BC5S, BytesPerPixel)
        {
            _reconstructZ = reconstructZ;
        }

        public override void DecodeBlock(ReadOnlySpan<byte> src, Span<byte> dst, int stride)
        {
            _decoder.DecodeBlock(src, dst, stride);
            _decoder.DecodeBlock(src[8..], dst[1..], stride);

            if (_reconstructZ)
            {
                ReconstructZ.Reconstruct(dst, stride, BytesPerPixel);
            }
        }
    }
}