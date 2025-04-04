using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TinyBCSharpTests;

public static class BCTestUtils
{
    public const int DdsHeaderSize = 148;

    public static byte[] ReadPng(string path)
    {
        using var image = Image.Load(path);
        var pixelArray = new byte[image.Width * image.Height * 4];
        image
            .CloneAs<Rgba32>()
            .CopyPixelDataTo(pixelArray);
        return pixelArray;
    }

    public static byte[] ReadDdsFp16(string path, int width, int height)
    {
        return File.ReadAllBytes(path)
            .AsSpan(DdsHeaderSize, width * height * 8)
            .ToArray();
    }

    public static byte[] ReadDdsFp32(string path, int width, int height)
    {
        return File.ReadAllBytes(path)
            .AsSpan(DdsHeaderSize, width * height * 16)
            .ToArray();
    }
}