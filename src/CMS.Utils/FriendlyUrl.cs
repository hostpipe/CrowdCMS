using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CMS.Utils
{
    public class FriendlyUrl
    {
        public static string CreateFriendlyUrl(string url)
        {
            return Regex.Replace(Regex.Replace(url.ToLowerInvariant().Trim(' '), "[^\\s\\w_/-]", String.Empty), "\\s+", "-");
        }
    }
}
