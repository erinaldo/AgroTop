using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgrotopApi.Models
{
    public class Utils
    {
        public static Decimal ParseDecimal(string s)
        {
            string localDecPoint = (0.5M).ToString("0.0").Substring(1, 1);
            s = s.Replace(",", localDecPoint).Replace(".", localDecPoint);
            return decimal.Parse(s);
        }
    }
}