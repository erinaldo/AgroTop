using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Text;
using System.Web.Routing;
using System.IO;

namespace AgroFichasWeb.Views
{
    public static class ViewHelpers
    {
        public static MvcHtmlString ListCheckBox(this HtmlHelper helper, bool test)
        {
            if (test)
                return new MvcHtmlString("<div class=\"checkbox\"></div>");
            else
                return new MvcHtmlString("");
        }



        public static MvcHtmlString ActionImage(this HtmlHelper helper, string image, string actionName, object routeValues)
        {
            return new MvcHtmlString(helper.ActionLink("***IMG***", actionName, routeValues).ToString().Replace("***IMG***", String.Format("<img src=\"{0}\" border=\"0\" />", image)));
        }

        public static MvcHtmlString ActionImage(this HtmlHelper helper, string image, string actionName, RouteValueDictionary routeValues)
        {
            return new MvcHtmlString(helper.ActionLink("***IMG***", actionName, routeValues).ToString().Replace("***IMG***", String.Format("<img src=\"{0}\" border=\"0\" />", image)));
        }

        public static MvcHtmlString ActionImage(this HtmlHelper helper, string image, string actionName, string controllerName, object routeValues, object htmlAttributes)
        {
            return new MvcHtmlString(helper.ActionLink("***IMG***", actionName, controllerName, routeValues, htmlAttributes).ToString().Replace("***IMG***", String.Format("<img src=\"{0}\" border=\"0\" />", image)));
        }

        public static MvcHtmlString Paginador(this HtmlHelper helper, int pageIndex, int pageCount, string routeName, string controllerName, string key = "", string action = "index")
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<div class=\"paginator\">");
            if (pageIndex > 1)
            {
                sb.Append(helper.RouteLink("<<", routeName, new { id = (pageIndex - 1), key = key }));
            }

            for (int i = 1; i <= pageCount; i++)
            {
                if (i != pageIndex)
                    sb.Append(helper.RouteLink(i.ToString(), routeName, new { controller = controllerName, action = action, id = i, key = key }));
                else
                    sb.Append(i.ToString());
            }

            if (pageIndex < pageCount)
            {
                sb.Append(helper.RouteLink(">>", routeName, new { id = (pageIndex + 1), key = key }));
            }

            sb.Append("</div>");
            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString PaginadorPrecios(this HtmlHelper helper, int pageIndex, int pageCount, string routeName, string controllerName, int id)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<div class=\"paginator\">");
            if (pageIndex > 1)
            {
                sb.Append(helper.RouteLink("<<", routeName, new { id = id, paginaIx = (pageIndex - 1) }));
            }

            for (int i = 1; i <= pageCount; i++)
            {
                if (i != pageIndex)
                    sb.Append(helper.RouteLink(i.ToString(), routeName, new { controller = controllerName, id = id, paginaIx = i }));
                else
                    sb.Append(i.ToString());
            }

            if (pageIndex < pageCount)
            {
                sb.Append(helper.RouteLink(">>", routeName, new { id = id, paginaIx = (pageIndex + 1) }));
            }

            sb.Append("</div>");
            return new MvcHtmlString(sb.ToString());
        }

        public static string Submenu(this HtmlHelper helper, List<string> list)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<ul>");
            foreach (string item in list)
            {
                sb.Append("<li>");
                sb.Append(item);
                sb.Append("</li>");
            }
            sb.Append("</ul>");
            return sb.ToString();
        }

        public static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static string FormatPeriodo(string periodo)
        {
            string[] aPeriodo = periodo.Split('-');
            return (new DateTime(int.Parse(aPeriodo[0]), int.Parse(aPeriodo[1]), 1)).ToString("MMMM yyyy");
        }

        public static string RenderRazorViewToString(ControllerContext ctx, string viewName, object model)
        {
            ctx.Controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = System.Web.Mvc.ViewEngines.Engines.FindPartialView(ctx, viewName);
                var viewContext = new ViewContext(ctx, viewResult.View, ctx.Controller.ViewData, ctx.Controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ctx, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}