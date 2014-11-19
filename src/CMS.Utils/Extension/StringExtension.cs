using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace CMS.Utils.Extension
{
    public static class StringExtension
    {
        public static string ChangeDecimalSeparator(this string number)
        {
            CultureInfo culture = CultureInfo.CurrentCulture;
            return number.Replace(",", culture.NumberFormat.CurrencyDecimalSeparator).Replace(".", culture.NumberFormat.CurrencyDecimalSeparator);
        }

        // TODO: Generic parsing method for numbers (int, decimal, double, float)

        public static DateTime ParseDateTime(this string dateTimeToParse)
        {
            return DateTime.ParseExact(dateTimeToParse, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        public static bool TryParseDateTime(this string dateTimeToParse, out DateTime date)
        {
            return DateTime.TryParseExact(dateTimeToParse, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }

        public static DateTime ParseDate(this string dateToParse)
        {
            return DateTime.ParseExact(dateToParse, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        public static bool TryParseDate(this string dateToParse, out DateTime date)
        {
            return DateTime.TryParseExact(dateToParse, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }
    }
}
