using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace NFT.Storage.Net.UnitTests
{
    internal class GUID_Image
    {
        internal static void GenerateGuidImage(string path, bool overwrite = true)
        {
            Bitmap bitmap = GenerateGuidImage();
            if (overwrite)
            {
                if (File.Exists(path)) File.Delete(path);
            }
            bitmap.Save(path);
        }
        internal static Bitmap BrandImage(Bitmap image)
        {
            Bitmap uuid = GenerateGuidImage();
            Bitmap result = (Bitmap) image.Clone();
            result.SetPixel(0, 0, uuid.GetPixel(0, 0));
            result.SetPixel(0, 1, uuid.GetPixel(0, 1));
            result.SetPixel(1, 0, uuid.GetPixel(1, 0));
            result.SetPixel(1, 1, uuid.GetPixel(1, 1));
            return result;
        }
        internal static Bitmap LargeImage()
        {
            Bitmap uuid = GenerateGuidImage();
            int scale = 1000;
            Bitmap result = new Bitmap(scale, scale);
            Random random = new Random();
            for (int width = 0; width < scale; width++)
            {
                for (int height = 0; height < scale; height++)
                {
                    Color randomColor = Color.FromArgb(255,random.Next(255), random.Next(255), random.Next(255));
                    result.SetPixel(width, height, randomColor);
                }
            }
            result.SetPixel(0, 0, uuid.GetPixel(0, 0));
            result.SetPixel(0, 1, uuid.GetPixel(0, 1));
            result.SetPixel(1, 0, uuid.GetPixel(1, 0));
            result.SetPixel(1, 1, uuid.GetPixel(1, 1));
            return result;
        }
        internal static Bitmap GenerateGuidImage()
        {
            Guid myuuid = Guid.NewGuid();
            byte[] bytes = myuuid.ToByteArray();
            List<Color> pixles = new List<Color>();
            for (int i = 0; i < bytes.Length; i+=4)
            {
                Color color = Color.FromArgb(
                alpha: Convert.ToInt32(bytes[i]),
                red: Convert.ToInt32(bytes[i+1]),
                green: Convert.ToInt32(bytes[i+2]),
                blue: Convert.ToInt32(bytes[i+3]));
                pixles.Add(color);
            }
            Bitmap bmp = new Bitmap(2,2);
            bmp.SetPixel(0, 0, pixles[0]);
            bmp.SetPixel(0, 1, pixles[1]);
            bmp.SetPixel(1, 0, pixles[2]);
            bmp.SetPixel(1, 1, pixles[3]);
            return bmp;
        }
        internal static Guid ReadGuidFromImage(Bitmap bmp)
        {
            if (bmp.Width + bmp.Height != 4)
            {
                throw new ArgumentException("Bitmap must have 4 Pixles!");
            }
            List<byte> bytes = new List<byte>();
            for(int height = 0; height < bmp.Height; height++)
            {
                for(int width = 0; width < bmp.Width; width++)
                {
                    Color c = bmp.GetPixel(width, height);
                    bytes.Add(c.A);
                    bytes.Add(c.R);
                    bytes.Add(c.G);
                    bytes.Add(c.B);
                }
            }
            Guid guid = new Guid(bytes.ToArray());
            return guid;
        }
    }
}
