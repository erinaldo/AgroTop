using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_DespachoCargaGranel
    {
        #region 1. DataContexts
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        private OperacionesDBDataContext dcOperaciones = new OperacionesDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades
        public int? IdSiloSelect { get; set; }
        public int ParametrosOk { get; set; }
        public int SilosOk { get; set; }
        #endregion
        #region 3. Funciones
        public bool NoEstaAnalizado(int IdDespachoCargaGranel)
        {
            return (dcSoftwareCalidad.CAL_DespachoCargaGranel.SingleOrDefault(X => X.IdDespachoCargaGranel == IdDespachoCargaGranel && X.Habilitado == true) == null);
        }

        public SYS_User GetAnalista(string UserIns)
        {
            return dcAgroFichas.SYS_User.SingleOrDefault(X => X.UserName == UserIns);
        }

        public string GetSilo(int? IdSilo)
        {
            OPR_Silo oPR_Silo = (from X in dcOperaciones.OPR_Silo
                                 where X.IdSilo == IdSilo.Value
                                 && X.Habilitado == true
                                 select X).Single();

            return oPR_Silo.Descripcion.ToUpper();
        }

        public List<CAL_DespachoCargaGranelSilo> GetSilos()
        {
            return dcSoftwareCalidad.CAL_DespachoCargaGranelSilo.Where(X => X.IdDespachoCargaGranel == this.IdDespachoCargaGranel).ToList();
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

            List<CAL_DespachoCargaGranel> cAL_DespachoCargaGranelList = dcSoftwareCalidad.CAL_DespachoCargaGranel.Where(X => X.IdOrdenProduccion == this.IdOrdenProduccion && X.Reproceso == false && X.Habilitado == true).ToList();
            List<SelectListItem> filtered = selectList.Where(p => !cAL_DespachoCargaGranelList.Any(p2 => p2.IdContenedor == int.Parse(p.Value))).ToList();

            // Agregamos el mismo Contenedor si es que fuera una edición 
            // en el caso de que no haya otro disponible
            if (this.IdDespachoCargaGranel != 0)
            {
                if (this.IdContenedor.HasValue)
                {
                    filtered.Add(new SelectListItem
                    {
                        Selected = (this.IdContenedor == IdContenedor && IdContenedor != null),
                        Text = this.CAL_RITContenedor.NContenedor,
                        Value = this.IdContenedor.ToString()
                    });
                }
            }

            return filtered;
        }

        public IEnumerable<SelectListItem> GetSilos(int? IdSilo)
        {
            IEnumerable<SelectListItem> selectList = from X in dcOperaciones.OPR_Silo
                                                     where X.Habilitado == true
                                                     orderby X.IdSilo
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdSilo == this.IdSilo && IdSilo != null),
                                                         Text = X.Descripcion.ToUpperInvariant(),
                                                         Value = X.IdSilo.ToString()
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
            if (this.NContainerDiario == 0)
                yield return new RuleViolation("El nº de container diario es requerido", "NContainerDiario");

            if (this.IdContenedor == null)
                yield return new RuleViolation("El container es requerido", "IdContenedor");

            if (this.KgProductoNoConforme < 0)
                yield return new RuleViolation("Los kg. de producto no conforme no puede ser negativo", "KgProductoNoConforme");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }

        public void Validate(ModelStateDictionary modelState, FormCollection formCollection)
        {
            CAL_OrdenProduccion ordenProduccion               = dcSoftwareCalidad.CAL_OrdenProduccion.Single(X => X.IdOrdenProduccion == this.IdOrdenProduccion);
            CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.Single(X => X.IdDetalleOrdenProduccion == this.IdDetalleOrdenProduccion);
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
                }
                else if (!decimal.TryParse(formCollection[string.Format("PARAMETROANALISIS__{0}", cAL_FTParametroAnalisis.IdParametroAnalisis)], out decimal PARAMETROANALISIS))
                {
                    modelState.AddModelError("ParametrosOk", "Uno o más parámetros de análisis son inválidos");
                }
                if (string.IsNullOrEmpty((formCollection["SILO__ALIMENTACION"])))
                {
                    modelState.AddModelError("SilosOk", "Debe ingresar al menos un silo de alimentación");
                }
            }
        }
        #endregion
    }
}