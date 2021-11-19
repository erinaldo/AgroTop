using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace AgroFichasApi.Models
{
    public class Utils
    {
        private static string hashSalt = "pad#%$&i=(/WOX(9821cusi-.,";

        public static string EncryptPassword(string password)
        {
            var md5 = new MD5CryptoServiceProvider();

            //Convert the input string to a byte array and compute the hash.
            byte[] data = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(hashSalt + password));

            //Create a new Stringbuilder to collect the bytes and create a string.
            var sBuilder = new System.Text.StringBuilder();

            //Loop through each byte of the hashed data and format each one as a hexadecimal string.
            for (var i = 0; i < data.Length; i++)
                sBuilder.Append(data[i].ToString("x2"));

            //Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public static string RemoteAddr()
        {
            return HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        }

        public static Decimal ParseDecimal(string s)
        {
            string localDecPoint = (0.5M).ToString("0.0").Substring(1, 1);
            s = s.Replace(",", localDecPoint).Replace(".", localDecPoint);
            return decimal.Parse(s);
        }
    }
}