using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_AnalisisPale
    {
        #region 1. DataContexts
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        private OperacionesDBDataContext dcOperaciones = new OperacionesDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades
        public int ParametrosOk   { get; set; }
        #endregion
        #region 3. Funciones
        public bool Cargado(int IdPale)
        {
            return (dcSoftwareCalidad.CAL_DespachoPale.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true) == null);
        }

        public bool NoEstaAnalizado(int IdPale)
        {
            return (dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true) == null);
        }

        public SYS_User GetAnalista(string UserIns)
        {
            return dcAgroFichas.SYS_User.SingleOrDefault(X => X.UserName == UserIns);
        }

        public List<CAL_AnalisisPaleTest> GetParametros(int IdPale)
        {
            CAL_AnalisisPale analisisPale = dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true);

            List<CAL_AnalisisPaleTest> cAL_AnalisisPaleTest = (from X in dcSoftwareCalidad.CAL_AnalisisPaleTest
                                                               join Y in dcSoftwareCalidad.CAL_ParametroAnalisisProducto on X.IdParametroAnalisis equals Y.IdParametroAnalisis
                                                               where X.IdAnalisisPale == analisisPale.IdAnalisisPale
                                                               && X.CAL_FTParametroAnalisis.NoAplica == false
                                                               && Y.IdProducto == analisisPale.CAL_DetalleOrdenProduccion.IdProducto
                                                               orderby Y.Orden
                                                               select X).ToList();
            return cAL_AnalisisPaleTest;
        }

        public bool Reprocesado(int IdPale)
        {
            return (dcSoftwareCalidad.CAL_ReprocesoPallets.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true) == null);
        }
        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetPalletPorDetalleOrdenProduccion(int IdDetalleOrdenProduccion, int IdPalet)
        {

            IEnumerable<SelectListItem> selectList = from O1 in dcSoftwareCalidad.CAL_GetPalletPorDetalleOrdenProduccion(IdDetalleOrdenProduccion, IdPalet)
                                                     select new SelectListItem
                                                     {
                                                         Text = O1.IdPale.ToString(),
                                                         Value = O1.IdPale.ToString()
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
            if (this.FechaEtiquetado == null)
                yield return new RuleViolation("La fecha del etiquetado es requerida", "FechaEtiquetado");

            if (this.SacosDetectorMetales < 0)
                yield return new RuleViolation("Los sacos en detector de metales no puede ser negativo", "SacosDetectorMetales");

            yield break;
        }

        public void Validate(ModelStateDictionary modelState, FormCollection formCollection)
        {
            CAL_Pale pale                                     = dcSoftwareCalidad.CAL_Pale.Single(X => X.IdPale == this.IdPale);
            CAL_OrdenProduccion ordenProduccion               = pale.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion;
            CAL_DetalleOrdenProduccion detalleOrdenProduccion = pale.CAL_DetalleOrdenProduccion;
            CAL_FT cAL_FT                                     = dcSoftwareCalidad.CAL_FT.Single(X => X.IdFichaTecnica == this.IdFichaTecnica && X.Habilitado == true);

            List<CAL_FTParametroAnalisis> cAL_FTParametroAnalisList = (from X in dcSoftwareCalidad.CAL_FTParametroAnalisis
                                                                       join Y in dcSoftwareCalidad.CAL_ParametroAnalisisProducto on X.IdParametroAnalisis equals Y.IdParametroAnalisis
                                                                       where X.IdFichaTecnica == cAL_FT.IdFichaTecnica
                                                                       && X.NoAplica == false
                                                                       && Y.IdProducto == detalleOrdenProduccion.IdProducto
                                                                       orderby Y.Orden ascending
                                                                       select X).ToList();

            foreach (CAL_FTParametroAnalisis cAL_FTParametroAnalisis in cAL_FTParametroAnalisList)
            {
                if (string.IsNullOrEmpty((formCollection[string.Format("PARAMETROANALISIS__{0}", cAL_FTParametroAnalisis.IdParametroAnalisis)])))
                {
                    modelState.AddModelError("ParametrosOk", "Debe ingresar todos los parámetros de análisis");
                    return;
                }

                if (!decimal.TryParse(formCollection[string.Format("PARAMETROANALISIS__{0}", cAL_FTParametroAnalisis.IdParametroAnalisis)], out decimal PARAMETROANALISIS))
                {
                    modelState.AddModelError("ParametrosOk", "Uno o más parámetros de análisis son inválidos");
                    return;
                }
            }
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
        #endregion
    }
}