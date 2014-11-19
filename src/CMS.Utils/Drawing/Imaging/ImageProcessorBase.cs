using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace CMS.Utils.Drawing.Imaging
{
    public abstract class ImageProcessorBase : IDisposable
    {
        private static object _syncRoot = new object();
        private Image _sourceImage = null;

        /// <summary>
        /// Gets or sets the source image.
        /// </summary>
        /// <value>The source image.</value>
        public Image SourceImage
        {
            get { return _sourceImage; }
            set
            {
                if (String.IsNullOrEmpty(_mimeType))
                    RetrieveMimeType(); // throws exception if unknown

                _sourceImage = value;
                ResultImage = null;
            }
        }

        private string _mimeType = null;

        /// <summary>
        /// Gets or sets the MIME type of the result image.
        /// </summary>
        /// <value>A representing the MIME type.</value>
        public string MimeType
        {
            get
            {
                if (_mimeType == null)
                    RetrieveMimeType();

                return _mimeType;
            }
            set
            {
                _mimeType = value;
            }
        }

        private Image _resultImage = null;

        /// <summary>
        /// Gets the result image.
        /// </summary>
        /// <value>The result image.</value>
        public Image ResultImage
        {
            get { return _resultImage; }
            protected set
            {
                if (_resultImage != null)
                    _resultImage.Dispose();

                _resultImage = value;
            }
        }

        private int _resultQuality = 100;

        /// <summary>
        /// Gets or sets the result JPEG quality.
        /// </summary>
        /// <value>The result quality. Values between 0 and 100.</value>
        public int ResultQuality
        {
            get { return _resultQuality; }
            set { _resultQuality = value; }
        }

        private string _sourceImageFile = null;

        /// <summary>
        /// Sets the source image file.
        /// </summary>
        /// <value>The source image file.</value>
        public string SourceImageFile
        {
            set
            {
                _sourceImageFile = value;

                using (FileStream S = new FileStream(value, FileMode.Open))
                {
                    SourceImage = Bitmap.FromStream(S, true, true);
                    S.Close();
                }
                
            }
            get
            {
                return _sourceImageFile;
            }
        }

        private static Dictionary<string, ImageCodecInfo> _encoders = null;

        /// <summary>
        /// Gets a MIME-related dictionary of available encoders.
        /// </summary>
        /// <value>The encoders dictionary.</value>
        public static Dictionary<string, ImageCodecInfo> Encoders
        {
            get
            {
                if (_encoders == null)
                {
                    ImageCodecInfo[] infos = ImageCodecInfo.GetImageEncoders();
                    _encoders = new Dictionary<string, ImageCodecInfo>(infos.Length);

                    foreach (ImageCodecInfo info in infos)
                    {
                        _encoders[info.MimeType] = info;
                    }
                }

                return _encoders;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageProcessorBase"/> class.
        /// </summary>
        /// <param name="sourceImage">The source image file location.</param>
        public ImageProcessorBase(string sourceImage)
        {
            lock (_syncRoot)
            {
                SourceImageFile = sourceImage;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageProcessorBase"/> class.
        /// </summary>
        public ImageProcessorBase()
        {
        }

        /// <summary>
        /// Processing method.
        /// </summary>
        public abstract void Process();

        /// <summary>
        /// Retrieves the MIME type of the source image.
        /// </summary>
        public void RetrieveMimeType()
        {
            string ext = Path.GetExtension(_sourceImageFile).ToLower();

            if (ext == ".jpg")
                _mimeType = "image/jpeg";
            else if (ext == ".jpeg")
                _mimeType = "image/jpeg";
            else if (ext == ".gif")
                _mimeType = "image/gif";
            else if (ext == ".png")
                _mimeType = "image/png";
            else if (ext == ".bmp")
                _mimeType = "image/bmp";
            else
                throw new ArgumentException("Unknown image type.");
        }

        /// <summary>
        /// Saves the result image.
        /// </summary>
        /// <param name="fileName">Name of the result image file.</param>
        public virtual void SaveAs(string fileName)
        {
            if (_resultImage == null)
                throw new InvalidOperationException("Cannot save results before processing");

            EncoderParameters Params = new System.Drawing.Imaging.EncoderParameters(1);
            Params.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_resultQuality);

            if (!Encoders.ContainsKey(MimeType))
                throw new ArgumentException("Unknown file format.");

            ImageCodecInfo codec = Encoders[MimeType];

            _resultImage.Save(fileName, codec, Params);
        }

        /// <summary>
        /// Saves the result image.
        /// </summary>
        /// <param name="stream">Output stream to save the file to.</param>
        public virtual void SaveAs(Stream stream)
        {
            if (_resultImage == null)
                throw new InvalidOperationException("Cannot save results before processing");

            EncoderParameters Params = new System.Drawing.Imaging.EncoderParameters(1);
            Params.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_resultQuality);

            if (!Encoders.ContainsKey(MimeType))
                throw new ArgumentException("Unknown file format.");

            ImageCodecInfo codec = Encoders[MimeType];

            _resultImage.Save(stream, codec, Params);
        }

        public void Dispose()
        {
            if (_resultImage != null)
                _resultImage.Dispose();

            if (_sourceImage != null)
                _sourceImage.Dispose();
        }
    }
}
