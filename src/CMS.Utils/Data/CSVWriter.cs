using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Utils.Data
{
    public class CSVWriter
    {
        public static byte[] WriteFileContent(List<string> data)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var item in data)
            {
                sb.AppendLine(item);
            }

            return UnicodeEncoding.Unicode.GetBytes(sb.ToString());
        }
    }
}
