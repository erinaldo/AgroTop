using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_OrdenProduccion
    {
        #region 1. DataContexts
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades
        public string IdsTransportistas { get; set; }
        #endregion
        #region 3. Funciones
        public Cliente GetCliente(int IdCliente)
        {
            return dcAgroFichas.Cliente.SingleOrDefault(X => X.IdCliente == IdCliente);
        }

        public string GetCssStyleAutorizado(bool? value)
        {
            if (value.HasValue)
            {
                if (value.Value)
                {
                    return "cal-Autorizado";
                }
                else
                {
                    return "cal-NoAutorizado";
                }
            }
            else
            {
                return "cal-NoAutorizado";
            }
        }

        public string GetGetCssStyleEstado()
        {
            if (this.Terminada)
            {
                return "cal-Cerrada";
            }
            else if (this.Autorizado.HasValue && this.Autorizado.Value)
            {
                return "cal-Autorizado";
            }
            else
            {
                return "cal-NoAutorizado";
            }
        }

        public List<CAL_DetalleOrdenProduccion> GetDetalleOrdenProduccion()
        {
            return dcSoftwareCalidad.CAL_DetalleOrdenProduccion.Where(X => X.IdOrdenProduccion == this.IdOrdenProduccion).ToList();
        }

        public string GetEstado()
        {
            if (this.Terminada)
            {
                return "Cerrada";
            }
            else if (this.Autorizado.HasValue && this.Autorizado.Value)
            {
                return "Autorizada";
            }
            else
            {
                return "No autorizada";
            }
        }

        public Pais GetPais(string PaisCodigo)
        {
            return dcAgroFichas.Pais.SingleOrDefault(X => X.PaisCodigo == PaisCodigo);
        }

        public List<vw_CAL_ResponsableArea> GetResponsablesArea()
        {
            return dcSoftwareCalidad.vw_CAL_ResponsableArea.ToList();
        }

        public List<CAL_GetTransportistasPorOrdenProduccionResult> GetTransporteTerrestre()
        {
            return (from O1 in dcSoftwareCalidad.CAL_GetTransportistasPorOrdenProduccion(this.IdOrdenProduccion)
                    where O1.Seleccionado == true
                    select O1).ToList();
        }

        public SYS_User GetUser(string UserIns)
        {
            return dcAgroFichas.SYS_User.SingleOrDefault(X => X.UserName == UserIns);
        }
        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetCarriers(int? IdCarrier)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.Carrier
                                                     orderby X.Nombre
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdCarrier == IdCarrier && IdCarrier != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdCarrier.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetEmbarcadores(int? IdExportador)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_Exportador
                                                     orderby X.Nombre
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdExportador == IdExportador && IdExportador != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdExportador.ToString()
                                                     };
            return selectList;
        }
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
        public IEnumerable<SelectListItem> GetPaises(string PaisCodigo)
        {
            IEnumerable<SelectListItem> selectList = from X in dcAgroFichas.Pais
                                                     orderby X.PaisNombre
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.PaisCodigo == PaisCodigo && !string.IsNullOrEmpty(PaisCodigo)),
                                                         Text = X.PaisNombre,
                                                         Value = X.PaisCodigo
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetTransportistas(int? IdOrdenProduccion)
        {
            var IdOrdenProduccionSelect = IdOrdenProduccion ?? 0;

            IEnumerable<SelectListItem> selectList = from O1 in dcSoftwareCalidad.CAL_GetTransportistasPorOrdenProduccion(IdOrdenProduccionSelect)
                                                     select new SelectListItem
                                                     {
                                                         Selected = O1.Seleccionado.Value,
                                                         Text = O1.Nombre,
                                                         Value = O1.IdTransportista.ToString()
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
            if (string.IsNullOrEmpty(this.LoteComercial))
                yield return new RuleViolation("El lote comercial es requerido", "LoteComercial");

            if (this.IdExportador == 0)
                yield return new RuleViolation("El embarcador es requerido", "IdExportador");

            if (this.IdCliente == 0)
                yield return new RuleViolation("El consignatario es requerido", "IdCliente");

            if (string.IsNullOrEmpty(this.PaisCodigo))
                yield return new RuleViolation("El país de destino es requerido", "PaisCodigo");

            if (this.IdCarrier == 0)
                yield return new RuleViolation("El carrier es requerido", "IdCarrier");

            if (this.IdBarco == 0)
                yield return new RuleViolation("El barco es requerido", "IdBarco");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
        #endregion
        #region 6. Mailing
        public bool NotificarAutorizar()
        {
            try
            {
                string destinatarios = "";
                var receptores = dcAgroFichas.ReceptoresNotificacionAutorizacionOrdenProduccion().ToList();

                if (receptores.Count == 0)
                    throw new Exception("No hay destinatarios disponibles para notificar la alerta");

                foreach (var receptor in receptores)
                {
                    var email = receptor.Email;
                    destinatarios += (destinatarios != "" ? ", " : "") + email;
                }

                string baseTemplate = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/autorizarordenproduccion_template.html"), Encoding.UTF8);
                Util.RepTempAngularStyle(ref baseTemplate, "LOTE", this.LoteComercial);
                Util.RepTempAngularStyle(ref baseTemplate, "DATETIME", string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Now));
                Util.RepTempAngularStyle(ref baseTemplate, "TIPO", this.CAL_TipoOrdenProduccion.Descripcion);
                Util.RepTempAngularStyle(ref baseTemplate, "FECHA", string.Format("{0:dd/MM/yyyy}", this.Fecha));
                Util.RepTempAngularStyle(ref baseTemplate, "EMBARCADOR", this.CAL_Exportador.Nombre);
                Util.RepTempAngularStyle(ref baseTemplate, "CONSIGNATARIO", this.GetCliente(this.IdCliente).RazonSocial);
                Util.RepTempAngularStyle(ref baseTemplate, "PAIS", this.GetPais(this.PaisCodigo).PaisNombreLocal);
                Util.RepTempAngularStyle(ref baseTemplate, "USER", this.GetUser(this.UserIns).FullName);
                Util.RepTempAngularStyle(ref baseTemplate, "USERUPDATE", this.GetUser(this.UserUpd).FullName);
                Util.RepTempAngularStyle(ref baseTemplate, "YEAR", DateTime.Now.Year.ToString());

                string subject = string.Format(string.Format("Se ha autorizado una orden de producción - N° {0}", this.LoteComercial));

                Util.SendMail(destinatarios, baseTemplate, subject);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool NotificarCreacion()
        {
            try
            {
                string destinatarios = "";
                var receptores = dcAgroFichas.ReceptoresNotificacionCreacionOrdenProduccion().ToList();

                if (receptores.Count == 0)
                    throw new Exception("No hay destinatarios disponibles para notificar la alerta");

                foreach (var receptor in receptores)
                {
                    var email = receptor.Email;
                    destinatarios += (destinatarios != "" ? ", " : "") + email;
                }

                string baseTemplate = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/crearordenproduccion_template.html"), Encoding.UTF8);
                Util.RepTempAngularStyle(ref baseTemplate, "LOTE", this.LoteComercial);
                Util.RepTempAngularStyle(ref baseTemplate, "DATETIME", string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Now));
                Util.RepTempAngularStyle(ref baseTemplate, "TIPO", this.CAL_TipoOrdenProduccion.Descripcion);
                Util.RepTempAngularStyle(ref baseTemplate, "FECHA", string.Format("{0:dd/MM/yyyy}", this.Fecha));
                Util.RepTempAngularStyle(ref baseTemplate, "EMBARCADOR", this.CAL_Exportador.Nombre);
                Util.RepTempAngularStyle(ref baseTemplate, "CONSIGNATARIO", this.GetCliente(this.IdCliente).RazonSocial);
                Util.RepTempAngularStyle(ref baseTemplate, "PAIS", this.GetPais(this.PaisCodigo).PaisNombreLocal);
                Util.RepTempAngularStyle(ref baseTemplate, "USER", this.GetUser(this.UserIns).FullName);
                Util.RepTempAngularStyle(ref baseTemplate, "YEAR", DateTime.Now.Year.ToString());

                string subject = string.Format(string.Format("Se ha creado una nueva orden de producción - N° {0}", this.LoteComercial));

                Util.SendMail(destinatarios, baseTemplate, subject);

                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public bool NotificarEdicion()
        {
            try
            {
                string destinatarios = "";
                var receptores = dcAgroFichas.ReceptoresNotificacionEdicionOrdenProduccion().ToList();

                if (receptores.Count == 0)
                    throw new Exception("No hay destinatarios disponibles para notificar la alerta");

                foreach (var receptor in receptores)
                {
                    var email = receptor.Email;
                    destinatarios += (destinatarios != "" ? ", " : "") + email;
                }

                string baseTemplate = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/editarordenproduccion_template.html"), Encoding.UTF8);
                Util.RepTempAngularStyle(ref baseTemplate, "LOTE", this.LoteComercial);
                Util.RepTempAngularStyle(ref baseTemplate, "DATETIME", string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Now));
                Util.RepTempAngularStyle(ref baseTemplate, "TIPO", this.CAL_TipoOrdenProduccion.Descripcion);
                Util.RepTempAngularStyle(ref baseTemplate, "FECHA", string.Format("{0:dd/MM/yyyy}", this.Fecha));
                Util.RepTempAngularStyle(ref baseTemplate, "EMBARCADOR", this.CAL_Exportador.Nombre);
                Util.RepTempAngularStyle(ref baseTemplate, "CONSIGNATARIO", this.GetCliente(this.IdCliente).RazonSocial);
                Util.RepTempAngularStyle(ref baseTemplate, "PAIS", this.GetPais(this.PaisCodigo).PaisNombreLocal);
                Util.RepTempAngularStyle(ref baseTemplate, "USER", this.GetUser(this.UserIns).FullName);
                Util.RepTempAngularStyle(ref baseTemplate, "USERUPDATE", this.GetUser(this.UserUpd).FullName);
                Util.RepTempAngularStyle(ref baseTemplate, "YEAR", DateTime.Now.Year.ToString());

                string subject = string.Format(string.Format("Se ha editado una orden de producción - N° {0}", this.LoteComercial));

                Util.SendMail(destinatarios, baseTemplate, subject);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool NotificarEliminacion()
        {
            try
            {
                string destinatarios = "";
                var receptores = dcAgroFichas.ReceptoresNotificacionEliminacionOrdenProduccion().ToList();

                if (receptores.Count == 0)
                    throw new Exception("No hay destinatarios disponibles para notificar la alerta");

                foreach (var receptor in receptores)
                {
                    var email = receptor.Email;
                    destinatarios += (destinatarios != "" ? ", " : "") + email;
                }

                string baseTemplate = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/eliminarordenproduccion_template.html"), Encoding.UTF8);
                Util.RepTempAngularStyle(ref baseTemplate, "LOTE", this.LoteComercial);
                Util.RepTempAngularStyle(ref baseTemplate, "DATETIME", string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Now));
                Util.RepTempAngularStyle(ref baseTemplate, "TIPO", this.CAL_TipoOrdenProduccion.Descripcion);
                Util.RepTempAngularStyle(ref baseTemplate, "FECHA", string.Format("{0:dd/MM/yyyy}", this.Fecha));
                Util.RepTempAngularStyle(ref baseTemplate, "EMBARCADOR", this.CAL_Exportador.Nombre);
                Util.RepTempAngularStyle(ref baseTemplate, "CONSIGNATARIO", this.GetCliente(this.IdCliente).RazonSocial);
                Util.RepTempAngularStyle(ref baseTemplate, "PAIS", this.GetPais(this.PaisCodigo).PaisNombreLocal);
                Util.RepTempAngularStyle(ref baseTemplate, "USER", this.GetUser(this.UserIns).FullName);
                Util.RepTempAngularStyle(ref baseTemplate, "USERDELETE", this.GetUser(this.UserUpd).FullName);
                Util.RepTempAngularStyle(ref baseTemplate, "YEAR", DateTime.Now.Year.ToString());

                string subject = string.Format(string.Format("Se ha eliminado una orden de producción - N° {0}", this.LoteComercial));

                Util.SendMail(destinatarios, baseTemplate, subject);

                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}