using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace TinyBCSharpTests;

public class BCTestUtils
{
    public const int DdsHeaderSize = 148;

    public static byte[] ReadPng(string path)
    {
        var bitmap = new Bitmap(path);
        var bitmapData = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb
        );

        var numPixels = bitmapData.Width * bitmapData.Height;
        var rawImage = new byte[numPixels * 4];
        Marshal.Copy(bitmapData.Scan0, rawImage, 0, rawImage.Length);
        bitmap.UnlockBits(bitmapData);

        for (var i = 0; i < rawImage.Length; i += 4)
        {
            (rawImage[i], rawImage[i + 2]) = (rawImage[i + 2], rawImage[i]);
        }

        return rawImage;
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