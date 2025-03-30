namespace TinyBCDec;

internal class BC5SDecoder(bool reconstructZ)
    : BlockDecoder(reconstructZ ? BlockFormat.BC5SReconstructZ : BlockFormat.BC5S, BPP)
{
    private const int BPP = 3;

    private readonly BC4SDecoder _decoder = new(BPP);

    public override void DecodeBlock(ReadOnlySpan<byte> src, Span<byte> dst, int stride)
    {
        _decoder.DecodeBlock(src, dst, stride);
        _decoder.DecodeBlock(src[8..], dst[1..], stride);

        if (reconstructZ)
        {
            ReconstructZ.Reconstruct(dst, stride, BPP);
        }
    }
}