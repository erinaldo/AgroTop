using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Agrotop.Extranet.Models
{
    public partial class Ficha
    {
        public string PdfUrl
        {
            get
            {
                return string.Format("{0}/documents/ficha/{1}?h={2}", Properties.Settings.Default.WebsiteUrl, this.IdFicha, this.Hash);
            }
        }

        public string Hash
        {
            get
            {
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                byte[] data = md5.ComputeHash(Encoding.Default.GetBytes("m$%Sh" + this.IdFicha.ToString() + this.IdPredio.ToString() + this.UserIns));

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                    sb.Append(data[i].ToString("x2"));

                return sb.ToString();
            }
        }

    }
}