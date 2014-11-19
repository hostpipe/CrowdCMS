using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CMS.Utils
{
    public class StringManipulation
    {
        public static string TruncateLongString(string str, int maxLength)
        {
            return str.Substring(0, Math.Min(str.Length, maxLength));
        }

        public static string ConvertURLToATag(string s)
        {
            //Finds URLs with no protocol
            var urlregex = new Regex(@"\b\({0,1}(?<url>(www|ftp)\.[^ ,""\s<)]*)\b",
              RegexOptions.IgnoreCase | RegexOptions.Compiled);
            //Finds URLs with a protocol
            var httpurlregex = new Regex(@"\b\({0,1}(?<url>[^>](http://www\.|http://|https://|ftp://)[^,""\s<)]*)\b",
              RegexOptions.IgnoreCase | RegexOptions.Compiled);
            //Finds email addresses
            var emailregex = new Regex(@"\b(?<mail>[a-zA-Z_0-9.-]+\@[a-zA-Z_0-9.-]+\.\w+)\b",
              RegexOptions.IgnoreCase | RegexOptions.Compiled);
            s = urlregex.Replace(s, " <a href=\"http://${url}\" rel=\"external\">${url}</a>");
            s = httpurlregex.Replace(s, " <a href=\"${url}\" rel=\"external\">${url}</a>");
            s = emailregex.Replace(s, "<a href=\"mailto:${mail}\">${mail}</a>");
            return s;
        }
    }
}
