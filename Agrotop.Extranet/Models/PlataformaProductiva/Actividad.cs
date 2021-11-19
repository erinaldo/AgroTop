using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Agrotop.Extranet.Models.PlataformaProductiva
{
    public class Actividad
    {
        public string Descripcion { get; set; }

        public string DescripcionAgricultor { get; set; }

        public string Mes { get; set; }

        public decimal Cantidad { get; set; }

        public string Unidad { get; set; }

        public decimal ValorUnitario { get; set; }

        public decimal ValorItem { get; set; }

        public Actividad()
        {
            this.Descripcion           = "";
            this.DescripcionAgricultor = "";
            this.Mes                   = "";
            this.Cantidad              = 0;
            this.Unidad                = "";
            this.ValorUnitario         = 0;
            this.ValorItem             = 0;
        }

        public static Actividad ParseLaborRequest(HttpContextBase httpContext)
        {
            try
            {
                Actividad actividad = new Actividad();
                actividad.Descripcion = httpContext.Request["MYL__DESCRIPCION__LABOR"] ?? "";
                actividad.DescripcionAgricultor = httpContext.Request["MYL__DESCRIPCION__AGRICULTOR"] ?? "";
                actividad.Mes = httpContext.Request["MYL__MES"] ?? "";
                if (!string.IsNullOrEmpty(httpContext.Request["MYL__CANTIDAD"]))
                {
                    decimal MYL__CANTIDAD = Utils.ParseDecimal(httpContext.Request["MYL__CANTIDAD"]);
                    actividad.Cantidad = MYL__CANTIDAD;
                }
                actividad.Unidad = httpContext.Request["MYL__UNIDAD"] ?? "";
                if (!string.IsNullOrEmpty(httpContext.Request["MYL__VALOR__UNITARIO"]))
                {
                    decimal MYL__VALOR__UNITARIO = Utils.ParseDecimal(httpContext.Request["MYL__VALOR__UNITARIO"]);
                    actividad.ValorUnitario = MYL__VALOR__UNITARIO;
                }
                if (!string.IsNullOrEmpty(httpContext.Request["MYL__VALOR__ITEM"]))
                {
                    decimal MYL__VALOR__ITEM = Utils.ParseDecimal(httpContext.Request["MYL__VALOR__ITEM"]);
                    actividad.ValorItem = MYL__VALOR__ITEM;
                }
                return actividad;
            }
            catch
            {
                return null;
            }
        }

        public static Actividad ParseInsumoRequest(HttpContextBase httpContext)
        {
            try
            {
                Actividad actividad = new Actividad();
                actividad.Descripcion = httpContext.Request["IN__DESCRIPCION__INSUMO"] ?? "";
                actividad.DescripcionAgricultor = httpContext.Request["IN__DESCRIPCION__AGRICULTOR"] ?? "";
                actividad.Mes = httpContext.Request["IN__MES"] ?? "";
                if (!string.IsNullOrEmpty(httpContext.Request["IN__CANTIDAD"]))
                {
                    decimal MYL__CANTIDAD = Utils.ParseDecimal(httpContext.Request["IN__CANTIDAD"]);
                    actividad.Cantidad = MYL__CANTIDAD;
                }
                actividad.Unidad = httpContext.Request["IN__UNIDAD"] ?? "";
                if (!string.IsNullOrEmpty(httpContext.Request["IN__VALOR__UNITARIO"]))
                {
                    decimal MYL__VALOR__UNITARIO = Utils.ParseDecimal(httpContext.Request["IN__VALOR__UNITARIO"]);
                    actividad.ValorUnitario = MYL__VALOR__UNITARIO;
                }
                if (!string.IsNullOrEmpty(httpContext.Request["IN__VALOR__ITEM"]))
                {
                    decimal MYL__VALOR__ITEM = Utils.ParseDecimal(httpContext.Request["IN__VALOR__ITEM"]);
                    actividad.ValorItem = MYL__VALOR__ITEM;
                }
                return actividad;
            }
            catch
            {
                return null;
            }
        }

        public static Actividad ParseFleteRequest(HttpContextBase httpContext)
        {
            try
            {
                Actividad actividad = new Actividad();
                actividad.Descripcion = httpContext.Request["FL__DESCRIPCION__FLETE"] ?? "";
                actividad.DescripcionAgricultor = httpContext.Request["FL__DESCRIPCION__AGRICULTOR"] ?? "";
                actividad.Mes = httpContext.Request["FL__MES"] ?? "";
                if (!string.IsNullOrEmpty(httpContext.Request["FL__CANTIDAD"]))
                {
                    decimal MYL__CANTIDAD = Utils.ParseDecimal(httpContext.Request["FL__CANTIDAD"]);
                    actividad.Cantidad = MYL__CANTIDAD;
                }
                actividad.Unidad = httpContext.Request["FL__UNIDAD"] ?? "";
                if (!string.IsNullOrEmpty(httpContext.Request["FL__VALOR__UNITARIO"]))
                {
                    decimal MYL__VALOR__UNITARIO = Utils.ParseDecimal(httpContext.Request["FL__VALOR__UNITARIO"]);
                    actividad.ValorUnitario = MYL__VALOR__UNITARIO;
                }
                if (!string.IsNullOrEmpty(httpContext.Request["FL__VALOR__ITEM"]))
                {
                    decimal MYL__VALOR__ITEM = Utils.ParseDecimal(httpContext.Request["FL__VALOR__ITEM"]);
                    actividad.ValorItem = MYL__VALOR__ITEM;
                }
                return actividad;
            }
            catch
            {
                return null;
            }
        }

        public static Actividad ParseManoObraRequest(HttpContextBase httpContext)
        {
            try
            {
                Actividad actividad = new Actividad();
                actividad.Descripcion = httpContext.Request["MO__DESCRIPCION__MANO__DE__OBRA"] ?? "";
                actividad.DescripcionAgricultor = httpContext.Request["MO__DESCRIPCION__AGRICULTOR"] ?? "";
                actividad.Mes = httpContext.Request["MO__MES"] ?? "";
                if (!string.IsNullOrEmpty(httpContext.Request["MO__CANTIDAD"]))
                {
                    decimal MYL__CANTIDAD = Utils.ParseDecimal(httpContext.Request["MO__CANTIDAD"]);
                    actividad.Cantidad = MYL__CANTIDAD;
                }
                actividad.Unidad = httpContext.Request["MO__UNIDAD"] ?? "";
                if (!string.IsNullOrEmpty(httpContext.Request["MO__VALOR__UNITARIO"]))
                {
                    decimal MYL__VALOR__UNITARIO = Utils.ParseDecimal(httpContext.Request["MO__VALOR__UNITARIO"]);
                    actividad.ValorUnitario = MYL__VALOR__UNITARIO;
                }
                if (!string.IsNullOrEmpty(httpContext.Request["MO__VALOR__ITEM"]))
                {
                    decimal MYL__VALOR__ITEM = Utils.ParseDecimal(httpContext.Request["MO__VALOR__ITEM"]);
                    actividad.ValorItem = MYL__VALOR__ITEM;
                }
                return actividad;
            }
            catch
            {
                return null;
            }
        }
    }
}