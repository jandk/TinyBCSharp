﻿using System;

namespace TinyBCDec
{
    internal class BC3Decoder : BlockDecoder
    {
        private const int BytesPerPixel = 4;

        private readonly BC1Decoder _colorDecoder = new BC1Decoder(BC1Mode.BC2Or3);
        private readonly BC4UDecoder _alphaDecoder = new BC4UDecoder(BytesPerPixel);

        public BC3Decoder()
            : base(BlockFormat.BC3, 4)
        {
        }

        public override void DecodeBlock(ReadOnlySpan<byte> src, Span<byte> dst, int stride)
        {
            _colorDecoder.DecodeBlock(src[8..], dst, stride);
            _alphaDecoder.DecodeBlock(src, dst[3..], stride);
        }
    }
}