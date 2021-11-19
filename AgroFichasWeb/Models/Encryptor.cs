using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace AgroFichasWeb.Models
{
    public class Encryptor
    {
        private byte[] key;
        private byte[] iv;

        public Encryptor()
        {
            this.key = StringToByteArray("ciepu7jmdlsUJSh65skamacoq9imyysm");
            this.iv = StringToByteArray("7018371335562710");
        }

        private byte[] StringToByteArray(string input)
        {
            return ASCIIEncoding.UTF8.GetBytes(input);
        }

        public string EncryptString(string clearText)
        {
            var alg = new RijndaelManaged();
            alg.Key = this.key;
            alg.IV = this.iv;
            alg.Mode = CipherMode.CBC;
            alg.BlockSize = 128;
            
            var encryptor = alg.CreateEncryptor(alg.Key, alg.IV);

            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(clearText);

                    cs.Write(buffer, 0, buffer.Length);
                    cs.FlushFinalBlock();
                    cs.Close();
                }
                ms.Close();

                byte[] encoded = ms.ToArray();
                return Convert.ToBase64String(encoded);
            }

        }

        public string DecryptString(string encodedString)
        {
            //byte[] encoded = Convert.FromBase64String(encodedString);

            var alg = new RijndaelManaged();
            alg.Key = this.key;
            alg.IV = this.iv;
            alg.Mode = CipherMode.CBC;
            alg.BlockSize = 128;

            var decryptor = alg.CreateDecryptor(alg.Key, alg.IV);

            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                {
                    byte[] buffer = Convert.FromBase64String(encodedString);

                    cs.Write(buffer, 0, buffer.Length);
                    cs.FlushFinalBlock();
                    cs.Close();
                }
                ms.Close();

                byte[] plain = ms.ToArray();
                return System.Text.Encoding.ASCII.GetString(plain);
            }
        }

        public static string ByteArrayToHexString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

    }
}