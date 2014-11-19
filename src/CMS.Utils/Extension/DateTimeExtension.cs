using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace CMS.Utils.Extension
{
    public static class DateTimeExtension
    {
        public static DateTime GetUtcTimeAssumingZone(this DateTime date, string timeZoneID)
        {
            return TimeZoneInfo.ConvertTimeToUtc(date, TimeZoneInfo.FindSystemTimeZoneById(timeZoneID));
        }

        public static string ToCustomDateTimeString(this DateTime date)
        {
            return date.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
        }

        public static string ToCustomDateString(this DateTime date)
        {
            return date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        }
    }
}
