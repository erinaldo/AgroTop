using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_DetalleOrdenProduccion
    {
        #region 1. DataContexts
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades
        public List<CAL_GetPorductoAvanceCargaPaleResult> AvanceCargaPallet { get; set; }
        #endregion
        #region 3. Funciones
        public int GetCargado()
        {
            List<CAL_DespachoCargaGranel> despachosCargaGranelList = dcSoftwareCalidad.CAL_DespachoCargaGranel.Where(X => X.IdOrdenProduccion == this.IdOrdenProduccion && X.IdDetalleOrdenProduccion == X.IdDetalleOrdenProduccion && X.Habilitado == true).ToList();
            if (despachosCargaGranelList.Count == 0) return 0;
            return despachosCargaGranelList.Sum(X => X.CantidadCargada);
        }

        public List<CAL_DespachoCargaGranel> GetCargas()
        {
            return dcSoftwareCalidad.CAL_DespachoCargaGranel.Where(X => X.IdDetalleOrdenProduccion == this.IdDetalleOrdenProduccion && X.Habilitado == true).ToList();
        }

        public List<CAL_DespachoCargaGranel> GetCargas(int IdDetalleOrdenProduccion, DateTime dateTime, int IdTurno)
        {
            List<CAL_DespachoCargaGranel> cAL_Cargas = (from X in dcSoftwareCalidad.vw_CAL_TurnoCargaGranel
                                                        join Y in dcSoftwareCalidad.CAL_DespachoCargaGranel on X.IdDespachoCargaGranel equals Y.IdDespachoCargaGranel
                                                        where X.IdDetalleOrdenProduccion == IdDetalleOrdenProduccion
                                                        && X.Date == dateTime.Date
                                                        && X.IdTurno == IdTurno
                                                        select Y).ToList();
            return cAL_Cargas;
        }

        public List<DateTime> GetFechas(int IdDetalleOrdenProduccion)
        {
            return dcSoftwareCalidad.vw_CAL_TurnoPallet.Where(X => X.IdDetalleOrdenProduccion == IdDetalleOrdenProduccion).DistinctBy(X => X.Date).Select(X => X.Date.Value).ToList();
        }

        public List<vw_CAL_TurnoPallet> GetFechas(int IdDetalleOrdenProduccion, DateTime dateTime)
        {
            return dcSoftwareCalidad.vw_CAL_TurnoPallet.Where(X => X.IdDetalleOrdenProduccion == IdDetalleOrdenProduccion && X.Date == dateTime.Date).DistinctBy(X => X.Turno).ToList();
        }

        public int GetLiberado()
        {
            List<CAL_AnalisisPale> liberadoList = dcSoftwareCalidad.CAL_AnalisisPale.Where(X => X.IdOrdenProduccion == this.IdOrdenProduccion && X.IdDetalleOrdenProduccion == this.IdDetalleOrdenProduccion && X.Habilitado == true && X.Retenido == false).ToList();
            if (liberadoList.Count == 0) return 0;
            return liberadoList.Count();
        }

        public List<CAL_Pale> GetPales()
        {
            return dcSoftwareCalidad.CAL_Pale.Where(X => X.IdDetalleOrdenProduccion == this.IdDetalleOrdenProduccion && X.Habilitado == true).ToList();
        }

        public List<CAL_Pale> GetPales(int IdDetalleOrdenProduccion, DateTime dateTime, int IdTurno2)
        {
            List<CAL_Pale> cAL_Pales = (from X in dcSoftwareCalidad.vw_CAL_TurnoPallet
                                        join Y in dcSoftwareCalidad.CAL_Pale on X.IdPale equals Y.IdPale
                                        where X.IdDetalleOrdenProduccion == IdDetalleOrdenProduccion
                                        && X.Date == dateTime.Date
                                        && X.IdTurno2 == IdTurno2
                                        select Y).ToList();
            return cAL_Pales;
        }

        public List<CAL_Pale> GetPalesLiberados(int IdDetalleOrdenProduccion)
        {
            List<CAL_Pale> liberado = (from X in dcSoftwareCalidad.CAL_Pale
                                       join Y in dcSoftwareCalidad.CAL_AnalisisPale on X.IdPale equals Y.IdPale
                                       where X.IdDetalleOrdenProduccion == IdDetalleOrdenProduccion
                                       && X.Habilitado == true
                                       && Y.Habilitado == true
                                       && Y.Retenido == false
                                       select X).ToList();
            return liberado;
        }

        public int GetPaletizado()
        {
            List<CAL_Pale> pales = GetPales();
            return pales.Sum(X => X.CantidadPaletizada);
        }

        public int GetPaletizadoLiberado(int IdDetalleOrdenProduccion)
        {
            List<CAL_Pale> pales = GetPalesLiberados(IdDetalleOrdenProduccion);
            return pales.Sum(X => X.CantidadPaletizada);
        }
        public decimal GetPorcentajeAvance()
        {
            List<CAL_Pale> pales = GetPales();
            return ((decimal)pales.Sum(X => X.CantidadPaletizada) / (decimal)this.CantidadSacos) * (decimal)100;
        }

        public decimal GetPorcentajeAvanceCargado()
        {
            return ((decimal)GetCargado() / (decimal)this.CantidadProducto) * (decimal)100;
        }

        public decimal GetPorcentajeAvanceLiberado(int IdDetalleOrdenProduccion)
        {
            List<CAL_Pale> pales = GetPales();
            return ((decimal)GetPaletizadoLiberado(IdDetalleOrdenProduccion) / (decimal)this.CantidadSacos) * (decimal)100;
        }

        public List<vw_CAL_SoloTurnoCargaGranel> GetSoloTurnoCargaGranel(int IdDetalleOrdenProduccion)
        {
            return dcSoftwareCalidad.vw_CAL_SoloTurnoCargaGranel.Where(X => X.IdDetalleOrdenProduccion == IdDetalleOrdenProduccion).OrderByDescending(X => X.Date).ThenByDescending(X => X.IdTurno).ToList();
        }

        public List<vw_CAL_SoloTurnoPallet> GetSoloTurnoPallet(int IdDetalleOrdenProduccion)
        {
            return dcSoftwareCalidad.vw_CAL_SoloTurnoPallet.Where(X => X.IdDetalleOrdenProduccion == IdDetalleOrdenProduccion).OrderByDescending(X => X.Date).ThenByDescending(X => X.IdTurno2).ToList();
        }
        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetFichasTecnicas(int? IdFichaTecnica)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_FT
                                                     join Y in dcSoftwareCalidad.CAL_FTSaco on X.IdFichaTecnica equals Y.IdFichaTecnica
                                                     where X.IdCliente == this.CAL_OrdenProduccion.IdCliente
                                                     && X.IdSubproducto == this.IdSubproducto
                                                     && Y.IdSaco == this.IdSaco
                                                     && X.Habilitado == true
                                                     orderby X.Codigo
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdFichaTecnica == IdFichaTecnica && IdFichaTecnica != null),
                                                         Text = X.Codigo,
                                                         Value = X.IdFichaTecnica.ToString()
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

            if (actionName == "Editar" && controllerName == "CALFT" && this.IdFichaTecnica == 0)
                yield return new RuleViolation("La ficha técnica es requerida", "IdFichaTecnica");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
        #endregion
    }
}