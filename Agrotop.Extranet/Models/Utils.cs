using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Agrotop.Extranet.Models
{
    public class Utils
    {
        public static decimal ParseDecimal(string s)
        {
            string localDecPoint = (0.5M).ToString("0.0").Substring(1, 1);
            s = s.Replace(",", localDecPoint).Replace(".", localDecPoint);
            return decimal.Parse(s);
        }
    }
}