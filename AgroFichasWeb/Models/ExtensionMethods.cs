using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public static class ExtensionMethods
    {
        public static bool HasImageExtension(this string source)
        {
            return (source.EndsWith(".png") || source.EndsWith(".jpg") || source.EndsWith(".jpeg") || source.EndsWith(".gif") || source.EndsWith(".bmp") || source.EndsWith(".PNG") || source.EndsWith(".JPG") || source.EndsWith(".JPEG") || source.EndsWith(".GIF") || source.EndsWith(".BMP"));
        }

        public static string RemoveFromEnd(this string str, int characterCount)
        {
            return str.Remove(str.Length - characterCount, characterCount);
        }

        public static int ToInt(this string number, int defaultInt)
        {
            int resultNum = defaultInt;
            try
            {
                if (!string.IsNullOrEmpty(number))
                    resultNum = Convert.ToInt32(number);
            }
            catch
            {
            }
            return resultNum;
        }

        public static string ToTrimmedString(this decimal num)
        {
            string str = num.ToString();
            string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            if (str.Contains(decimalSeparator))
            {
                str = str.TrimEnd('0');
                if (str.EndsWith(decimalSeparator))
                {
                    str = str.RemoveFromEnd(1);
                }
            }
            return str;
        }

        public static string ToTitleCase(this string s) => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.ToLower());
    }
}