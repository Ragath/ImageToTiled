using System.Drawing;
using TiledLib;

namespace ImageToTiled
{
    static class ImageProcessing
    {
        public static Rectangle GetRectangle(this Tile tile)
              => new Rectangle(tile.Left, tile.Top, tile.Width, tile.Height);

        static System.Security.Cryptography.SHA1 sha1 { get; } = System.Security.Cryptography.SHA1.Create();
        public static byte[] GetTileHash(this Bitmap bmp, Rectangle rect)
        {
            var format = System.Drawing.Imaging.PixelFormat.Format32bppArgb; //bmp.PixelFormat;

            //var bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, format);

            // Declare an array to hold the bytes of the bitmap.
            var data = new byte[Image.GetPixelFormatSize(format) / 8 * rect.Width * rect.Height];
            for (int y = rect.Top, i = 0; y < rect.Bottom; y++)
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    var c = bmp.GetPixel(x, y);
                    data[i++] = c.A;
                    if (c.A == 0)
                    {
                        data[i++] = 0;
                        data[i++] = 0;
                        data[i++] = 0;
                    }
                    else
                    {
                        data[i++] = c.R;
                        data[i++] = c.G;
                        data[i++] = c.B;
                    }
                }
            //Marshal.Copy(bmpData.Scan0, data, 0, data.Length);

            var hash = sha1.ComputeHash(data);

            //bmp.UnlockBits(bmpData);

            return hash;
        }
    }
}
