using System;
using System.Collections.Generic;
using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;

namespace AgroFichasWeb.Models
{
    public class Antiguedad
    {
        public string Descripcion { get; set; }

        public int IdAntiguedad { get; set; }
    }

    public partial class CTR_ControlTiempo
    {
        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        #region Region Propiedades

        public int IdAntiguedad { get; set; }

        public int UserID { get; set; }

        public DateTime FechaDesde { get; set; }

        public DateTime FechaHasta { get; set; }

        public enum Lista
        {
            Telefonos,
            Emails
        };

        public IEnumerable<SelectListItem> GetPlantasProduccion(int IdPlantaProduccion)
        {
            IEnumerable<SelectListItem> selectList = from p in dc.PlantaProduccion
                                                     where p.Habilitado
                                                     select new SelectListItem
                                                     {
                                                         Value = p.IdPlantaProduccion.ToString(),
                                                         Text = p.Nombre,
                                                         Selected = IdPlantaProduccion == p.IdPlantaProduccion
                                                     };
            return selectList;
        }

        public Empresa GetEmpresa(int IdEmpresa)
        {
            return dc.Empresa.SingleOrDefault(X => X.IdEmpresa == IdEmpresa);
        }

        public List<rpt_CTR_ResumenControlDiarioRomanaPorEmpresaResult> GetResumenDiario()
        {
            return (from O1 in dc.rpt_CTR_ResumenControlDiarioRomanaPorEmpresa(this.IdEmpresa,this.FechaHoraIns)
                    select O1).ToList();
        }

        public List<rpt_CTR_ResumenControlDiarioPorEmpresaResult> GetResumenDiarioCamionesPlanta()
        {
            return (from O1 in dc.rpt_CTR_ResumenControlDiarioPorEmpresa(this.IdEmpresa, this.FechaDesde,this.FechaHasta,this.IdProducto,this.IdProducto)
                    select O1).ToList();
        }
        
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

        public string CreateCSSClassMedition()
        {
            TimeSpan ts;
            switch (this.IdEstado)
            {
                case 1:
                    ts = (DateTime.Now - this.FechaLlegada);
                    break;
                case 2:
                    ts = (this.FechaPesajeInicial.Value - this.FechaLlegada);
                    break;
                case 3:
                    ts = (this.FechaPesajeFinal.Value - this.FechaPesajeInicial.Value);
                    break;
                case 4:
                    ts = (this.FechaSalida.Value - this.FechaLlegada);
                    break;
                default:
                    return "";
            }

            if (ts.Days == 0 && ts.Hours < 3)
                return "ct-ATiempo";
            else if (ts.Days == 0 && ts.Hours >= 3 && ts.Hours < 12)
                return "ct-EnEspera";
            else if (ts.Days == 0 && ts.Hours >= 12)
                return "ct-Parado";
            else if (ts.Days > 0)
                return "ct-Parado";
            else
                return "";
        }

        public string CreateCSSClassMeditionResponsive()
        {
            TimeSpan ts;
            switch (this.IdEstado)
            {
                case 1:
                    ts = (DateTime.Now - this.FechaLlegada);
                    break;
                case 2:
                    ts = (this.FechaPesajeInicial.Value - this.FechaLlegada);
                    break;
                case 3:
                    ts = (this.FechaPesajeFinal.Value - this.FechaPesajeInicial.Value);
                    break;
                case 4:
                    ts = (this.FechaSalida.Value - this.FechaLlegada);
                    break;
                default:
                    return "";
            }

            if (ts.Days == 0 && ts.Hours < 3)
                return "label label-success";
            else if (ts.Days == 0 && ts.Hours >= 3 && ts.Hours < 12)
                return "label label-warning";
            else if (ts.Days == 0 && ts.Hours >= 12)
                return "label label-danger";
            else if (ts.Days > 0)
                return "label label-danger";
            else
                return "";
        }

        public string CreateCSSClassMeditionResponsive_V2()
        {
            TimeSpan ts;
            if (this.IdEstado != 4)
                ts = (DateTime.Now - this.FechaLlegada);
            else
                ts = (this.FechaSalida.Value - this.FechaLlegada);

            if (ts.Days == 0 && ts.Hours < 3)
                return "label label-success";
            else
            if (ts.Days == 0 && ts.Hours >= 3 && ts.Hours < 12)
                return "label label-warning";
            else
            if (ts.Days == 0 && ts.Hours >= 12)
                return "label label-danger";
            else
            if (ts.Days > 0)
                return "label label-danger";
            else
                return "";
        }

        public string CreateCSSClassResponsive()
        {
            switch (this.IdEstado)
            {
                case 1:
                    return "label label-warning";
                case 2:
                    return "label label-warning";
                case 3:
                    return "label label-warning";
                case 4:
                    return "label label-success";
                default:
                    return "";
            }
        }

        #endregion

        #region Region List Bindings

        public List<CTR_RegistroAlerta> GetRegistrosAlerta()
        {
            return dc.CTR_RegistroAlerta.Where(X => X.IdControlTiempo == this.IdControlTiempo).ToList();
        }



        #endregion

        #region Region Mediciones

