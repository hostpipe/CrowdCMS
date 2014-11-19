using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.Utils.Cryptography;

namespace CMS.Utils.Drawing
{
    public class ScreenGrab
    {
        
       public static string url2png_v6(string apikey, string secret, string url, string format="png", int maxwidth=270, bool fullpage=true, string viewport="1024x768")
        {
            string location = "http://beta.url2png.com/v6/";
            StringBuilder sb = new StringBuilder();
            sb.Append(String.Format("{0}={1}&", "url", System.Web.HttpUtility.UrlEncode(url)));
            if(format != "")
                sb.Append(String.Format("{0}={1}&", "format", format));
            if (maxwidth >= 0)
                sb.Append(String.Format("{0}={1}&", "thumbnail_max_width", maxwidth.ToString()));
            sb.Append(String.Format("{0}={1}&", "fullpage", fullpage));
            if (viewport != "")
                sb.Append(String.Format("{0}={1}&", "viewport", viewport));
            if (sb.Length > 0)
            {
                sb.Length -= 1;
            }
            string emdeefive = "?" + sb.ToString() + secret;
            string emdeefiveresult = Md5.GetMd5Hash("?" + sb.ToString() + secret);
            return location + apikey + "/" + Md5.GetMd5Hash("?" + sb.ToString() + secret) + "/png/?" + sb.ToString();
        }
    }
}
