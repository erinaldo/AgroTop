using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class LOG_Movimiento
    {
        #region Propiedades Adicionales

        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        private LOG_AsignacionCamion AsignacionCamion
        {
            get { return dc.LOG_AsignacionCamion.Single(x => x.IdPedido == this.IdPedido); }
        }

        private LOG_AsignacionPedido AsignacionPedido
        {
            get { return dc.LOG_AsignacionPedido.Single(x => x.IdPedido == this.IdPedido); }
        }

        private Bodega Bodega
        {
            get
            {
                //En tránsito a planta de origen
                if (this.Pedido.IdEstado == 2)
                {
                    if (this.Pedido.Origen == null)
                        return new Bodega { Nombre = this.Pedido.OtroOrigen };
                    else
                        return dc.Bodega.Single(x => x.IdBodega == this.Pedido.Origen);
                }

                //En tránsito a planta de destino
                if (this.Pedido.IdEstado == 3)
                {
                    if (this.Pedido.Destino == null)
                        return new Bodega { Nombre = this.Pedido.OtroDestino };
                    else
                        return dc.Bodega.Single(x => x.IdBodega == this.Pedido.Destino);
                }

                return new Bodega { Nombre = "Bodega Externa" };
            }
        }

        private LOG_Camion Camion
        {
            get { return dc.LOG_Camion.Single(x => x.IdCamion == this.AsignacionCamion.IdCamion); }
        }

        private LOG_Chofer Chofer
        {
            get { return dc.LOG_Chofer.Single(x => x.IdChofer == this.AsignacionCamion.IdChofer); }
        }

        private LOG_Pedido Pedido
        {
            get { return dc.LOG_Pedido.Single(x => x.IdPedido == this.IdPedido); }
        }

        private Sucursal Sucursal
        {
            get
            {
                if (this.Bodega != null)
                {
                    if (this.Bodega.IdBodega != 0)
                    {
                        return dc.Sucursal.Single(x => x.IdSucursal == this.Bodega.IdSucursal);
                    }
                    else
                    {
                        return new Sucursal { Nombre = "Sucursal Externa" };
                    }
                }
                else
                {
                    return new Sucursal { Nombre = "Sucursal Externa" };
                }
            }
        }

        private LOG_Transportista Transportista
        {
            get { return dc.LOG_Transportista.Single(x => x.IdTransportista == Camion.IdTransportista); }
        }

        #endregion

        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            //Registrar Movimiento de Salida
            if (this.IdPedido == 0)
                yield return new RuleViolation("El IdPedido es requerido (no altere la query)", "IdPedido");
            if (this.NumeroGuia == 0)
                yield return new RuleViolation("El número de guía es requerido", "NumeroGuia");
            if (this.FechaSalida == null)
                yield return new RuleViolation("La fecha de salida es requerida", "FechaSalida");
            if (this.PesajeSalidaKg == 0)
                yield return new RuleViolation("El pesaje de salida es requerido", "PesajeSalidaKg");
            if (this.ValorFletePorKgTransportado == 0)
                yield return new RuleViolation("El valor unitario por kg transportado es requerido", "ValorUnitario");
            if (this.CCD_ConSello == 1 && (this.CCD_NumeroSerieSello == null || this.CCD_NumeroSerieSello == 0))
                yield return new RuleViolation("El número de serie sello es requerido", "CCD_NumeroSerieSello");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }

        public bool NotificarDiferenciaSobreRango(decimal pesajeSalida, decimal pesajeLlegada, string identityName, string remoteAddr)
        {
            try
            {
                string destinatarios = "";

                var dc = new AgroFichasDBDataContext();
                var receptores = dc.ReceptoresNotificacionRomanasDescalibradas().ToList();

                if (receptores.Count == 0)
                    throw new Exception("No hay destinatarios disponibles para notificar la alerta");

                foreach (var receptor in receptores)
                {
                    var email = receptor.Email;
                    destinatarios += (destinatarios != "" ? ", " : "") + email;
                }

                string baseTemplate = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/notificadiferenciasobrerango_template.html"), Encoding.UTF8);
                RepTemp(ref baseTemplate, "NUMPED", this.Pedido.IdPedido.ToString());
                RepTemp(ref baseTemplate, "TRANSPORTISTA", this.Transportista.Nombre);
                RepTemp(ref baseTemplate, "CHOFER", this.Chofer.Nombre);
                RepTemp(ref baseTemplate, "PATENTE", this.Camion.Patente);
                RepTemp(ref baseTemplate, "ORIGEN", this.AsignacionPedido.Origen);
                RepTemp(ref baseTemplate, "PESAJESALIDA", pesajeSalida.ToString("N0"));
                RepTemp(ref baseTemplate, "DESTINO", this.AsignacionPedido.Destino);
                RepTemp(ref baseTemplate, "PESAJELLEGADA", pesajeLlegada.ToString("N0"));
                RepTemp(ref baseTemplate, "DESTINATARIOS", destinatarios);
                RepTemp(ref baseTemplate, "HORAENVIO", DateTime.Now.ToString("dd/MM/yy HH:mm:ss"));

                string subject = string.Format("Romanas Descalibradas en {0} (#{1})", this.Bodega.Nombre, this.Pedido.IdPedido);

                SendMailLogistica(destinatarios, baseTemplate, subject);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool NotificarExcedeTaraMaxima(decimal diferenciaSobreTaraMaxima, decimal porcentajeAdicional, string identityName, string remoteAddr)
        {
            try
            {
                string destinatarios = "";

                var dc = new AgroFichasDBDataContext();
                var receptores = dc.ReceptoresNotificacionCamionSobrecargado().ToList();

                if (receptores.Count == 0)
                    throw new Exception("No hay destinatarios disponibles para notificar la alerta");

                foreach (var receptor in receptores)
                {
                    var email = receptor.Email;
                    destinatarios += (destinatarios != "" ? ", " : "") + email;
                }

                string baseTemplate = null;
                baseTemplate = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/notificaexcedetaramaxima_template.html"), Encoding.UTF8);
                RepTemp(ref baseTemplate, "NUMPED", this.Pedido.IdPedido.ToString());
                RepTemp(ref baseTemplate, "TRANSPORTISTA", this.Transportista.Nombre);
                RepTemp(ref baseTemplate, "CHOFER", this.Chofer.Nombre);
                RepTemp(ref baseTemplate, "PATENTE", this.Camion.Patente);
                RepTemp(ref baseTemplate, "TARAMAXIMA", this.Camion.TaraMaxima.ToString());
                RepTemp(ref baseTemplate, "KILOSADICIONALES", diferenciaSobreTaraMaxima.ToString("N0"));
                RepTemp(ref baseTemplate, "DESTINATARIOS", destinatarios);
                RepTemp(ref baseTemplate, "HORAENVIO", DateTime.Now.ToString("dd/MM/yy HH:mm:ss"));

                string subject = String.Format("Camión {0} Sobrecargado ({1})", this.Camion.Patente, this.Pedido.IdPedido);

                SendMailLogistica(destinatarios, baseTemplate, subject);

                var alerta = dc.LOG_Alerta.SingleOrDefault(x => x.IdTransportista == this.Transportista.IdTransportista);
                if (alerta != null)
                {
                    alerta.Tara += 1;
                    dc.SubmitChanges();
                }
                else
                {
                    alerta = new LOG_Alerta()
                    {
                        CondicionesCamion = 0,
                        IdTransportista = this.Transportista.IdTransportista,
                        Merma = 0,
                        Tara = 1,
                        UserIns = identityName,
                        FechaHoraIns = DateTime.Now,
                        IpIns = remoteAddr,
                    };
                    dc.LOG_Alerta.InsertOnSubmit(alerta);
                    dc.SubmitChanges();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool NotificarMermaExcedeMaximo(decimal merma, string identityName, string remoteAddr)
        {
            try
            {
                string destinatarios = "";

                var dc = new AgroFichasDBDataContext();
                var receptores = dc.ReceptoresNotificacionMermaExcedeElMaximoPermitido().ToList();

                if (receptores.Count == 0)
                    throw new Exception("No hay destinatarios disponibles para notificar la alerta");

                foreach (var receptor in receptores)
                {
                    var email = receptor.Email;
                    destinatarios += (destinatarios != "" ? ", " : "") + email;
                }

                string baseTemplate = null;
                baseTemplate = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/notificamermaexcedemaximo_template.html"), Encoding.UTF8);
                RepTemp(ref baseTemplate, "NUMPED", this.Pedido.IdPedido.ToString());
                RepTemp(ref baseTemplate, "TRANSPORTISTA", this.Transportista.Nombre);
                RepTemp(ref baseTemplate, "CHOFER", this.Chofer.Nombre);
                RepTemp(ref baseTemplate, "PATENTE", this.Camion.Patente);
                RepTemp(ref baseTemplate, "MERMA", merma.ToString("N0"));
                RepTemp(ref baseTemplate, "DESTINATARIOS", destinatarios);
                RepTemp(ref baseTemplate, "HORAENVIO", DateTime.Now.ToString("dd/MM/yy HH:mm:ss"));

                string subject = String.Format("Merma Excede el Máximo Permitido (#{0})", this.Pedido.IdPedido);

                SendMailLogistica(destinatarios, baseTemplate, subject);

                var alerta = dc.LOG_Alerta.SingleOrDefault(x => x.IdTransportista == this.Transportista.IdTransportista);
                if (alerta != null)
                {
                    alerta.Merma += 1;
                    dc.SubmitChanges();
                }
                else
                {
                    alerta = new LOG_Alerta()
                    {
                        CondicionesCamion = 0,
                        IdTransportista = this.Transportista.IdTransportista,
                        Merma = 1,
                        Tara = 0,
                        UserIns = identityName,
                        FechaHoraIns = DateTime.Now,
                        IpIns = remoteAddr,
                    };
                    dc.LOG_Alerta.InsertOnSubmit(alerta);
                    dc.SubmitChanges();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool NotificarNoCumpleCondiciones(string motivo, string identityName, string remoteAddr)
        {
            try
            {
                string destinatarios = "";

                var dc = new AgroFichasDBDataContext();
                var receptores = dc.ReceptoresNotificacionCamionNoAptoParaCarga().ToList();

                if (receptores.Count == 0)
                    throw new Exception("No hay destinatarios disponibles para notificar la alerta");

                foreach (var receptor in receptores)
                {
                    var email = receptor.Email;
                    destinatarios += (destinatarios != "" ? ", " : "") + email;
                }

                string baseTemplate = null;
                baseTemplate = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/notificanocumplecondiciones_template.html"), Encoding.UTF8);
                RepTemp(ref baseTemplate, "NUMPED", this.Pedido.IdPedido.ToString());
                RepTemp(ref baseTemplate, "TRANSPORTISTA", this.Transportista.Nombre);
                RepTemp(ref baseTemplate, "CHOFER", this.Chofer.Nombre);
                RepTemp(ref baseTemplate, "PATENTE", this.Camion.Patente);
                RepTemp(ref baseTemplate, "MOTIVO", motivo);
                RepTemp(ref baseTemplate, "DESTINATARIOS", destinatarios);
                RepTemp(ref baseTemplate, "HORAENVIO", DateTime.Now.ToString("dd/MM/yy HH:mm:ss"));

                string subject = String.Format("Camión No Apto para Carga (#{0})", this.Pedido.IdPedido);

                SendMailLogistica(destinatarios, baseTemplate, subject);

                var alerta = dc.LOG_Alerta.SingleOrDefault(x => x.IdTransportista == this.Transportista.IdTransportista);
                if (alerta != null)
                {
                    alerta.CondicionesCamion += 1;
                    dc.SubmitChanges();
                }
                else
                {
                    alerta = new LOG_Alerta()
                    {
                        CondicionesCamion = 1,
                        IdTransportista = this.Transportista.IdTransportista,
                        Merma = 0,
                        Tara = 0,
                        UserIns = identityName,
                        FechaHoraIns = DateTime.Now,
                        IpIns = remoteAddr,
                    };
                    dc.LOG_Alerta.InsertOnSubmit(alerta);
                    dc.SubmitChanges();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void RepTemp(ref string template, string key, string value)
        {
            template = template.Replace("***" + key + "***", value);
        }

        private void SendMailLogistica(string destinatarios, string body, string subject)
        {
            MailMessage objMM = new MailMessage();

            objMM.From = new MailAddress("notificaciones@saprosem.cl", "Notificaciones Agrotop");
            objMM.To.Add(destinatarios);
            objMM.Subject = subject;
            objMM.IsBodyHtml = true;

            objMM.Body = body;

            var objSmtp = new SmtpClient();
            objSmtp.Send(objMM);
        }
    }
}