        public string CreateDayMedition()
        {
            switch (this.IdEstado)
            {
                case 1:
                    return string.Format("{0:dd} días", (DateTime.Now - this.FechaLlegada));
                case 2:
                    return string.Format("{0:dd} días", this.FechaPesajeInicial.Value - this.FechaLlegada);
                case 3:
                    return string.Format("{0:dd} días", this.FechaPesajeFinal.Value - this.FechaLlegada);
                case 4:
                    return string.Format("{0:dd} días", this.FechaSalida.Value - this.FechaLlegada);
                default:
                    return "";
            }
        }

        public string CreateDayMedition_V2()
        {
            if (this.IdEstado != 4)
                return string.Format("{0:dd} días", (DateTime.Now - this.FechaLlegada));
            else
                return string.Format("{0:dd} días", (this.FechaSalida - this.FechaLlegada));
        }

        public string CreateHoursMedition()
        {
            switch (this.IdEstado)
            {
                case 1:
                    return string.Format("{0:hh} hrs.", (DateTime.Now - this.FechaLlegada));
                case 2:
                    return string.Format("{0:hh} hrs.", this.FechaPesajeInicial.Value - this.FechaLlegada);
                case 3:
                    return string.Format("{0:hh} hrs.", this.FechaPesajeFinal.Value - this.FechaLlegada);
                case 4:
                    return string.Format("{0:hh} hrs.", this.FechaSalida.Value - this.FechaLlegada);
                default:
                    return "";
            }
        }

        public string CreateHoursMedition_V2()
        {
            if (this.IdEstado != 4)
                return string.Format("{0:hh} hrs.", (DateTime.Now - this.FechaLlegada));
            else
                return string.Format("{0:hh} hrs.", (this.FechaSalida - this.FechaLlegada));
        }

        public string CreateMinutesMedition()
        {
            switch (this.IdEstado)
            {
                case 1:
                    return string.Format("{0:mm} min.", (DateTime.Now - this.FechaLlegada));
                case 2:
                    return string.Format("{0:mm} min.", this.FechaPesajeInicial.Value - this.FechaLlegada);
                case 3:
                    return string.Format("{0:mm} min.", this.FechaPesajeFinal.Value - this.FechaLlegada);
                case 4:
                    return string.Format("{0:mm} min.", this.FechaSalida.Value - this.FechaLlegada);
                default:
                    return "";
            }
        }

        public string CreateMinutesMedition_V2()
        {
            if (this.IdEstado != 4)
                return string.Format("{0:mm} min.", (DateTime.Now - this.FechaLlegada));
            else
                return string.Format("{0:mm} min.", (this.FechaSalida - this.FechaLlegada));
        }

        public string CreateMedition()
        {
            switch (this.IdEstado)
            {
                case 1:
                    return string.Format("{0:dd} días con {0:hh} hrs. con {0:mm} min. y con {0:ss} segundos.", (DateTime.Now - this.FechaLlegada));
                case 2:
                    return string.Format("{0:dd} días con {0:hh} hrs. con {0:mm} min. y con {0:ss} segundos.", this.FechaPesajeInicial.Value - this.FechaLlegada);
                case 3:
                    return string.Format("{0:dd} días con {0:hh} hrs. con {0:mm} min. y con {0:ss} segundos.", this.FechaPesajeFinal.Value - this.FechaLlegada);
                case 4:
                    return string.Format("{0:dd} días con {0:hh} hrs. con {0:mm} min. y con {0:ss} segundos.", this.FechaSalida.Value - this.FechaLlegada);
                default:
                    return "";
            }
        }

        public string CreateMedition_V2()
        {
            if (this.IdEstado != 4)
                return string.Format("{0:dd} días con {0:hh} hrs. con {0:mm} min. y con {0:ss} segundos.", (DateTime.Now - this.FechaLlegada));
            else
                return string.Format("{0:dd} días con {0:hh} hrs. con {0:mm} min. y con {0:ss} segundos.", (this.FechaSalida - this.FechaLlegada));
        }

        public string CreateShortMedition()
        {
            switch (this.IdEstado)
            {
                case 1:
                    return string.Format("{0:dd} días con {0:hh} hrs. y con {0:mm} min.", (DateTime.Now - this.FechaLlegada));
                case 2:
                    return string.Format("{0:dd} días con {0:hh} hrs. y con {0:mm} min.", this.FechaPesajeInicial.Value - this.FechaLlegada);
                case 3:
                    return string.Format("{0:dd} días con {0:hh} hrs. y con {0:mm} min.", this.FechaPesajeFinal.Value - this.FechaLlegada);
                case 4:
                    return string.Format("{0:dd} días con {0:hh} hrs. y con {0:mm} min.", this.FechaSalida.Value - this.FechaLlegada);
                default:
                    return "";
            }
        }

        #endregion

        #region Region Plantillas

        public string GetPlantilla()
        {
            return string.Format("El transportista {0} con el camión {1} lleva {2:dd} días con {2:hh} hrs. y con {2:mm} min. en la planta "+ this.PlantaProduccion.Nombre +".", this.NombreTransportista, this.Patente.Replace(" ", "-"), (DateTime.Now - this.FechaLlegada));
        }

