using AgroFichasWeb.AppLayer.Standarizadores;
using AgroFichasWeb.ViewModels.Recepciones;
using AgroFichasWeb.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class ProcesoIngreso
    {
        private AgroFichasDBDataContext dataContext = new AgroFichasDBDataContext();

        public static string ANALISIS_ACCION_NOTIFICAR = "Notificar";
        public static string ANALISIS_ACCION_RECHAZAR = "Rechazar";
        public static int ANALISIS_AUTORIZADO = 1;
        public static int ANALISIS_ENREVISION = 2;
        public static int ANALISIS_RECHAZADO = 0;

        public string Barcode
        {
            get
            {
                return CodeEan13;
            }
        }

        public string CultivoEmpresa
        {
            get
            {
                return string.Format("{0} {1}", this.CultivoContrato.Nombre, this.Empresa.Sigla);
            }

        }

        public List<Precio> TotalesPrecios
        {
            get
            {
                var dc = new AgroFichasDBDataContext();
                return (from p in dc.TotalesPreciosIngreso(this.IdProcesoIngreso)
                        select new Precio()
                        {
                            Valor = p.Valor.Value,
                            FormatoMoneda = p.Formato,
                            IdMoneda = p.IdMoneda,
                            NombreMoneda = p.Nombre,
                            SimboloMoneda = p.Simbolo
                        }).ToList();
            }
        }

        private string CodeEan13
        {
            get
            {

                return "99" + this.IdProcesoIngreso.ToString("0000000000");
            }
        }

        public EstadoProcesoIngreso EstadoProcesoIngreso
        {
            get
            {
                return dataContext.EstadoProcesoIngreso.Single(X => X.IdEstado == this.IdEstado);
            }
        }

        public static List<ParametroAnalisis> GetParametrosAnalisisLiquidacion(int idCultivo)
        {
            var dc = new AgroFichasDBDataContext();
            return GetParametrosAnalisisLiquidacion(dc, idCultivo);
        }

        public static List<ParametroAnalisis> GetParametrosAnalisisLiquidacion(AgroFichasDBDataContext dc, int idCultivo)
        {
            return dc.ParametroAnalisis.Where(p => p.IdCultivo == idCultivo && p.MostrarEnLiquidacion == true).OrderBy(p => p.Orden).ToList();
        }

        public static int IdFromBarcode(string barcode)
        {
            barcode = barcode.Trim();

            if (barcode.Length != 12 && barcode.Length != 13)
                return 0;

            if (!barcode.StartsWith("99"))
                return 0;

            barcode = barcode.Substring(2, 10);
            int id;
            if (!int.TryParse(barcode, out id))
                return 0;

            if (id < 0)
                return 0;

            return id;
        }

        public void AutorizarSinRevisarAnalisis()
        {
            this.Autorizado = ANALISIS_AUTORIZADO;
            this.AutorizadoAuto = true;
            this.MotivoRechazoAnalisis = "";
        }

        public void CalcularAutorizacionPorAnalisis(out bool notificarAlertas)
        {
            notificarAlertas = false;

            if (this.CultivoContrato.IdCultivo == 1 ||
                this.CultivoContrato.IdCultivo == 2 ||
                this.CultivoContrato.IdCultivo == 3 ||
                this.CultivoContrato.IdCultivo == 4 ||
                this.CultivoContrato.IdCultivo == 10||
                this.CultivoContrato.IdCultivo == 16) //Raps, Trigo, Avena, Lupino, Maiz y Linaza
            {
                var rechazar = false;
                var notificar = false;

                var fails = new List<string>();
                foreach (var valor in this.ValorAnalisis.Where(va => va.Valor.HasValue))
                {
                    if ((valor.ParametroAnalisis.MinAutValue.HasValue && valor.Valor < valor.ParametroAnalisis.MinAutValue) ||
                        (valor.ParametroAnalisis.MaxAutValue.HasValue && valor.Valor > valor.ParametroAnalisis.MaxAutValue))
                    {
                        if ((valor.ParametroAnalisis.AccionAutValue ?? "") == ANALISIS_ACCION_RECHAZAR)
                            rechazar = true;
                        else if ((valor.ParametroAnalisis.AccionAutValue ?? "") == ANALISIS_ACCION_NOTIFICAR)
                            notificar = true;

                        fails.Add(String.Format("{1}: {0} está fuera de rango", valor.ParametroAnalisis.Nombre, valor.ParametroAnalisis.AccionAutValue ?? ""));
                    }
                }

                if (fails.Count > 0 && rechazar)
                {
                    this.Autorizado = ANALISIS_ENREVISION;
                    this.AutorizadoAuto = true;
                    this.MotivoRechazoAnalisis = fails.Aggregate((i, j) => i + "\n" + j);
                }
                else
                {
                    this.Autorizado = ANALISIS_AUTORIZADO;
                    this.AutorizadoAuto = true;
                    if (fails.Count > 0)
                        this.MotivoRechazoAnalisis = fails.Aggregate((i, j) => i + "\n" + j);
                    else
                        this.MotivoRechazoAnalisis = "";

                    notificarAlertas = notificar;
                }
            }
            else
            {
                this.Autorizado = ANALISIS_AUTORIZADO;
                this.AutorizadoAuto = true;
                this.MotivoRechazoAnalisis = "";
            }
        }

        public int CalcularPesoNormal(AgroFichasDBDataContext dc)
        {
            int pn = this.PesoBruto.Value;

            if (this.CultivoContrato == null)
                this.CultivoContrato = dc.CultivoContrato.Single(cc => cc.IdCultivoContrato == this.IdCultivoContrato);

            if (this.CultivoContrato.Cultivo.IdCultivo == 1) //Raps
                pn = RapsCalculator.Normalizar(dc, this).PesoNormal;
            else if (this.CultivoContrato.Cultivo.IdCultivo == 2) //Trigo
                pn = TrigoCalculator.Normalizar(this).PesoNormal;
            else if (this.CultivoContrato.Cultivo.IdCultivo == 3) //Avena
                pn = AvenaCalculator.Normalizar(dc, this).PesoNormal;
            else if (this.CultivoContrato.Cultivo.IdCultivo == 4) //Lupino
                pn = LupinoCalculator.Normalizar(dc, this).PesoNormal;
            else if (this.CultivoContrato.Cultivo.IdCultivo == 16) //Linaza
                pn = LinazaCalculator.Normalizar(dc, this).PesoNormal;
            else if (this.CultivoContrato.Cultivo.IdCultivo == 10) //Maiz
                pn = MaizCalculator.Normalizar(dc, this).PesoNormal;
            return pn;
        }

        public bool EsAnalisisEditable()
        {
            int[] estados = { 4, 5, 6, 7, 9 };
            return estados.Contains(this.IdEstado);
        }

        public bool EsAnulable()
        {
            int[] estados = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            return estados.Contains(this.IdEstado);
        }

        public bool EsCerrable(out List<string> problemas)
        {
            problemas = new List<string>();
            foreach (var va in this.ValorAnalisis)
            {
                if (va.ParametroAnalisis.Requerido && !va.Valor.HasValue)
                    problemas.Add("Se debe ingresar el valor de " + va.ParametroAnalisis.Nombre);
            }

            return problemas.Count == 0;
        }

        public bool EsCierreEditable()
        {
            int[] estados = { 8 };
            return estados.Contains(this.IdEstado);
        }

        public bool EsExportable(out string motivo)
        {
            motivo = "";

            int[] estados = { 10 };
            if (!estados.Contains(this.IdEstado))
            {
                motivo = "Estado " + this.EstadoProcesoIngreso.Nombre + " no permite exportar";
                return false;
            }

            if (this.IdPrimeraLiquidacion() == null)
            {
                motivo = "No tiene número de liquidación";
                return false;
            }

            int[] empresas = { 1, 2, 3, 4 };
            if (!empresas.Contains(this.IdEmpresa))
            {
                motivo = "Empresa " + this.Empresa.Nombre + " no es exportable";
                return false;
            }

            if (String.IsNullOrWhiteSpace(this.Agricultor.SAPID(this.IdEmpresa)))
            {
                motivo = "Agricultor no asociado a SAP";
                return false;
            }

            if (String.IsNullOrWhiteSpace(this.Bodega.SAPID(this.IdEmpresa)))
            {
                motivo = "Bodega no asociada a SAP";
                return false;
            }

            if (String.IsNullOrWhiteSpace(this.CultivoContrato.SAPID))
            {
                motivo = "Cultivo no asociado a SAP";
                return false;
            }

            return true;
        }

        public bool EsPesoFinalEditable()
        {
            int[] estados = { 7 };
            return estados.Contains(this.IdEstado);
        }

        public bool EsPesoInicialEditable()
        {
            int[] estados = { 6, 7 };
            return estados.Contains(this.IdEstado);
        }

        public bool EsReStandarizable(out string msg)
        {
            msg = "";

            int[] estados = { 7, 8 };
            if (!estados.Contains(this.IdEstado))
                msg += "Estado " + this.EstadoProcesoIngreso.Nombre + " no permite re-estandarizar. ";

            if (this.PrecioIngreso.Count > 1)
                msg += "Cierre está dividido. ";

            return msg == "";
        }

        public CalculatorResult ExplainPesoNormal(AgroFichasDBDataContext dc)
        {
            if (this.CultivoContrato.Cultivo.IdCultivo == 1) //Raps
                return RapsCalculator.Normalizar(dc, this);
            else if (this.CultivoContrato.IdCultivo == 2) //Trigo
                return TrigoCalculator.Normalizar(this);
            else if (this.CultivoContrato.Cultivo.IdCultivo == 3) //Avena
                return AvenaCalculator.Normalizar(dc, this);
            else if (this.CultivoContrato.Cultivo.IdCultivo == 4) //Lupino
                return LupinoCalculator.Normalizar(dc, this);
            else if (this.CultivoContrato.Cultivo.IdCultivo == 16) //Linaza
                return LinazaCalculator.Normalizar(dc, this);
            else if (this.CultivoContrato.Cultivo.IdCultivo == 10) //Maiz
                return MaizCalculator.Normalizar(dc, this);

            return null;
        }

        public int? IdPrimeraLiquidacion()
        {
            int? idLiquidacion = null;
            var primeraLiq = this.PrecioIngreso.Where(p => p.IdLiquidacion.HasValue).FirstOrDefault();
            if (primeraLiq != null)
                idLiquidacion = primeraLiq.IdLiquidacion;

            return idLiquidacion;
        }

        public int[] IdsLiquidaciones()
        {
            return this.PrecioIngreso.Where(pi => pi.IdLiquidacion.HasValue).Select(pi => pi.IdLiquidacion.Value).Distinct().ToArray();
        }

        public List<Liquidacion> Liquidaciones()
        {
            var result = new List<Liquidacion>();
            foreach (var precio in this.PrecioIngreso.Where(pi => pi.IdLiquidacion.HasValue))
            {
                if (result.SingleOrDefault(r => r.IdLiquidacion == precio.IdLiquidacion) == null)
                    result.Add(precio.Liquidacion);
            }
            return result;
        }

        public bool NotificarAlertaLaboratorio(ControllerContext ctx, out string msg)
        {
            msg = "";
            try
            {
                string destinatarios = "";

                var dc = new AgroFichasDBDataContext();
                var receptores = dc.ReceptoresNotificacionRechazoFiltrado(this.IdProcesoIngreso).ToList();

                if (receptores.Count == 0)
                    throw new Exception("No hay destinatarios disponibles para notificar la alerta");

                foreach (var receptor in receptores)
                {
                    var email = receptor.Email;
                    destinatarios += (destinatarios != "" ? ", " : "") + email;
                }

                string baseTemplate = null;
                baseTemplate = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/notificaalertalaboratorio_template.html"), Encoding.UTF7);
                RepTemp(ref baseTemplate, "AGRICULTOR", this.Agricultor.Nombre);
                RepTemp(ref baseTemplate, "RUT", this.Agricultor.Rut);
                RepTemp(ref baseTemplate, "CULTIVO", this.CultivoContrato.Nombre);
                RepTemp(ref baseTemplate, "SUCURSAL", this.Sucursal.Nombre);
                RepTemp(ref baseTemplate, "NUMERO", String.Format("{0} (Guía Agricultor: {1})", this.IdProcesoIngreso, this.NumeroGuia));
                RepTemp(ref baseTemplate, "LLEGADA", this.FechaHoraLlegada.Value.ToString("dd/MM/yyyy HH:mm"));
                RepTemp(ref baseTemplate, "VEHICULO", String.Format("{0} {1}", this.TipoVehiculo.Nombre, this.Patente));
                RepTemp(ref baseTemplate, "HORAENVIO", DateTime.Now.ToString("dd/MM/yy HH:mm:ss"));
                RepTemp(ref baseTemplate, "DESTINATARIOS", destinatarios);
                RepTemp(ref baseTemplate, "AUTORIZADORES", "");
                RepTemp(ref baseTemplate, "RESULTADOANALISIS", ViewHelpers.RenderRazorViewToString(ctx, "~/Views/Recepciones/DetalleAnalisis.cshtml", new DetalleAnalisisViewModel() { ProcesoIngreso = this, Columas = 2 }));

                string subject = String.Format("Notificación de Lab en {2} para {1} de {0}", this.Agricultor.Nombre, this.CultivoContrato.Cultivo.Nombre, this.Sucursal.Nombre);

                SendMailLaboratorio(destinatarios, baseTemplate, subject);

                msg = "Se ha notificado de las alertas a los siguientes correos: " + destinatarios;
                return true;
            }
            catch (Exception ex)
            {
                msg = "Ocurrió un error al intentar notificar por correo: " + ex.Message;
                return false;
            }
        }

        public bool NotificarEnRevisionLaboratorio(ControllerContext ctx, out string msg)
        {
            msg = "";
            try
            {
                string destinatarios = "";

                var dc = new AgroFichasDBDataContext();
                var receptores = dc.ReceptoresNotificacionRechazoFiltrado(this.IdProcesoIngreso).ToList();

                if (receptores.Count == 0)
                    throw new Exception("No hay destinatarios disponibles para notificar el rechazo");

                string receptoresSinAut = "";
                string autorizadores = "";
                var receptoresConAut = new List<ReceptoresNotificacionRechazoFiltradoResult>();

                foreach (var receptor in receptores)
                {
                    var email = receptor.Email;
                    destinatarios += (destinatarios != "" ? ", " : "") + email;

                    if (receptor.PuedeAutorizar.Value)
                    {
                        receptoresConAut.Add(receptor);
                        autorizadores += (autorizadores != "" ? ", " : "") + receptor.FullName;
                    }
                    else
                    {
                        receptoresSinAut += (receptoresSinAut != "" ? ", " : "") + email;
                    }
                }

                string baseTemplate = null;
                baseTemplate = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/notificarevisionlaboratorio_template.html"), Encoding.UTF7);
                RepTemp(ref baseTemplate, "AGRICULTOR", this.Agricultor.Nombre);
                RepTemp(ref baseTemplate, "RUT", this.Agricultor.Rut);
                RepTemp(ref baseTemplate, "CULTIVO", this.CultivoContrato.Nombre);
                RepTemp(ref baseTemplate, "SUCURSAL", this.Sucursal.Nombre);
                RepTemp(ref baseTemplate, "NUMERO", String.Format("{0} (Guía Agricultor: {1})", this.IdProcesoIngreso, this.NumeroGuia));
                RepTemp(ref baseTemplate, "LLEGADA", this.FechaHoraLlegada.Value.ToString("dd/MM/yyyy HH:mm"));
                RepTemp(ref baseTemplate, "VEHICULO", String.Format("{0} {1}", this.TipoVehiculo.Nombre, this.Patente));
                RepTemp(ref baseTemplate, "HORAENVIO", DateTime.Now.ToString("dd/MM/yy HH:mm:ss"));
                RepTemp(ref baseTemplate, "DESTINATARIOS", destinatarios);
                RepTemp(ref baseTemplate, "AUTORIZADORES", autorizadores);
                RepTemp(ref baseTemplate, "RESULTADOANALISIS", ViewHelpers.RenderRazorViewToString(ctx, "~/Views/Recepciones/DetalleAnalisis.cshtml", new DetalleAnalisisViewModel() { ProcesoIngreso = this, Columas = 2 }));

                string subject = String.Format("Autorización Requerida en {2} para {1} de {0}", this.Agricultor.Nombre, this.CultivoContrato.Cultivo.Nombre, this.Sucursal.Nombre);

                foreach (var autorizador in receptoresConAut)
                {
                    string token = String.Format("userID={0}&idProcesoIngreso={1}&ts={2:yyyy-MM-dd-HH-mm}", autorizador.UserID, this.IdProcesoIngreso, DateTime.Now);

                    var enc = new Encryptor();
                    token = enc.EncryptString(token);

                    string autorizarLink = string.Format("{0}?h={1}", ConfigurationManager.AppSettings["AutUrl"], HttpContext.Current.Server.UrlEncode(token));

                    string autorizarHtml = "<p>Puede autorizar este ingreso en el sistema, opción <i>Recepciones &gt; Autorizar Ingresos</i>.</p>" +
                                           "<p>Para autorizar inmediatemente, utilice el siguiente link: <a href=\"***AUTORIZALINK***\">***AUTORIZALINK***</a></p>";

                    string Template = baseTemplate;
                    RepTemp(ref Template, "SECCIONAUTORIZACION", autorizarHtml.Replace("***AUTORIZALINK***", autorizarLink));

                    SendMailLaboratorio(autorizador.Email, Template, subject);
                }

                if (receptoresSinAut != "")
                {
                    string Template = baseTemplate;
                    RepTemp(ref Template, "SECCIONAUTORIZACION", "");

                    SendMailLaboratorio(receptoresSinAut, Template, subject);
                }

                msg = "Se ha notificado que el ingreso está en revisión a los siguientes correos: " + destinatarios;
                return true;
            }
            catch (Exception ex)
            {
                msg = "Ocurrió un error al intentar notificar por correo: " + ex.Message;
                return false;
            }
        }

        public bool NotificarMaiz(ControllerContext ctx, string userName, string fullName, out string msg)
        {
            msg = "";
            try
            {
                string destinatarios = "";
                string nombreAnalista= "";

                var dc = new AgroFichasDBDataContext();
                var receptores = dc.ReceptoresNotificacionMaiz(this.IdProcesoIngreso/*, userName*/).ToList();

                if (receptores.Count == 0)
                    throw new Exception("No hay destinatarios disponibles para notificar la alerta");

                foreach (var receptor in receptores)
                {
                    if (receptor.Notificar == 1)
                    {
                        var email = receptor.Email;
                        destinatarios += (destinatarios != "" ? ", " : "") + email;
                    }
                    else
                    {
                        nombreAnalista = receptor.FullName;
                    }
                }

                destinatarios = destinatarios + ", notificacionesmaiz@granotop.cl"; 

                string fechaPesoFinal = this.FechaPesoFinal.HasValue ? Convert.ToString(this.FechaPesoFinal.Value.ToString("dd/MM/yyyy HH:mm")) : "No registrado";
                //string pesoFinal = this.PesoFinal.HasValue ? Convert.ToString(this.PesoFinal) + " Kg" : "No registrado";
                //string pesoBruto = this.PesoBruto.HasValue ? Convert.ToString(this.PesoBruto) + " Kg" : "No registrado";


                string baseTemplate = null;
                baseTemplate = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/notificamaiz_template.html"), Encoding.UTF7);
                RepTemp(ref baseTemplate, "AGRICULTOR", this.Agricultor.Nombre);
                RepTemp(ref baseTemplate, "RUT", this.Agricultor.Rut);
                RepTemp(ref baseTemplate, "EMAILCONTACTO", this.Agricultor.Email);
                RepTemp(ref baseTemplate, "FONOCONTACTO", this.Agricultor.Fono1);
                RepTemp(ref baseTemplate, "CULTIVO", this.CultivoContrato.Nombre);
                RepTemp(ref baseTemplate, "SUCURSAL", this.Sucursal.Nombre);
                RepTemp(ref baseTemplate, "NUMERO", Convert.ToString(this.IdProcesoIngreso));
                RepTemp(ref baseTemplate, "GUIA", Convert.ToString(this.NumeroGuia));
                RepTemp(ref baseTemplate, "TIPOSERVICIO", Convert.ToString(this.IdTipoServicio == 1 ? "VENTA" : "A GUARDA"));
                RepTemp(ref baseTemplate, "LLEGADA", this.FechaHoraLlegada.Value.ToString("dd/MM/yyyy HH:mm"));
                RepTemp(ref baseTemplate, "SALIDA", fechaPesoFinal);
                RepTemp(ref baseTemplate, "VEHICULO", String.Format("{0} {1}", this.TipoVehiculo.Nombre, this.Patente));
                RepTemp(ref baseTemplate, "CHOFER", this.Chofer);
                RepTemp(ref baseTemplate, "HORAENVIO", DateTime.Now.ToString("dd/MM/yy HH:mm:ss"));
                //RepTemp(ref baseTemplate, "RESPONSABLE", fullName);
                //RepTemp(ref baseTemplate, "ANALISTA", nombreAnalista);
                RepTemp(ref baseTemplate, "DESTINATARIOS", destinatarios);
                RepTemp(ref baseTemplate, "AUTORIZADORES", "");
                RepTemp(ref baseTemplate, "RESULTADOANALISIS", ViewHelpers.RenderRazorViewToString(ctx, "~/Views/Recepciones/DetalleAnalisisNotificacion.cshtml", new DetalleAnalisisViewModel() { ProcesoIngreso = this, Columas = 2 }));

                string subject = String.Format("Notificación de ingreso N°{3} correspondiente a guía N°{4} en {2} para cultivo {1} de {0} ", this.Agricultor.Nombre, this.CultivoContrato.Cultivo.Nombre, this.Sucursal.Nombre, this.IdProcesoIngreso, this.NumeroGuia);

                SendMailNotificaMaiz(destinatarios, baseTemplate, subject);

                msg = "Se ha notificado de la recepción y resultados a los siguientes correos: " + destinatarios;
                return true;
            }
            catch (Exception ex)
            {
                msg = "Ocurrió un error al intentar notificar por correo: " + ex.Message;
                return false;
            }
        }

        public bool NotificarRechazoFinalLaboratorio(ControllerContext ctx, out string msg)
        {
            msg = "";
            try
            {
                string destinatarios = "";

                var dc = new AgroFichasDBDataContext();
                var receptores = dc.ReceptoresNotificacionRechazoFiltrado(this.IdProcesoIngreso).ToList();

                if (receptores.Count == 0)
                    throw new Exception("No hay destinatarios disponibles para notificar la alerta");

                foreach (var receptor in receptores)
                {
                    var email = receptor.Email;
                    destinatarios += (destinatarios != "" ? ", " : "") + email;
                }

                string baseTemplate = null;
                baseTemplate = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/notificarechazofinallaboratorio_template.html"), Encoding.UTF7);
                RepTemp(ref baseTemplate, "AGRICULTOR", this.Agricultor.Nombre);
                RepTemp(ref baseTemplate, "RUT", this.Agricultor.Rut);
                RepTemp(ref baseTemplate, "CULTIVO", this.CultivoContrato.Nombre);
                RepTemp(ref baseTemplate, "SUCURSAL", this.Sucursal.Nombre);
                RepTemp(ref baseTemplate, "NUMERO", String.Format("{0} (Guía Agricultor: {1})", this.IdProcesoIngreso, this.NumeroGuia));
                RepTemp(ref baseTemplate, "LLEGADA", this.FechaHoraLlegada.Value.ToString("dd/MM/yyyy HH:mm"));
                RepTemp(ref baseTemplate, "VEHICULO", String.Format("{0} {1}", this.TipoVehiculo.Nombre, this.Patente));
                RepTemp(ref baseTemplate, "HORAENVIO", DateTime.Now.ToString("dd/MM/yy HH:mm:ss"));
                RepTemp(ref baseTemplate, "DESTINATARIOS", destinatarios);
                RepTemp(ref baseTemplate, "AUTORIZADORES", "");
                RepTemp(ref baseTemplate, "RESULTADOANALISIS", ViewHelpers.RenderRazorViewToString(ctx, "~/Views/Recepciones/DetalleAnalisis.cshtml", new DetalleAnalisisViewModel() { ProcesoIngreso = this, Columas = 2 }));

                string subject = String.Format("Rechazo en {2} para {1} de {0}", this.Agricultor.Nombre, this.CultivoContrato.Cultivo.Nombre, this.Sucursal.Nombre);

                SendMailLaboratorio(destinatarios, baseTemplate, subject);

                msg = "Se ha notificado del rechazo a los siguientes correos: " + destinatarios;
                return true;
            }
            catch (Exception ex)
            {
                msg = "Ocurrió un error al intentar notificar por correo: " + ex.Message;
                return false;
            }
        }

        public void RechazarSinRevisarAnalisis()
        {
            this.Autorizado = ANALISIS_RECHAZADO;
            this.AutorizadoAuto = true;
            this.MotivoRechazoAnalisis = "Marcado manualmente como rechazado";
        }

        public void ReStandarizar(AgroFichasDBDataContext dc)
        {
            string msg = "";
            if (!this.EsReStandarizable(out msg))
                throw new Exception(msg);

            //Recalculamos el Peso Standard
            this.PesoNormal = this.CalcularPesoNormal(dc);

            //Si está cerrado, es necesario recalcular los precios
            //Podemos re-estandarizar si existe a lo más 1 precio
            var precio = this.PrecioIngreso.FirstOrDefault();
            if (precio != null)
            {
                precio.Cantidad = this.PesoNormal.Value;

                var convenio = new CerrarViewModel.ConvenioParaCierre()
                {
                    PrecioUnidadBase = precio.PrecioUnidad,
                    BonoUnidad = precio.BonoUnidad,
                    TasaCambio = precio.TasaCambio.Value,
                    CantidadAsignadaIngreso = precio.Cantidad,
                    BonoCantidad = precio.BonoCantidad,
                    IdMoneda = precio.IdMoneda,
                    SobrePrecioPor = precio.SobrePrecioPor,
                    DescuentoPor = precio.DescuentoPor
                };

                precio.SobrePrecioTotal = convenio.SobrePrecioTotal(dc);
                precio.DescuentoTotal = convenio.DescuentoTotal(dc);
                precio.BonoTotal = convenio.BonoTotal(dc);
                precio.TotalNeto = convenio.TotalLinea(dc);
                this.TotalNetoRecepcion = precio.TotalNeto;
            }
        }

        public void SetUpValoresAnalisis(AgroFichasDBDataContext dc)
        {
            dc.ValorAnalisisInitialFill(this.IdProcesoIngreso);
        }

        public void ActualizaContactoAgricultor(AgroFichasDBDataContext dc, string email, string fono1, string fono2)
        {
            dc.ActualizaContactoAgricultor(this.IdAgricultor, email, fono1, fono2);
        }

        public bool SolicitoLiquidacionEnDolares(AgroFichasDBDataContext dc)
        {
            var items = dc.ProcesoIngreso.Where(p => p.IdTemporada == this.IdTemporada && p.IdEmpresa == this.IdEmpresa && p.IdAgricultor == this.IdAgricultor && p.Nulo == 0 && p.LiquidacionDolar);
            return items.Count() > 0;
        }

        public void UpdatePesoYNetoLiquidado()
        {
            this.PesoLiquidado = 0;
            this.TotalNetoLiquidacion = 0;
            foreach (var precio in this.PrecioIngreso.Where(p => p.IdLiquidacion.HasValue))
            {
                this.PesoLiquidado += precio.Cantidad;
                this.TotalNetoLiquidacion += precio.TotalNeto;
            }
        }

        public decimal? Valorizar(AgroFichasDBDataContext dc)
        {
            decimal precioCLP = 0;
            bool usingSpot = false;
            return Valorizar(dc, out precioCLP, out usingSpot);
        }

        public decimal? Valorizar(AgroFichasDBDataContext dc, out bool usingSpot)
        {
            decimal precioCLP = 0;
            return Valorizar(dc, out precioCLP, out usingSpot);
        }

        public decimal? Valorizar(AgroFichasDBDataContext dc, out decimal precioCLP, out bool usingSpot)
        {
            usingSpot = false;
            precioCLP = 0;
            if (this.TotalNetoLiquidacion.HasValue && this.PesoNoLiquidado.HasValue && this.PesoNoLiquidado.Value == 0)
            {
                if (this.PesoNormal.HasValue && this.PesoNormal.Value != 0)
                    precioCLP = TotalNetoLiquidacion.Value / this.PesoNormal.Value;

                return TotalNetoLiquidacion.Value;
            }

            if (this.TotalNetoRecepcion.HasValue)
            {
                if (this.PesoNormal.HasValue && this.PesoNormal.Value != 0)
                    precioCLP = TotalNetoRecepcion.Value / this.PesoNormal.Value;

                return TotalNetoRecepcion.Value;
            }
            var precioSpot = PrecioSpot.GetPrecioSpotCLP(dc, this.FechaHoraLlegada.Value.Date, this.CultivoContrato.IdCultivo, this.IdSucursal);
            if (precioSpot != 0 && this.PesoNormal.HasValue)
            {
                usingSpot = true;
                precioCLP = precioSpot;
                return this.PesoNormal.Value * precioSpot;
            }

            return null;
        }

        public decimal? ValorizarConIva(AgroFichasDBDataContext dc)
        {
            decimal precioCLP = 0;
            bool usingSpot = false;

            var valorNeto = Valorizar(dc, out precioCLP, out usingSpot);
            if (valorNeto.HasValue)
                return valorNeto.Value * (1M + Parametros.FactorIva);

            return null;
        }

        public decimal? ValorizarSinSpot(out decimal precioCLP)
        {
            precioCLP = 0;
            if (this.TotalNetoLiquidacion.HasValue && this.PesoNoLiquidado.HasValue && this.PesoNoLiquidado.Value == 0)
            {
                if (this.PesoNormal.HasValue && this.PesoNormal.Value != 0)
                    precioCLP = TotalNetoLiquidacion.Value / this.PesoNormal.Value;

                return TotalNetoLiquidacion.Value;
            }

            if (this.TotalNetoRecepcion.HasValue)
            {
                if (this.PesoNormal.HasValue && this.PesoNormal.Value != 0)
                    precioCLP = TotalNetoRecepcion.Value / this.PesoNormal.Value;

                return TotalNetoRecepcion.Value;
            }

            return null;
        }

        public Comuna ComunaOrigen(AgroFichasDBDataContext dc)
        {
            var idComuna = dc.ComunaOrigenIngreso(this.IdComunaOrigen, this.IdAgricultor, this.IdTemporada, this.CultivoContrato.IdCultivo);

            return dc.Comuna.SingleOrDefault(c => c.IdComuna == idComuna);
        }

        private void RepTemp(ref string template, string key, string value)
        {
            template = template.Replace("***" + key + "***", value);
        }

        private void SendMailLaboratorio(string destinatarios, string body, string subject)
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

        private void SendMailNotificaMaiz(string destinatarios, string body, string subject)
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

        public void IngresoDescuentoServicio(ProcesoIngreso ingreso, string username, string ipAddress)
        {
            try
            {
                PrecioServicio secado = dataContext.PrecioServicio.Where(ps => ps.IdCultivo == ingreso.CultivoContrato.IdCultivo && ps.IdSucursal == ingreso.IdSucursal && ps.Fecha <= ingreso.FechaHoraLlegada && ps.IdTipoServicio == 1 && ps.Habilitado == true).OrderByDescending(ps => ps.IdPrecioServicio).FirstOrDefault();
                PrecioServicio analisis = dataContext.PrecioServicio.Where(ps => ps.IdCultivo == ingreso.CultivoContrato.IdCultivo && ps.IdSucursal == ingreso.IdSucursal && ps.Fecha <= ingreso.FechaHoraLlegada && ps.IdTipoServicio == 2 && ps.Habilitado == true).OrderByDescending(ps => ps.IdPrecioServicio).FirstOrDefault();
                PrecioServicio limpieza = dataContext.PrecioServicio.Where(ps => ps.IdCultivo == ingreso.CultivoContrato.IdCultivo && ps.IdSucursal == ingreso.IdSucursal && ps.Fecha <= ingreso.FechaHoraLlegada && ps.IdTipoServicio == 3 && ps.Habilitado == true).OrderByDescending(ps => ps.IdPrecioServicio).FirstOrDefault();


                List<DescuentoServicio> descuentoServicio = new List<DescuentoServicio>();

                List<TipoServicio> tiposServicios = dataContext.TipoServicio.Where(ts => ts.Habilitado == true).ToList();

                foreach(var ts in tiposServicios)
                {
                    var descuentoServicioSecado1 = new DescuentoServicio()
                    {
                        TotalDescuento = 0,
                        ValorUnitario = 0,
                        Fecha = DateTime.Now,
                        IdProcesoIngreso = ingreso.IdProcesoIngreso,
                        IdTipoServicio = ts.IdTipoServicio,
                        IdPrecioServicio = null,
                        IdSucursal = ingreso.IdSucursal,
                        IdCultivo = ingreso.CultivoContrato.IdCultivo,
                        UserIns = username,
                        FechaHoraIns = DateTime.Now,
                        IpIns = ipAddress,
                        Habilitado = true
                    };

                    descuentoServicio.Add(descuentoServicioSecado1);
                }

                //var descuentoServicioSecado = new DescuentoServicio()
                //{
                //    TotalDescuento = 0,
                //    Fecha = DateTime.Now,
                //    IdProcesoIngreso = ingreso.IdProcesoIngreso,
                //    IdTipoServicio = .IdTipoServicio,
                //    IdPrecioServicio = secado.IdPrecioServicio,
                //    IdSucursal = secado.IdSucursal,
                //    IdCultivo = secado.IdCultivo,
                //    UserIns = username,
                //    FechaHoraIns = DateTime.Now,
                //    IpIns = ipAddress,
                //    Habilitado = true
                //};

                //descuentoServicio.Add(descuentoServicioSecado);

                //var descuentoServicioAnalisis = new DescuentoServicio()
                //{
                //    TotalDescuento = 0,
                //    Fecha = analisis.Fecha,
                //    IdProcesoIngreso = ingreso.IdProcesoIngreso,
                //    IdTipoServicio = analisis.IdTipoServicio,
                //    IdPrecioServicio = analisis.IdPrecioServicio,
                //    IdSucursal = analisis.IdSucursal,
                //    IdCultivo = analisis.IdCultivo,
                //    UserIns = username,
                //    FechaHoraIns = DateTime.Now,
                //    IpIns = ipAddress,
                //    Habilitado = true
                //};

                //descuentoServicio.Add(descuentoServicioAnalisis);

                //var descuentoServicioLimpieza = new DescuentoServicio()
                //{
                //    TotalDescuento = 0,
                //    Fecha = limpieza.Fecha,
                //    IdProcesoIngreso = ingreso.IdProcesoIngreso,
                //    IdTipoServicio = limpieza.IdTipoServicio,
                //    IdPrecioServicio = limpieza.IdPrecioServicio,
                //    IdSucursal = limpieza.IdSucursal,
                //    IdCultivo = limpieza.IdCultivo,
                //    UserIns = username,
                //    FechaHoraIns = DateTime.Now,
                //    IpIns = ipAddress,
                //    Habilitado = true
                //};

                //descuentoServicio.Add(descuentoServicioLimpieza);

                foreach (var ds in descuentoServicio)
                {
                    DescuentoServicio descuentoServicio1 = new DescuentoServicio();
                    descuentoServicio1.TotalDescuento = 0;
                    descuentoServicio1.ValorUnitario = 0;
                    descuentoServicio1.Fecha = ds.Fecha;
                    descuentoServicio1.IdProcesoIngreso = ds.IdProcesoIngreso;
                    descuentoServicio1.IdTipoServicio = ds.IdTipoServicio;
                    descuentoServicio1.IdPrecioServicio = ds.IdPrecioServicio;
                    descuentoServicio1.IdSucursal = ds.IdSucursal;
                    descuentoServicio1.IdCultivo = ds.IdCultivo;
                    descuentoServicio1.UserIns = username;
                    descuentoServicio1.FechaHoraIns = DateTime.Now;
                    descuentoServicio1.IpIns = ipAddress;
                    descuentoServicio1.Habilitado = true;

                    dataContext.DescuentoServicio.InsertOnSubmit(descuentoServicio1);
                    dataContext.SubmitChanges();
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}