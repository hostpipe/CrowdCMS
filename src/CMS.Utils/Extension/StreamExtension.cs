using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Utils.Extension
{
    public static class StreamExtension
    {
        /// <summary>
        /// Writes content from one stream to another.
        /// </summary>
        /// <param name="src">Source stream.</param>
        /// <param name="dest">Destination stream.</param>
        /// <param name="bufferSize">Buffer size.</param>
        public static void Write(this Stream src, Stream dest, int bufferSize)
        {
            int read = 0;
            byte[] buffer = new byte[bufferSize];

            while ((read = src.Read(buffer, 0, bufferSize)) != 0)
            {
                dest.Write(buffer, 0, read);
            }
        }
    }
}
