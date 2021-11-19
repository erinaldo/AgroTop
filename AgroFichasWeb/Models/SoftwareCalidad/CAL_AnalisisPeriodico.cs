using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_AnalisisPeriodico
    {
        #region 1. DataContexts
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades
        public int IdCliente { get; set; }
        public int IdOrdenProduccion { get; set; }
        #endregion
        #region 3. Funciones
        public SYS_User GetAnalista(string UserIns)
        {
            return dcAgroFichas.SYS_User.SingleOrDefault(X => X.UserName == UserIns);
        }
        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetClientes(int? IdCliente)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.vw_CAL_OrdenProduccionCliente
                                                     orderby X.RazonSocial
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdCliente == IdCliente && IdCliente != null),
                                                         Text = X.RazonSocial,
                                                         Value = X.IdCliente.ToString()
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

            if ((controllerName == "CALAnalisisPeriodicos" || controllerName == "calanalisisperiodicos") && actionName == "Index" && this.IdCliente == 0)
                yield return new RuleViolation("El cliente es requerido", "IdCliente");

            yield break;
        }

        public void ValidateParametros(ModelStateDictionary modelState, FormCollection formCollection)
        {
            CAL_OrdenProduccion ordenProduccion               = dcSoftwareCalidad.CAL_OrdenProduccion.Single(X => X.IdOrdenProduccion == this.IdOrdenProduccion);
            CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.Single(X => X.IdDetalleOrdenProduccion == this.IdDetalleOrdenProduccion);
            CAL_FT cAL_FT                                     = dcSoftwareCalidad.CAL_FT.Single(X => X.IdFichaTecnica == this.IdFichaTecnica && X.Habilitado == true);

            switch (this.IdTipoAnalisis)
            {
                case 1:
                    List<CAL_FTParametroMetalPesado> cAL_FTParametroMetalPesadoList = (from X in dcSoftwareCalidad.CAL_FTParametroMetalPesado
                                                                                       where X.IdFichaTecnica == cAL_FT.IdFichaTecnica
                                                                                       && X.NoAplica == false
                                                                                       select X).ToList();

                    foreach (CAL_FTParametroMetalPesado cAL_FTParametroMetalPesado in cAL_FTParametroMetalPesadoList)
                    {
                        if (string.IsNullOrEmpty((formCollection[string.Format("PARAMETROMETALPESADO__{0}", cAL_FTParametroMetalPesado.IdParametroMetalPesado)])))
                        {
                            modelState.AddModelError("ParametrosOk", "Debe ingresar todos los parámetros de metales pesados");
                            return;
                        }

                        if (!decimal.TryParse(formCollection[string.Format("PARAMETROMETALPESADO__{0}", cAL_FTParametroMetalPesado.IdParametroMetalPesado)], out decimal PARAMETROMETALPESADO))
                        {
                            modelState.AddModelError("ParametrosOk", "Uno o más parámetros de metales pesados son inválidos");
                            return;
                        }
                    }
                    break;
                case 2:
                    List<CAL_FTParametroMicotoxina> cAL_FTParametroMicotoxinaList = (from X in dcSoftwareCalidad.CAL_FTParametroMicotoxina
                                                                                     where X.IdFichaTecnica == cAL_FT.IdFichaTecnica
                                                                                     && X.NoAplica == false
                                                                                     select X).ToList();

                    foreach (CAL_FTParametroMicotoxina cAL_FTParametroMicotoxina in cAL_FTParametroMicotoxinaList)
                    {
                        if (string.IsNullOrEmpty((formCollection[string.Format("PARAMETROMICOTOXINA__{0}", cAL_FTParametroMicotoxina.IdParametroMicotoxina)])))
                        {
                            modelState.AddModelError("ParametrosOk", "Debe ingresar todos los parámetros de micotoxinas");
                            return;
                        }

                        if (!decimal.TryParse(formCollection[string.Format("PARAMETROMICOTOXINA__{0}", cAL_FTParametroMicotoxina.IdParametroMicotoxina)], out decimal PARAMETROMICOTOXINA))
                        {
                            modelState.AddModelError("ParametrosOk", "Uno o más parámetros de micotoxinas son inválidos");
                            return;
                        }
                    }
                    break;
                case 3:
                    List<CAL_FTParametroMicrobiologia> cAL_FTParametroMicrobiologiaList = (from X in dcSoftwareCalidad.CAL_FTParametroMicrobiologia
                                                                                           where X.IdFichaTecnica == cAL_FT.IdFichaTecnica
                                                                                           && X.NoAplica == false
                                                                                           select X).ToList();

                    foreach (CAL_FTParametroMicrobiologia cAL_FTParametroMicrobiologia in cAL_FTParametroMicrobiologiaList)
                    {
                        if (string.IsNullOrEmpty((formCollection[string.Format("PARAMETROMICROBIOLOGIA__{0}", cAL_FTParametroMicrobiologia.IdParametroMicrobiologia)])))
                        {
                            modelState.AddModelError("ParametrosOk", "Debe ingresar todos los parámetros de microbiología");
                            return;
                        }

                        if (!decimal.TryParse(formCollection[string.Format("PARAMETROMICROBIOLOGIA__{0}", cAL_FTParametroMicrobiologia.IdParametroMicrobiologia)], out decimal PARAMETROMICROBIOLOGIA))
                        {
                            modelState.AddModelError("ParametrosOk", "Uno o más parámetros de microbiología son inválidos");
                            return;
                        }
                    }
                    break;
                case 4:
                    List<CAL_FTParametroNutricional> cAL_FTParametroNutricionalList = (from X in dcSoftwareCalidad.CAL_FTParametroNutricional
                                                                                       where X.IdFichaTecnica == cAL_FT.IdFichaTecnica
                                                                                       && X.NoAplica == false
                                                                                       select X).ToList();

                    foreach (CAL_FTParametroNutricional cAL_FTParametroNutricional in cAL_FTParametroNutricionalList)
                    {
                        if (string.IsNullOrEmpty((formCollection[string.Format("PARAMETRONUTRICIONAL__{0}", cAL_FTParametroNutricional.IdParametroNutricional)])))
                        {
                            modelState.AddModelError("ParametrosOk", "Debe ingresar todos los parámetros nutricionales");
                            return;
                        }

                        if (!decimal.TryParse(formCollection[string.Format("PARAMETRONUTRICIONAL__{0}", cAL_FTParametroNutricional.IdParametroNutricional)], out decimal PARAMETRONUTRICIONAL))
                        {
                            modelState.AddModelError("ParametrosOk", "Uno o más parámetros nutricionales son inválidos");
                            return;
                        }
                    }
                    break;
                case 5:
                    List<CAL_FTParametroPesticida> cAL_FTParametroPesticidaList = (from X in dcSoftwareCalidad.CAL_FTParametroPesticida
                                                                                   where X.IdFichaTecnica == cAL_FT.IdFichaTecnica
                                                                                   && X.NoAplica == false
                                                                                   select X).ToList();

                    foreach (CAL_FTParametroPesticida cAL_FTParametroPesticida in cAL_FTParametroPesticidaList)
                    {
                        if (string.IsNullOrEmpty((formCollection[string.Format("PARAMETROPESTICIDA__{0}", cAL_FTParametroPesticida.IdParametroPesticida)])))
                        {
                            modelState.AddModelError("ParametrosOk", "Debe ingresar todos los parámetros de pesticidas");
                            return;
                        }

                        if (!decimal.TryParse(formCollection[string.Format("PARAMETROPESTICIDA__{0}", cAL_FTParametroPesticida.IdParametroPesticida)], out decimal PARAMETROPESTICIDA))
                        {
                            modelState.AddModelError("ParametrosOk", "Uno o más parámetros de pesticidas son inválidos");
                            return;
                        }
                    }
                    break;
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