using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AgroFichasWeb.Controllers
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public int TotalPages { get; private set; }
        public IList<T> Source { get; private set; }

        public RouteValueDictionary BaseRouteValues { get; set; }

        public PaginatedList(IList<T> source, int? pageIndex, int pageSize)
        {
            this.Source = source;

            PageSize = pageSize;
            TotalCount = source.Count();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            PageIndex = pageIndex ?? 1;

            if (PageIndex == -1)
            {
                this.AddRange(source);
            }
            else
            {
                if (PageIndex < 1)
                    PageIndex = 1;

                this.AddRange(source.Skip((PageIndex - 1) * PageSize).Take(PageSize));
            }
        } 

        public PaginatedList(IQueryable<T> source, int? pageIndex, int pageSize)
        {
            PageSize = pageSize;
            TotalCount = source.Count();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            PageIndex = pageIndex ?? 1;
            if (PageIndex == -1)
            {
                this.AddRange(source);
            }
            else
            {
                if (PageIndex < 1)
                    PageIndex = 1;

                this.AddRange(source.Skip((PageIndex - 1) * PageSize).Take(PageSize));
            }
        }
        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 1);
            }
        }
        public bool HasNextPage
        {
            get
            {
                return (PageIndex < TotalPages);
            }
        }

        public RouteValueDictionary RouteValues(RouteValueDictionary routeValues)
        {
            var result = new RouteValueDictionary(BaseRouteValues);

            if (routeValues != null)
            {
                foreach (var rv in routeValues)
                {
                    if (result.ContainsKey(rv.Key))
                        result[rv.Key] = rv.Value;
                    else
                        result.Add(rv.Key, rv.Value);
                }
            }
            return result;
        }

        public MvcHtmlString Paginator(string actionName, string controllerName, RequestContext context)
        {
            StringBuilder sb = new StringBuilder();
            
            var helper = new UrlHelper(context);

            sb.Append("<div class=\"paginator\">");
            if (PageIndex > 1)
            {
                sb.AppendFormat("<a href=\"{1}\">{0}</a>", "<<", helper.Action(actionName, controllerName, RouteValues(new RouteValueDictionary() {{"pageIndex", PageIndex - 1}})));
            }

            for (int i = 1; i <= TotalPages; i++)
            {
                if (i != PageIndex)
                    sb.AppendFormat("<a href=\"{1}\">{0}</a>", i, helper.Action(actionName, controllerName, RouteValues(new RouteValueDictionary() { { "pageIndex", i } })));
                else
                    sb.Append(i.ToString());
            }

            if (PageIndex < TotalPages)
            {
                sb.AppendFormat("<a href=\"{1}\">{0}</a>", ">>", helper.Action(actionName, controllerName, RouteValues(new RouteValueDictionary() { { "pageIndex", PageIndex + 1 } })));
            }

            sb.AppendFormat("<a href=\"{0}\">Todos</a>", helper.Action(actionName, controllerName, RouteValues(new RouteValueDictionary() { { "pageIndex", -1 } })));

            sb.Append("</div>");
            return new MvcHtmlString(sb.ToString());
        }
    }
}