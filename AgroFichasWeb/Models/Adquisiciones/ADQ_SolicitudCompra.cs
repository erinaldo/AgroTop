using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class ADQ_SolicitudCompra
    {
        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        #region Region Propiedades

        public int UserID { get; set; }

        public enum Lista
        {
            Telefonos,
            Emails
        };

        #endregion

        #region Region CSS

        public string CreateCSSClass()
        {
            switch (this.IdEstado)
            {
                case 1:
                    return "ct-RegistradoEnPorteria";
                case 2:
                    return "ct-CargandoCamion";
                case 3:
                    return "ct-CamionCargado";
                case 4:
                    return "ct-Despachado";
                default:
                    return "";
            }
        }

        //public string CreateCSSClassMedition()
        //{
        //    TimeSpan ts;
        //    switch (this.IdEstado)
        //    {
        //        case 1:
        //            ts = (DateTime.Now - this.FechaLlegada);
        //            break;
        //        case 2:
        //            ts = (this.FechaPesajeInicial.Value - this.FechaLlegada);
        //            break;
        //        case 3:
        //            ts = (this.FechaPesajeFinal.Value - this.FechaPesajeInicial.Value);
        //            break;
        //        case 4:
        //            ts = (this.FechaSalida.Value - this.FechaLlegada);
        //            break;
        //        default:
        //            return "";
        //    }

        //    if (ts.Days == 0 && ts.Hours < 3)
        //        return "ct-ATiempo";
        //    else if (ts.Days == 0 && ts.Hours >= 3 && ts.Hours < 12)
        //        return "ct-EnEspera";
        //    else if (ts.Days == 0 && ts.Hours >= 12)
        //        return "ct-Parado";
        //    else if (ts.Days > 0)
        //        return "ct-Parado";
        //    else
        //        return "";
        //}

        //public string CreateCSSClassMeditionResponsive()
        //{
        //    TimeSpan ts;
        //    switch (this.IdEstado)
        //    {
        //        case 1:
        //            ts = (DateTime.Now - this.FechaLlegada);
        //            break;
        //        case 2:
        //            ts = (this.FechaPesajeInicial.Value - this.FechaLlegada);
        //            break;
        //        case 3:
        //            ts = (this.FechaPesajeFinal.Value - this.FechaPesajeInicial.Value);
        //            break;
        //        case 4:
        //            ts = (this.FechaSalida.Value - this.FechaLlegada);
        //            break;
        //        default:
        //            return "";
        //    }

        //    if (ts.Days == 0 && ts.Hours < 3)
        //        return "label label-success";
        //    else if (ts.Days == 0 && ts.Hours >= 3 && ts.Hours < 12)
        //        return "label label-warning";
        //    else if (ts.Days == 0 && ts.Hours >= 12)
        //        return "label label-danger";
        //    else if (ts.Days > 0)
        //        return "label label-danger";
        //    else
        //        return "";
        //}

        //public string CreateCSSClassMeditionResponsive_V2()
        //{
        //    TimeSpan ts;
        //    if (this.IdEstado != 4)
        //        ts = (DateTime.Now - this.FechaLlegada);
        //    else
        //        ts = (this.FechaSalida.Value - this.FechaLlegada);

        //    if (ts.Days == 0 && ts.Hours < 3)
        //        return "label label-success";
        //    else
        //    if (ts.Days == 0 && ts.Hours >= 3 && ts.Hours < 12)
        //        return "label label-warning";
        //    else
        //    if (ts.Days == 0 && ts.Hours >= 12)
        //        return "label label-danger";
        //    else
        //    if (ts.Days > 0)
        //        return "label label-danger";
        //    else
        //        return "";
        //}

        public string CreateCSSClassResponsive()
        {
            switch (this.IdEstado)
            {
                case 1:
                    return "badge badge-pill badge-info mb-1";
                case 2:
                    return "badge badge-pill badge-warning mb-1";
                case 3:
                    return "badge badge-pill badge-secondary mb-1";
                case 4:
                    return "badge badge-pill badge-light mb-1";
                case 5:
                    return "badge badge-pill badge-dark mb-1";
                case 6:
                    return "badge badge-pill badge-success mb-1";
                case 99:
                    return "badge badge-pill badge-danger mb-1";
                default:
                    return "";
            }
        }

        #endregion

        #region Region List Bindings

        //public List<CTR_RegistroAlerta> GetRegistrosAlerta()
        //{
        //    return dc.CTR_RegistroAlerta.Where(X => X.IdControlTiempo == this.IdControlTiempo).ToList();
        //}

        #endregion

        #region Region Mediciones

        //public string CreateDayMedition()
        //{
        //    switch (this.IdEstado)
        //    {
        //        case 1:
        //            return string.Format("{0:dd} días", (DateTime.Now - this.FechaLlegada));
        //        case 2:
        //            return string.Format("{0:dd} días", this.FechaPesajeInicial.Value - this.FechaLlegada);
        //        case 3:
        //            return string.Format("{0:dd} días", this.FechaPesajeFinal.Value - this.FechaLlegada);
        //        case 4:
        //            return string.Format("{0:dd} días", this.FechaSalida.Value - this.FechaLlegada);
        //        default:
        //            return "";
        //    }
        //}

        //public string CreateDayMedition_V2()
        //{
        //    if (this.IdEstado != 4)
        //        return string.Format("{0:dd} días", (DateTime.Now - this.FechaLlegada));
        //    else
        //        return string.Format("{0:dd} días", (this.FechaSalida - this.FechaLlegada));
        //}

        //public string CreateHoursMedition()
        //{
        //    switch (this.IdEstado)
        //    {
        //        case 1:
        //            return string.Format("{0:hh} hrs.", (DateTime.Now - this.FechaLlegada));
        //        case 2:
        //            return string.Format("{0:hh} hrs.", this.FechaPesajeInicial.Value - this.FechaLlegada);
        //        case 3:
        //            return string.Format("{0:hh} hrs.", this.FechaPesajeFinal.Value - this.FechaLlegada);
        //        case 4:
        //            return string.Format("{0:hh} hrs.", this.FechaSalida.Value - this.FechaLlegada);
        //        default:
        //            return "";
        //    }
        //}

        //public string CreateHoursMedition_V2()
        //{
        //    if (this.IdEstado != 4)
        //        return string.Format("{0:hh} hrs.", (DateTime.Now - this.FechaLlegada));
        //    else
        //        return string.Format("{0:hh} hrs.", (this.FechaSalida - this.FechaLlegada));
        //}

        //public string CreateMinutesMedition()
        //{
        //    switch (this.IdEstado)
        //    {
        //        case 1:
        //            return string.Format("{0:mm} min.", (DateTime.Now - this.FechaLlegada));
        //        case 2:
        //            return string.Format("{0:mm} min.", this.FechaPesajeInicial.Value - this.FechaLlegada);
        //        case 3:
        //            return string.Format("{0:mm} min.", this.FechaPesajeFinal.Value - this.FechaLlegada);
        //        case 4:
        //            return string.Format("{0:mm} min.", this.FechaSalida.Value - this.FechaLlegada);
        //        default:
        //            return "";
        //    }
        //}

        //public string CreateMinutesMedition_V2()
        //{
        //    if (this.IdEstado != 4)
        //        return string.Format("{0:mm} min.", (DateTime.Now - this.FechaLlegada));
        //    else
        //        return string.Format("{0:mm} min.", (this.FechaSalida - this.FechaLlegada));
        //}

        //public string CreateMedition()
        //{
        //    switch (this.IdEstado)
        //    {
        //        case 1:
        //            return string.Format("{0:dd} días con {0:hh} hrs. con {0:mm} min. y con {0:ss} segundos.", (DateTime.Now - this.FechaLlegada));
        //        case 2:
        //            return string.Format("{0:dd} días con {0:hh} hrs. con {0:mm} min. y con {0:ss} segundos.", this.FechaPesajeInicial.Value - this.FechaLlegada);
        //        case 3:
        //            return string.Format("{0:dd} días con {0:hh} hrs. con {0:mm} min. y con {0:ss} segundos.", this.FechaPesajeFinal.Value - this.FechaLlegada);
        //        case 4:
        //            return string.Format("{0:dd} días con {0:hh} hrs. con {0:mm} min. y con {0:ss} segundos.", this.FechaSalida.Value - this.FechaLlegada);
        //        default:
        //            return "";
        //    }
        //}

        //public string CreateMedition_V2()
        //{
        //    if (this.IdEstado != 4)
        //        return string.Format("{0:dd} días con {0:hh} hrs. con {0:mm} min. y con {0:ss} segundos.", (DateTime.Now - this.FechaLlegada));
        //    else
        //        return string.Format("{0:dd} días con {0:hh} hrs. con {0:mm} min. y con {0:ss} segundos.", (this.FechaSalida - this.FechaLlegada));
        //}

        //public string CreateShortMedition()
        //{
        //    switch (this.IdEstado)
        //    {
        //        case 1:
        //            return string.Format("{0:dd} días con {0:hh} hrs. y con {0:mm} min.", (DateTime.Now - this.FechaLlegada));
        //        case 2:
        //            return string.Format("{0:dd} días con {0:hh} hrs. y con {0:mm} min.", this.FechaPesajeInicial.Value - this.FechaLlegada);
        //        case 3:
        //            return string.Format("{0:dd} días con {0:hh} hrs. y con {0:mm} min.", this.FechaPesajeFinal.Value - this.FechaLlegada);
        //        case 4:
        //            return string.Format("{0:dd} días con {0:hh} hrs. y con {0:mm} min.", this.FechaSalida.Value - this.FechaLlegada);
        //        default:
        //            return "";
        //    }
        //}

        #endregion

        #region Region Plantillas

        //public string GetPlantilla()
        //{
        //    return string.Format("El transportista {0} con el camión {1} lleva {2:dd} días con {2:hh} hrs. y con {2:mm} min. en la planta.", this.NombreTransportista, this.Patente.Replace(" ", "-"), (DateTime.Now - this.FechaLlegada));
        //}

        #endregion

        #region Region SelectList

        public SYS_User GetCliente(string UserIns)
        {
            return dc.SYS_User.SingleOrDefault(X => X.UserName == UserIns);
        }

        public IEnumerable<SelectListItem> GetEmpresas(int? IdEmpresa)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.Empresa
                                                     orderby X.Nombre
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdEmpresa == IdEmpresa && IdEmpresa != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdEmpresa.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetEstados(int? IdEstado)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.ADQ_Estado
                                                     orderby X.IdEstado
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdEstado == IdEstado && IdEstado != null),
                                                         Text = X.Descripcion,
                                                         Value = X.IdEstado.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetTipoCompra(int? IdTipoCompra)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.ADQ_TipoCompra
                                                     orderby X.Nombre
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdTipoCompra == IdTipoCompra && IdTipoCompra != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdTipoCompra.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetCentroCosto(int? IdCentroCosto)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.ADQ_CentroCosto
                                                     orderby X.Nombre
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdCentroCosto == IdCentroCosto && IdCentroCosto != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdCentroCosto.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetDetalleProyecto(int? IdDetalleProyecto)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.ADQ_DetalleProyecto
                                                     orderby X.Nombre
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdDetalleProyecto == IdDetalleProyecto && IdDetalleProyecto != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdDetalleProyecto.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetDetalleProyectos()
        {
            IEnumerable<SelectListItem> selectList = from X in dc.ADQ_DetalleProyecto
                                                     orderby X.Nombre
                                                     select new SelectListItem
                                                     {
                                                         Text = X.Nombre,
                                                         Value = X.IdDetalleProyecto.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetUsuarios(int? UserID, Lista lista)
        {
            IEnumerable<SelectListItem> selectList = null;
            if (lista == Lista.Telefonos)
            {
                selectList = from X in dc.SYS_User
                             where X.Telefono != null &&
                             X.Telefono != ""
                             orderby X.FullName
                             select new SelectListItem
                             {
                                 Selected = (X.UserID == UserID && UserID != null),
                                 Text = string.Format("{0} {1}", X.Telefono, X.FullName),
                                 Value = X.UserID.ToString()
                             };
            }
            else if (lista == Lista.Emails)
            {
                selectList = from X in dc.SYS_User
                             orderby X.FullName
                             select new SelectListItem
                             {
                                 Selected = (X.UserID == UserID && UserID != null),
                                 Text = string.Format("{0} ({1})", X.Email, X.FullName),
                                 Value = X.UserID.ToString()
                             };
            }
            return selectList;
        }

        #endregion

        #region Region Validaciones

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

            if (this.IdEmpresa == 0)
                yield return new RuleViolation("La empresa es requerida", "IdEmpresa");

            if (this.IdTipoCompra == 0)
                yield return new RuleViolation("El tipo de compra es requerido", "IdTipoCompra");

            if (this.IdCentroCosto == 0)
                yield return new RuleViolation("El centro de costo es requerido", "IdCentroCosto");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }

        public bool ValidacionEnCotizacion(ADQ_SolicitudCompra solicitudCompra, ModelStateDictionary modelState)
        {
            string errMsg = "";
            bool returnValue = true;
            if (solicitudCompra.IdEstado != 2)
            {
                errMsg = "La confirmación es requerido";
                modelState.AddModelError("IdEstado", errMsg);
                returnValue = false;
            }

            return returnValue;
        }

        public bool ValidacionEnProcesoAutorizacion(ADQ_SolicitudCompra solicitudCompra, ModelStateDictionary modelState)
        {
            string errMsg = "";
            bool returnValue = true;
            if (solicitudCompra.IdEstado != 3)
            {
                errMsg = "La confirmación es requerido";
                modelState.AddModelError("IdEstado", errMsg);
                returnValue = false;
            }

            return returnValue;
        }

        public bool ValidacionOCEnviadaProveedor(ADQ_SolicitudCompra solicitudCompra, ModelStateDictionary modelState)
        {
            string errMsg = "";
            bool returnValue = true;
            if (solicitudCompra.IdEstado != 4)
            {
                errMsg = "La confirmación es requerido";
                modelState.AddModelError("IdEstado", errMsg);
                returnValue = false;
            }

            if (solicitudCompra.NumeroOrdenCompra == null)
            {
                errMsg = "El número de OC es requerido";
                modelState.AddModelError("NumeroOrdenCompra", errMsg);
                returnValue = false;
            }

            return returnValue;
        }

        public bool ValidacionRecepcionadoBodega(ADQ_SolicitudCompra solicitudCompra, ModelStateDictionary modelState)
        {
            string errMsg = "";
            bool returnValue = true;
            if (solicitudCompra.IdEstado != 5)
            {
                errMsg = "La confirmación es requerido";
                modelState.AddModelError("IdEstado", errMsg);
                returnValue = false;
            }

            if (string.IsNullOrEmpty(solicitudCompra.ObservacionRecepcionadaBodega))
            {
                errMsg = "La Observación es requerido";
                modelState.AddModelError("ObservacionRecepcionadaBodega", errMsg);
                returnValue = false;
            }

            return returnValue;
        }

        public bool ValidacionCerrado(ADQ_SolicitudCompra solicitudCompra, ModelStateDictionary modelState)
        {
            string errMsg = "";
            bool returnValue = true;
            if (solicitudCompra.IdEstado != 6)
            {
                errMsg = "La confirmación es requerido";
                modelState.AddModelError("IdEstado", errMsg);
                returnValue = false;
            }

            return returnValue;
        }

        //public bool ValidacionNumeroDeGuiaYPesoFinal(CTR_ControlTiempo controlTiempo, ModelStateDictionary modelState)
        //{
        //    string errMsg = "";
        //    bool returnValue = true;

        //    if (controlTiempo.NumeroGuia <= 0)
        //    {
        //        errMsg = "El número de guía es requerido";
        //        modelState.AddModelError("NumeroGuia", errMsg);
        //        returnValue = false;
        //    }

        //    if (!controlTiempo.PesoFinal.HasValue)
        //    {
        //        errMsg = "El peso final es requerido";
        //        modelState.AddModelError("PesoFinal", errMsg);
        //        returnValue = false;
        //    }

        //    return returnValue;
        //}

        #endregion
    }
}