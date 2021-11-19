using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;

namespace AgroFichasWeb.Models.Logistica.Utilidades
{
    public class Comunes
    {
        public static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string HttpBasePath()
        {
            HttpRequest request = HttpContext.Current.Request;
            string s = "http://" + request.ServerVariables["HTTP_HOST"] + request.ApplicationPath;
            if (!s.EndsWith("/"))
            {
                s += "/";
            }
            return s;
        }
    }
}