using AgroFichasWeb.ViewModels;
using AgroFichasWeb.ViewModels.Liquidaciones;
using AgroFichasWeb.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace AgroFichasWeb.Models
{
    public partial class ConvenioPrecio
    { 
        public static int GetCantidaUtilizada(AgroFichasDBDataContext dc, int idConvenioPrecio)
        {
            var utilizado = dc.PrecioIngreso.Where(x =>
                x.IdConvenioPrecio == idConvenioPrecio).Sum(x => (int?)x.Cantidad) ?? 0;

            return utilizado;
        }

        public static int NextPrioridad(AgroFichasDBDataContext dc, int idContrato)
        {
            int? p = ( from cp in dc.ConvenioPrecio
                      where cp.IdContrato == idContrato
                      select (int?)cp.Prioridad).Max();

            return (p ?? 0) + 1;
        }

        public static List<ConvenioPrecioItemTabla> CalcularTabla(int cantidadBase, decimal precioUnidadBase, List<int> sucursalesBase, List<ConvenioPrecioAjuste> ajustes)
        {
            var dc = new AgroFichasDBDataContext();
            var itemsBase = new List<ConvenioPrecioItemTabla>();
            var itemsAjuste = new List<ConvenioPrecioItemTabla>();

            foreach (var idSucursal in sucursalesBase)
            {
                var item = new ConvenioPrecioItemTabla()
                {
                    Sucursal = dc.Sucursal.SingleOrDefault(s => s.IdSucursal == idSucursal),
                    Cantidad = cantidadBase,
                    PrecioUnidad = precioUnidadBase,
                    Autorizado = null,
                    Autorizador = null,
                };

                itemsBase.Add(item);
            }

            foreach (var ajuste in ajustes)
            {
                foreach (var suc in ajuste.ConvenioPrecioAjusteSucursal)
                {
                    var newItem = new ConvenioPrecioItemTabla()
                    {
                        Sucursal = dc.Sucursal.SingleOrDefault(s => s.IdSucursal == suc.IdSucursal),
                        Cantidad = ajuste.Cantidad,
                        PrecioUnidad = ajuste.PrecioUnidad + precioUnidadBase,
                        Autorizado = null,
                        Autorizador = null,
                    };
                    itemsAjuste.Add(newItem);

                    var itemBase = itemsBase.SingleOrDefault(t => t.IdSucursal == suc.IdSucursal);
                    if (itemBase != null)
                    {
                        itemBase.Cantidad -= newItem.Cantidad;
                    }
                }
            }

            //Agrupamos por sucursal y precio, sumando cantidades
            foreach (var item in itemsAjuste)
            {
                var existingItem = itemsBase.SingleOrDefault(ib => ib.IdSucursal == item.IdSucursal && ib.PrecioUnidad == item.PrecioUnidad);
                if (existingItem == null)
                    itemsBase.Add(item);
                else
                    existingItem.Cantidad += item.Cantidad;
            }

            //Eliminamos items en 0
            itemsBase.RemoveAll(ib => ib.Cantidad == 0);

            return itemsBase;
        }

        public static List<ItemTablaPrecio> ToListOfItemTablaPrecio(List<ConvenioPrecioItemTabla> tabla)
        {
            var simpleTabla = (from item in tabla
                               select new ItemTablaPrecio()
                               {
                                   NombreSucursal = item.Sucursal.Nombre,
                                   Cantidad = item.Cantidad,
                                   PrecioUnidad = item.PrecioUnidad,
                                   Autorizado = item.Autorizado,
                                   Autorizador = item.Autorizador
                               }).ToList();

            return simpleTabla;
        }

        public static bool EsTablaValida(int cantidadBase, List<ConvenioPrecioItemTabla> tabla, out List<string> msg)
        {
            msg = new List<string>();
            var result = true;

            foreach (var item in tabla)
            {
                if (item.Cantidad < 0)
                {
                    result = false;
                    msg.Add($"La cantidad de {item.Cantidad.ToString("#,##0")} Kg para la sucursal {item.Sucursal.Nombre} no es válida porque es negativa.");
                }
                else if (item.Cantidad > cantidadBase)
                {
                    result = false;
                    msg.Add($"La cantidad de {item.Cantidad.ToString("#,##0")} Kg para la sucursal {item.Sucursal.Nombre} no es válida porque es mayor que la cantidad total del convenio.");
                }
            }

            return result;
        }

        public List<ConvenioPrecioItemTabla> CalcularTabla()
        {
            var sucursalesBase = this.ConvenioPrecioSucursal.Select(s => s.IdSucursal).ToList();

            return ConvenioPrecio.CalcularTabla(this.Cantidad, this.PrecioUnidad, sucursalesBase, this.ConvenioPrecioAjuste.ToList());
        }

        public List<DestinatarioNotificacion> DestanatariosNotificacion()
        {
            var dc = new AgroFichasDBDataContext();
            return (from d in dc.ReceptoresNotificacionConvenioPrecio(this.IdConvenioPrecio)
                   select new DestinatarioNotificacion()
                   {
                       Rol = d.Rol,
                       Email = d.Email,
                       Nombre = d.Nombre,
                       Seleccionado = true,
                       Optional = d.Optional ?? false
                   }).ToList();
        }

        public bool NotificarActualizacion(AgroFichasDBDataContext dc, ControllerContext ctx,List<DestinatarioNotificacion> receptores, out string msg)
        {
            msg = "";
            try
            {
                string destinatarios = "";
                string sucursalesEntrega = "";

                if (receptores.Count == 0)
                {
                    msg = "No se indicaron destinatarios para la notificación.";
                    return false;
                }

                foreach (var receptor in receptores)
                {
                    var email = receptor.Email;
                    destinatarios += (destinatarios != "" ? ", " : "") + email;
                }

                foreach (var suc in this.ConvenioPrecioSucursal)
                {
                    sucursalesEntrega += (sucursalesEntrega != "" ? ", " : "") + suc.Sucursal.Nombre;
                }

                string baseTemplate = null;
                baseTemplate = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/notificacionconvenioprecio_template.html"), Encoding.UTF7);
                RepTemp(ref baseTemplate, "AGRICULTOR", this.Contrato.Agricultor.Nombre);
                RepTemp(ref baseTemplate, "RUT", this.Contrato.Agricultor.Rut);
                RepTemp(ref baseTemplate, "TEMPORADA", this.Contrato.Temporada.Nombre);
                RepTemp(ref baseTemplate, "IDCONVENIOPRECIO", this.IdConvenioPrecio.ToString());
                RepTemp(ref baseTemplate, "CANTIDAD", this.Cantidad.ToString("#,##0"));
                RepTemp(ref baseTemplate, "PRECIO", String.Format("{0:#,##0.00##} {1}/Kg", this.PrecioUnidad, this.Moneda.Simbolo));
                RepTemp(ref baseTemplate, "CONTRATO", this.Contrato.NumeroContrato + " - " + this.Contrato.Empresa.Nombre + " - " + this.Contrato.DescripcionCultivos(", "));
                RepTemp(ref baseTemplate, "SUCURSALES", sucursalesEntrega);
                RepTemp(ref baseTemplate, "HORAENVIO", DateTime.Now.ToString("dd/MM/yy HH:mm:ss"));
                RepTemp(ref baseTemplate, "DESTINATARIOS", destinatarios);

                var model = ConvenioPrecioViewModel.Create(dc, this.IdConvenioPrecio);
                RepTemp(ref baseTemplate, "AJUSTES", ViewHelpers.RenderRazorViewToString(ctx, "~/Views/ConveniosPrecio/DetalleAjustes.cshtml", model));

                string subject = String.Format("Actualización de Convenio de Precio para {0}", this.Contrato.Agricultor.Nombre);

                var objMM = new MailMessage
                {
                    From = new MailAddress("notificaciones@saprosem.cl", "Notificaciones Agrotop"),
                    Subject = subject,
                    IsBodyHtml = true,
                    Body = baseTemplate
                };
                objMM.To.Add(destinatarios);

                var objSmtp = new SmtpClient();
                objSmtp.Send(objMM);
                //}

                msg = "Se ha notificado que el convenio de precio fue actualizado a los siguientes correos: " + destinatarios;
                return true;
            }
            catch (Exception ex)
            {
                msg = "Ocurrió un error al intentar notificar por correo: " + ex.Message;
                return false;
            }
        }

        private void RepTemp(ref string Template, string Key, string Value)
        {
            Template = Template.Replace("***" + Key + "***", Value);
        }

        private void WriteCData(ref XmlTextWriter w, string Name, string Value)
        {
            w.WriteStartElement(Name);
            w.WriteCData(Value);
            w.WriteEndElement();
        }
    }
}