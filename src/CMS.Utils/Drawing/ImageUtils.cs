using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using CMS.Utils.Drawing.Imaging;
using System.Drawing.Imaging;

namespace CMS.Utils.Drawing
{
    public class ImageUtils
    {
        public static Stream Resize(int width, int height, Stream image, string fileName)
        {
            Stream fileStream = new MemoryStream();
            Image tempImage = Bitmap.FromStream(image);
            long maxFactor = width * height;
            long imageFactor = tempImage.Width * tempImage.Height;

            if (maxFactor < imageFactor)
            {
                using (ImageResizer resizer = new ImageResizer())
                {
                    resizer.MimeType = Path.GetExtension(fileName);
                    resizer.SourceImage = tempImage;
                    resizer.Background = ColorTranslator.FromHtml("#fff");
                    resizer.Mode = ImageResizer.ResizeMode.KeepOriginalAspect;
                    resizer.Width = width;
                    resizer.Height = height;

                    resizer.Process();

                    resizer.ResultImage.Save(fileStream, tempImage.RawFormat);
                }
            }
            else
            {
                image.Seek(0, SeekOrigin.Begin);
                image.CopyTo(fileStream);
            }

            return fileStream;
        }
    }
}
