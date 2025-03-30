using System;

namespace TinyBCSharp
{
    internal class BC5UDecoder : BlockDecoder
    {
        private const int BytesPerPixel = 3;

        private readonly BC4UDecoder _decoder = new BC4UDecoder(BytesPerPixel);
        private readonly bool _reconstructZ;

        public BC5UDecoder(bool reconstructZ)
            : base(reconstructZ ? BlockFormat.BC5UReconstructZ : BlockFormat.BC5U, BytesPerPixel)
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