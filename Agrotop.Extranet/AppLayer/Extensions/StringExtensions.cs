using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Agrotop.Extranet.AppLayer.Extensions
{
    public static class StringExtensions
    {
        public static MvcHtmlString ToMvcHtmlString(this string value)
        {
            return new MvcHtmlString(value);
        }
    }
}