using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_DespachoPale
    {
        #region 1. DataContexts
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        private OperacionesDBDataContext dcOperaciones = new OperacionesDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades
        public CAL_AnalisisPale AnalisisPale { get; set; }
        public int CantidadPaletizada { get; set; }
        public int HiddenSegundaCarga { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "La primera carga no es válida")]
        public int PrimeraCarga { get; set; }
        public string QRCode { get; set; }
        #endregion
        #region 3. Funciones
        public SYS_User GetAnalista(string UserIns)
        {
            return dcAgroFichas.SYS_User.SingleOrDefault(X => X.UserName == UserIns);
        }

        public string GetLote(int IdDetalleOrdenProduccion)
        {
            CAL_OrdenProduccion ordenProduccion = (from X in dcSoftwareCalidad.CAL_DespachoPale
                                                   where X.IdDetalleOrdenProduccion == IdDetalleOrdenProduccion
                                                    && X.Habilitado == true
                                                   select X.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion).FirstOrDefault();

            return ordenProduccion.LoteComercial;
        }

        public string GetSilo(int IdSilo)
        {
            OPR_Silo oPR_Silo = (from X in dcOperaciones.OPR_Silo
                                 where X.IdSilo == IdSilo
                                 && X.Habilitado == true
                                 select X).Single();

            return oPR_Silo.Descripcion.ToUpper();
        }

        public bool Liberado(int IdPale)
        {
            return (dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true && X.Liberado == true) == null);
        }

        public bool Reproceso(int IdPale)
        {
            return (dcSoftwareCalidad.CAL_ReprocesoPallets.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true) == null);
        }

        public bool Retenido(int IdPale)
        {
            return (dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true && X.Retenido == true) == null);
        }

        public bool RetenidoAut(int IdPale)
        {
            return (dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true && X.RetenidoAut == true) == null);
        }

        public bool Despachado(int IdPale)
        {
            return (dcSoftwareCalidad.CAL_DespachoPale.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true) == null);
        }
        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetContenedores(int? IdContenedor)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_RITContenedor
                                                     join Y in dcSoftwareCalidad.CAL_RIT on X.IdContenedor equals Y.IdContenedor
                                                     where X.Habilitado == true
                                                     && Y.Habilitado == true
                                                     && Y.Aprobado 
                                                     && Y.IdOrdenProduccion == this.IdOrdenProduccion
                                                     orderby X.NContenedor
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdContenedor == this.IdContenedor && IdContenedor != null),
                                                         Text = X.NContenedor.ToUpperInvariant(),
                                                         Value = X.IdContenedor.ToString()
                                                     };

            List<CAL_DespachoPale> cAL_DespachoPaleList = dcSoftwareCalidad.CAL_DespachoPale.Where(X => X.IdOrdenProduccion == this.IdOrdenProduccion && X.Habilitado == true).ToList();
            List<SelectListItem> filtered = selectList.Where(p => !cAL_DespachoPaleList.Any(p2 => p2.IdContenedor == int.Parse(p.Value))).ToList();

            // Agregamos el mismo Contenedor si es que fuera una edición 
            // en el caso de que no haya otro disponible
            if (this.IdDespachoPale != 0)
            {
                filtered.Add(new SelectListItem
                {
                    Selected = (this.IdContenedor == this.IdContenedor && IdContenedor != null),
                    Text = this.CAL_RITContenedor.NContenedor,
                    Value = this.IdContenedor.ToString()
                });
            }

            return selectList;
        }
        #endregion
        #region 5. Validaciones
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

            if (controllerName == "CALDespachosPallets" && actionName == "CrearDespachoPallet" && this.IdContenedor == 0)
                yield return new RuleViolation("El container es requerido", "IdContenedor");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }

        public void Validate(ModelStateDictionary modelState, FormCollection formCollection)
        {
            if (!string.IsNullOrEmpty((formCollection["PrimeraCarga"])))
            {
                if (!int.TryParse(formCollection["PrimeraCarga"], out int PrimeraCarga))
                {
                    modelState.AddModelError("PrimeraCarga", "La primera carga no es válida");
                    return;
                }
                if (PrimeraCarga <= 0 || PrimeraCarga >= this.CantidadPaletizada)
                {
                    modelState.AddModelError("PrimeraCarga", "La carga dividida no puede ser igual o superior a los sacos cargados");
                    return;
                }
                if (HiddenSegundaCarga < 0)
                {
                    modelState.AddModelError("HiddenSegundaCarga", "La segunda carga dividida no puede ser menor a 0 sacos");
                    return;
                }

            }
            else
            {
                modelState.AddModelError("PrimeraCarga", "Debe ingresar la primera carga");
                return;
            }
        }
        #endregion
    }
}