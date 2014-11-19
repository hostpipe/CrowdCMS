using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Utils.Drawing.Imaging
{
    public class ImageResizer : ImageProcessorBase
    {
        private ResizeMode _mode = ResizeMode.KeepOriginalAspect;
        private int _width = 0;
        private int _height = 0;
        private Color _background = Color.Black;
        private Rectangle _selectionArea;

        /// <summary>
        /// Gets or sets the resize mode.
        /// </summary>
        /// <value>The resize mode.</value>
        public ResizeMode Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        /// <summary>
        /// Gets or sets the target image width.
        /// </summary>
        /// <value>Target image width in pixels.</value>
        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        /// <summary>
        /// Gets or sets the target image height.
        /// </summary>
        /// <value>Target image height in pixels.</value>
        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }

        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        /// <value>The background color.</value>
        public Color Background
        {
            get { return _background; }
            set { _background = value; }
        }

        public Rectangle SelectionArea
        {
            get { return _selectionArea; }
            set { _selectionArea = value; }
        }


        private bool CropImageEnabled
        {
            get { return SelectionArea != null && SelectionArea.Width != 0 && SelectionArea.Height != 0; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ImageResizer"/> class.
        /// </summary>
        /// <param name="sourceImage">The source image file path.</param>
        public ImageResizer(string sourceImage)
            : base(sourceImage)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageResizer"/> class.
        /// </summary>
        public ImageResizer()
            : base()
        {
        }

        /// <summary>
        /// Generates resized result.
        /// </summary>
        public override void Process()
        {
            SourceImage = CropImage(SourceImage);
            Size resultSize = CalculateResultImageSize();
            Rectangle resultPosition = CalculateResultImagePosition(resultSize);

            ResultImage = new Bitmap(resultSize.Width, resultSize.Height);

            Graphics g = Graphics.FromImage(ResultImage);

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            if (Mode == ResizeMode.FillToDestinationAspect)
                g.FillRectangle(new SolidBrush(Background), 0, 0, resultSize.Width, resultSize.Height);

            g.DrawImage(SourceImage,
                resultPosition.X,
                resultPosition.Y,
                resultPosition.Width,
                resultPosition.Height);

            g.Dispose();
        }

        private Image CropImage(Image img)
        {
            if (!CropImageEnabled)
                return img;

            Bitmap bmpImage = new Bitmap(img);
            Bitmap bmpCrop = bmpImage.Clone(SelectionArea,
            bmpImage.PixelFormat);
            return (Image)(bmpCrop);
        }

        private Size CalculateResultImageSize()
        {
            if (Mode != ResizeMode.KeepOriginalAspect && Width != 0 && Height != 0)
            {
                return new Size(Width, Height);
            }

            double aspect = 1.0;

            Size result = new Size(SourceImage.Width, SourceImage.Height);

            aspect = ((double)SourceImage.Width) / (double)SourceImage.Height;

            result.Width = Width;
            result.Height = (int)Math.Round(result.Width / aspect);

            if (result.Height > Height || result.Height == 0)
            {
                result.Height = Height;
                result.Width = (int)Math.Round(aspect * result.Height);
            }

            return result;
        }

        private Rectangle CalculateResultImagePosition(Size imageSize)
        {
            switch (Mode)
            {
                case ResizeMode.FillToDestinationAspect:
                    {
                        Rectangle result = new Rectangle();


                        double aspect = ((double)SourceImage.Width) / SourceImage.Height;

                        result.Width = imageSize.Width;
                        result.Height = (int)Math.Round(result.Width / aspect);

                        if (result.Height > imageSize.Height)
                        {
                            result.Height = imageSize.Height;
                            result.Width = (int)Math.Round(aspect * result.Height);
                        }

                        result.X = (int)Math.Round(imageSize.Width / 2.0 - result.Width / 2.0);
                        result.Y = (int)Math.Round(imageSize.Height / 2.0 - result.Height / 2.0);

                        return result;
                    }
                case ResizeMode.CropToDestinationAspect:
                    {
                        Rectangle result = new Rectangle();

                        double aspect = ((double)SourceImage.Width) / SourceImage.Height;

                        result.Width = imageSize.Width;
                        result.Height = (int)Math.Round(result.Width / aspect);

                        if (result.Height < imageSize.Height)
                        {
                            result.Height = imageSize.Height;
                            result.Width = (int)Math.Round(aspect * result.Height);
                        }

                        result.X = (int)Math.Round(imageSize.Width / 2.0 - result.Width / 2.0);
                        result.Y = (int)Math.Round(imageSize.Height / 2.0 - result.Height / 2.0);

                        return result;
                    }
                default:
                    return new Rectangle(0, 0, imageSize.Width, imageSize.Height);
            }
        }

        public enum ResizeMode
        {
            KeepOriginalAspect = 0,
            FillToDestinationAspect = 1,
            DeformToDestinationAspect = 2,
            CropToDestinationAspect = 4,
        }
    }
}
