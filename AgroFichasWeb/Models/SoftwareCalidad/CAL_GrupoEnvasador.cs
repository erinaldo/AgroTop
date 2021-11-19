using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_GrupoEnvasador
    {
        #region 1. DataContexts
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        #endregion
        #region 2. Propiedades
        public int EnvasadoresValidos { get; set; }
        public OPR_Turno oPR_Turno { get; set; }
        #endregion
        #region 3. Funciones
        public List<CAL_GetEnvasadoresResult> GetEnvasadores()
        {
            return dcSoftwareCalidad.CAL_GetEnvasadores().ToList();
        }

        public List<CAL_Envasador> GetEnvasadores(int IdGrupoEnvasador)
        {
            return dcSoftwareCalidad.CAL_Envasador.Where(X => X.IdGrupoEnvasador == IdGrupoEnvasador).ToList();
        }

        public List<CAL_GetEnvasadoresPorGrupoEnvasadorResult> GetEnvasadoresPorGrupoEnvasador(int IdGrupoEnvasador)
        {
            return dcSoftwareCalidad.CAL_GetEnvasadoresPorGrupoEnvasador(IdGrupoEnvasador).ToList();
        }



        public int GetTipoEnvasadores(List<SYS_User> users)
        {
            int IdGrupoEnvasador = 0;

            bool cInterna = false, cExterna = false;
            foreach (SYS_User user in users)
            {
                if (user.IdSeccion == 8)
                    cInterna = true;
                if (user.IdSeccion == 9)
                    cExterna = true;
            }

            if (cInterna == true && cExterna == true)
            {
                IdGrupoEnvasador = 3;
            }
            else if (cInterna == true)
            {
                IdGrupoEnvasador = 1;
            }
            else if (cInterna == true)
            {
                IdGrupoEnvasador = 2;
            }

            return IdGrupoEnvasador;
        }
        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetPlantaProduccion(int? IdPlantaProduccion)
        {
            IEnumerable<SelectListItem> selectList = from pp in dcAgroFichas.PlantaProduccion
                                                     select new SelectListItem
                                                     {
                                                         Value = pp.IdPlantaProduccion.ToString(),
                                                         Text = pp.Nombre,
                                                         Selected = (pp.IdPlantaProduccion == IdPlantaProduccion && IdPlantaProduccion != null)
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
            if (string.IsNullOrEmpty(this.Nombre))
                yield return new RuleViolation("El nombre es requerido", "Nombre");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }

        public bool ValidacionEnvasadores(ModelStateDictionary modelState, HttpContext httpContext)
        {
            if (string.IsNullOrEmpty(httpContext.Request["chkEnvasador"]))
            {
                string errMsg = "Debe seleccionar al menos un envasador";
                modelState.AddModelError("EnvasadoresValidos", errMsg);
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