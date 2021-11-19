using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_RIT
    {
        #region 1. DataContexts
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades
        public int ParametrosOk { get; set; }
        #endregion
        #region 3. Funciones
        public SYS_User GetVerificador(string UserIns)
        {
            return dcAgroFichas.SYS_User.SingleOrDefault(X => X.UserName == this.UserInsVerificacion);
        }
        public CAL_RITContenedor GetContenedor(int IdContenedor)
        {
            return dcSoftwareCalidad.CAL_RITContenedor.SingleOrDefault(X => X.IdContenedor == IdContenedor);
        }
        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetContenedores(int? IdContenedor)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_RITContenedor
                                                     where X.Habilitado == true
                                                     orderby X.NContenedor
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdContenedor == this.IdContenedor && IdContenedor != null),
                                                         Text = X.NContenedor.ToUpperInvariant(),
                                                         Value = X.IdContenedor.ToString()
                                                     };
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

            //if (this.IdContenedor == 0)
            //    yield return new RuleViolation("El Contenedor es requerido", "IdContenedor");
            if (string.IsNullOrEmpty(this.NContenedor))
                yield return new RuleViolation("El Contenedor es requerido", "NContenedor");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }

        public void Validate(ModelStateDictionary modelState, FormCollection formCollection)
        {
            List<CAL_ParametroRevisarContenedor> cAL_ParametroRevisarContenedorList = dcSoftwareCalidad.CAL_ParametroRevisarContenedor.ToList();

            foreach (CAL_ParametroRevisarContenedor cAL_ParametroRevisarContenedor in cAL_ParametroRevisarContenedorList)
            {
                if (!string.IsNullOrEmpty((formCollection[string.Format("ACCIONCORRECTIVA__{0}", cAL_ParametroRevisarContenedor.IdParametroRevisarContenedor)])) && !int.TryParse(formCollection[string.Format("ACCIONCORRECTIVA__{0}", cAL_ParametroRevisarContenedor.IdParametroRevisarContenedor)], out int ACCIONCORRECTIVA))
                {
                    modelState.AddModelError("ParametrosOk", "Una o más acciones correctivas no son válidas");
                    return;
                }
            }
        }

        public bool ValidacionSelloTara(CAL_RITSelloTara sellotara, ModelStateDictionary modelState)
        {
            string errMsg = "";
            bool returnValue = true;
            if (String.IsNullOrEmpty(sellotara.SelloLinea))
            {
                errMsg = "El sello de línea es requerido";
                modelState.AddModelError("SelloLinea", errMsg);
                returnValue = false;
            }

            if (sellotara.Tara == 0)
            {
                errMsg = "La tara es requerido";
                modelState.AddModelError("Tara", errMsg);
                returnValue = false;
            }

            return returnValue;
        }
        #endregion
    }
}