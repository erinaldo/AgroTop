using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.ModelBinding;

namespace AgroFichasWeb.Models
{
    public partial class CAL_TipoSaco
    {
        #region 1. DataContexts
        SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades

        #endregion
        #region 3. Funciones
        public List<CAL_PesoTipoSaco> GetPesosSaco(int IdTipoSaco)
        {
            List<CAL_PesoTipoSaco> list = new List<Models.CAL_PesoTipoSaco>();

            var pesosSaco = dc.CAL_GetPesoSacoPorTipoSaco(IdTipoSaco);

            foreach (var pesoSaco in pesosSaco)
            {
                list.Add(new Models.CAL_PesoTipoSaco()
                {
                    IdTipoSaco = this.IdTipoSaco,
                    IdPesoSaco = pesoSaco.IdPesoSaco,
                    CAL_PesoSaco = dc.CAL_PesoSaco.Single(X => X.IdPesoSaco == pesoSaco.IdPesoSaco),
                    Tiene = pesoSaco.Tiene.Value
                });
            }

            return list;
        }
        #endregion
        #region 4. SelectLists

        #endregion
        #region 5. Validaciones
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (String.IsNullOrEmpty(this.Descripcion))
                yield return new RuleViolation("La descripción es requerida", "Descripcion");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }

        public bool ValidacionEmpresas(ModelStateDictionary modelState, HttpContext httpContext)
        {
            string errMsg = "";
            if (string.IsNullOrEmpty(httpContext.Request["chkEmpresa"]))
            {
                errMsg = "Debe seleccionar al menos un peso";
                modelState.AddModelError("PesosValidas", errMsg);
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion
    }
}