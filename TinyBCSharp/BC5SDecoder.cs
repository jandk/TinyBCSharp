namespace TinyBCSharp
{
    internal class BC5SDecoder : BC5Decoder
    {
        public BC5SDecoder(bool reconstructZ)
            : base(new BC4SDecoder(false), reconstructZ)
        {
        }
    }
}