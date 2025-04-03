namespace TinyBCSharp;

internal class BC5UDecoder : BC5Decoder
{
    public BC5UDecoder(bool reconstructZ)
        : base(new BC4UDecoder(false), reconstructZ)
    {
    }
}