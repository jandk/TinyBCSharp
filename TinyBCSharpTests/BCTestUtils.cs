using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace TinyBCDecTests;

public class BCTestUtils
{
    public const int DdsHeaderSize = 148;

    public static byte[] ReadPng(String path, int channels)
    {
        var bitmap = new Bitmap(path);
        var bitmapData = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb
        );

        var numPixels = bitmapData.Width * bitmapData.Height;
        byte[] rawImage = new byte[numPixels * 4];
        Marshal.Copy(bitmapData.Scan0, rawImage, 0, rawImage.Length);
        bitmap.UnlockBits(bitmapData);

        switch (channels)
        {
            case 1:
            {
                var dst = new byte[numPixels];
                for (int i = 0, o = 0; i < rawImage.Length; i += 4, o++)
                {
                    dst[o] = rawImage[i];
                }

                return dst;
            }
            case 3:
            {
                var dst = new byte[numPixels * 3];
                for (int i = 0, o = 0; i < rawImage.Length; i += 4, o += 3)
                {
                    dst[o + 0] = rawImage[i + 2];
                    dst[o + 1] = rawImage[i + 1];
                    dst[o + 2] = rawImage[i + 0];
                }

                return dst;
            }
            case 4:
            {
                for (var i = 0; i < rawImage.Length; i += 4)
                {
                    var b = rawImage[i + 0];
                    var r = rawImage[i + 2];
                    rawImage[i + 0] = r;
                    rawImage[i + 2] = b;
                }

                return rawImage;
            }
            default:
                throw new Exception();
        }
    }
}