using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class OPR_PresupuestoProduccionConsumo
    {
        public string PresupuestoProduccionConsumo { get; set; }

        public IEnumerable<SelectListItem> GetMeses(int? Numero)
        {
            List<Mes> list = new List<Models.Mes>();
            for (int I = 1; I < 13; I++)
            {
                list.Add(new Models.Mes()
                {
                    Numero = I,
                    Nombre = new DateTime(2010, I, 1).ToString("MMMM", CultureInfo.GetCultureInfo("es"))
                });
            }

            IEnumerable<SelectListItem> selectList = from X in list
                                                     orderby X.Numero
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.Numero == Numero && Numero != null),
                                                         Text = X.Nombre,
                                                         Value = X.Numero.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetAños(int? Numero)
        {
            List<Año> list = new List<Models.Año>();
            for (int I = 2017; I < (2017 + ((2017 - DateTime.Now.Year) + 4)); I++)
            {
                list.Add(new Models.Año()
                {
                    Numero = I,
                });
            }

            IEnumerable<SelectListItem> selectList = from X in list
                                                     orderby X.Numero
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.Numero == Numero && Numero != null),
                                                         Text = X.Numero.ToString(),
                                                         Value = X.Numero.ToString()
                                                     };
            return selectList;
        }

        // Validación de Entrada
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            string actionName = "";
            string controllerName = "";

            var routeValues = HttpContext.Current.Request.RequestContext.RouteData.Values;
            if (routeValues != null)
            {
                if (routeValues.ContainsKey("action"))
                {
                    actionName = routeValues["action"].ToString();
                }
                if (routeValues.ContainsKey("controller"))
                {
                    controllerName = routeValues["controller"].ToString();
                }
            }

            if (this.Mes <= 0)
                yield return new RuleViolation("El mes es requerido", "Mes");

            if (this.Año <= 0)
                yield return new RuleViolation("El año es requerido", "Año");

            if (this.ProduccionTotal <= 0)
                yield return new RuleViolation("La producción total es requerida", "ProduccionHojuela");

            if (this.ProduccionHojuela < 0)
                yield return new RuleViolation("La producción de hojuela es requerida", "ProduccionHojuela");

            if (this.ProduccionHarina < 0)
                yield return new RuleViolation("La producción de harina es requerida", "ProduccionHarina");

            if (this.ProductoRetenido < 0)
                yield return new RuleViolation("El producto retenido es requerido", "ProductoRetenido");

            if (this.ProductoRechazado < 0)
                yield return new RuleViolation("El producto rechazado es requerido", "ProductoRechazado");

            if (this.ConsumoAvena <= 0)
                yield return new RuleViolation("El consumo de avena es requerido", "ConsumoAvena");

            if (this.RendimientoTeorico <= 0)
                yield return new RuleViolation("El rendimiento teórico es requerido", "RendimientoTeorico");

            if (this.ProduccionTnPorH <= 0)
                yield return new RuleViolation("La producción de ton/h es requerida", "ProduccionTonH");

            if (actionName.ToLower() != "editar" && !(this.Mes <= 0) && !(this.Año <= 0))
            {
                OperacionesDBDataContext dc = new OperacionesDBDataContext();
                OPR_PresupuestoProduccionConsumo presupuestoProduccionConsumo = dc.OPR_PresupuestoProduccionConsumo.SingleOrDefault(X => X.Mes == this.Mes && X.Año == this.Año);
                if (presupuestoProduccionConsumo != null)
                {
                    yield return new RuleViolation(string.Format("Ya existe un presupuesto de producción y consumo para {0} del {1}", new DateTime(2010, this.Mes, 1).ToString("MMMM", CultureInfo.GetCultureInfo("es")), this.Año), "PresupuestoProduccionConsumo");
                }
            }

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
    }

    public class Mes
    {
        public int Numero { get; set; }

        public string Nombre { get; set; }
    }

    public class Año
    {
        public int Numero { get; set; }
    }
}