        #endregion

        #region Region SelectList

        public IEnumerable<SelectListItem> GetAntiguedades(int? IdAntiguedad)
        {
            List<Antiguedad> list = new List<Antiguedad>();
            list.Add(new Antiguedad() { IdAntiguedad = 1, Descripcion = "De los más Antiguos a los más Nuevos" });
            list.Add(new Antiguedad() { IdAntiguedad = 2, Descripcion = "De los más Nuevos a los más Antiguos" });


            IEnumerable<SelectListItem> selectList = from X in list
                                                     orderby X.IdAntiguedad
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdAntiguedad == IdAntiguedad && IdAntiguedad != null),
                                                         Text = X.Descripcion,
                                                         Value = X.IdAntiguedad.ToString()
                                                     };
            return selectList;
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
            IEnumerable<SelectListItem> selectList = from X in dc.CTR_Estado
                                                     orderby X.IdEstado
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdEstado == IdEstado && IdEstado != null),
                                                         Text = X.Descripcion,
                                                         Value = X.IdEstado.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetProductos(int? IdProducto)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.CTR_Producto
                                                     where X.Habilitado == true
                                                     orderby X.Nombre
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdProducto == IdProducto && IdProducto != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdProducto.ToString()
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

        public IEnumerable<SelectListItem> GetPlantaProduccion(int? IdPlantaProduccion, int userID)
        {
            IEnumerable<SelectListItem> selectList = from us in dc.PlantaUsuario
                                                     join pl in dc.PlantaProduccion on us.IdPlantaProduccion equals pl.IdPlantaProduccion
                                                     where us.UserID == userID
                                                     && pl.Habilitado
                                                     select new SelectListItem
                                                     {
                                                         Value = pl.IdPlantaProduccion.ToString(),
                                                         Text = pl.Nombre,
                                                         Selected = IdPlantaProduccion == pl.IdPlantaProduccion
                                                     };
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
            String validaEmail = "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*";
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

            if (this.IdProducto == 0)
                yield return new RuleViolation("El producto es requerido", "IdProducto");

            if (String.IsNullOrEmpty(this.RutTransportista))
                yield return new RuleViolation("El RUT del transportista es requerido", "RutTransportista");

            if (Rut.ValidaRut(this.RutTransportista) == false)
                yield return new RuleViolation("El RUT del transportista es inválido", "RutTransportista");

            if (String.IsNullOrEmpty(this.RutChofer))
                yield return new RuleViolation("El RUT del chofer es requerido", "RutChofer");

            if (Rut.ValidaRut(this.RutChofer) == false)
                yield return new RuleViolation("El RUT del chofer es inválido", "RutChofer");

            if (String.IsNullOrEmpty(this.NombreTransportista))
                yield return new RuleViolation("El nombre del transportista es requerido", "NombreTransportista");

            if (String.IsNullOrEmpty(this.Patente))
                yield return new RuleViolation("La patente del camión es requerida", "Patente");

            if (!string.IsNullOrEmpty(this.EmailChofer))
            {
                if (Regex.IsMatch(this.EmailChofer, validaEmail))
                {
                    if (Regex.Replace(this.EmailChofer, validaEmail, String.Empty).Length != 0)
                        yield return new RuleViolation("El email del chofer es inválido", "EmailChofer");

                }
                else
                {
                    yield return new RuleViolation("El email del chofer es inválido", "EmailChofer");
                }
            }


            if (this.Patente != null)
                if (this.Patente.Length < 6)
                    yield return new RuleViolation("La patente no puede tener menos de 6 carácteres incluyendo espacios", "Patente");

            if ((actionName.ToLower() == "enviarsms" || actionName.ToLower() == "enviaremail") && this.UserID == 0)
                yield return new RuleViolation("El destinatario es requerido", "UserID");

            if (string.IsNullOrEmpty(this.NombreChofer))
                yield return new RuleViolation("El nombre del chofer es requerido", "NombreChofer");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }

        public bool ValidacionPesoInicial(CTR_ControlTiempo controlTiempo, ModelStateDictionary modelState)
        {
            string errMsg = "";
            bool returnValue = true;
            if (!controlTiempo.PesoInicial.HasValue)
            {
                errMsg = "El peso inicial es requerido";
                modelState.AddModelError("PesoInicial", errMsg);
                returnValue = false;
            }

            return returnValue;
        }

        public bool ValidacionNumeroDeGuiaYPesoFinal(CTR_ControlTiempo controlTiempo, ModelStateDictionary modelState)
        {
            string errMsg = "";
            bool returnValue = true;

            //if (controlTiempo.NumeroGuia <= 0)
            //{
            //    errMsg = "El número de guía es requerido";
            //    modelState.AddModelError("NumeroGuia", errMsg);
            //    returnValue = false;
            //}

            if (!controlTiempo.PesoFinal.HasValue)
            {
                errMsg = "El peso final es requerido";
                modelState.AddModelError("PesoFinal", errMsg);
                returnValue = false;
            }

            return returnValue;
        }

        #endregion
    }
}