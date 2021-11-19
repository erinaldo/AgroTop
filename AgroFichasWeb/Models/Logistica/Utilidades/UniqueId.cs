using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace AgroFichasWeb.Models.Logistica.Utilidades
{
    public class UniqueId
    {
        public static string GetUniqueKey()
        {
            int maxSize = 8;
            char[] chars = new char[62];
            string a;
            a = "ABCDEFGHIJKLMNOPQRSTUWXYZ1234567890";
            chars = a.ToCharArray();
            int size = maxSize;
            byte[] data = new byte[1];
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            size = maxSize;
            data = new byte[size];
            crypto.GetNonZeroBytes(data);
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            { result.Append(chars[b % (chars.Length - 1)]); }
            return result.ToString();
        }
    }
}