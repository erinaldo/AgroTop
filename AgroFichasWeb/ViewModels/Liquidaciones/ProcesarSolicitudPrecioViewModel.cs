using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.ViewModels.Liquidaciones
{
    public class ProcesarSolicitudPrecioViewModel
    {
        public int IdSolicitudPrecio { get; set; }
        public int? IdContrato { get; set; }
        public string Action { get; set; } 

        public SolicitudPrecio Solicitud { get; set; } 
        public List<Contrato> ContratosCandidatos { get; set; }

        public ProcesarSolicitudPrecioViewModel()
        {

        }

        public ProcesarSolicitudPrecioViewModel(AgroFichasDBDataContext dc, int idSolicitudPrecio)
        {
            this.IdSolicitudPrecio = idSolicitudPrecio;
            this.IdContrato = null;

            LoadLookups(dc);
        }

        public void LoadLookups(AgroFichasDBDataContext dc)
        {
            this.Solicitud = dc.SolicitudPrecio.Single(s => s.IdSolicitudPrecio == this.IdSolicitudPrecio);
            this.ContratosCandidatos = (from c in dc.Contrato
                                        join i in dc.ItemContrato on c.IdContrato equals i.IdContrato
                                        join x in dc.CultivoContrato on i.IdCultivoContrato equals x.IdCultivoContrato
                                        where c.Habilitado
                                           && c.IdAgricultor == this.Solicitud.IdAgricultor
                                           && c.IdTemporada == this.Solicitud.IdTemporada
                                           && x.IdCultivo == this.Solicitud.IdCultivo
                                        select c).Distinct().ToList();
        }

        public void Validate(ModelStateDictionary modelState)
        {
            if (this.Action == "ACEPTAR")
            {
                if (!this.IdContrato.HasValue)
                    modelState.AddModelError("IdContrato", "Seleccione el contrato");
            }
        }

        public SolicitudPrecio Persist(ControllerContext ctx, AgroFichasDBDataContext dc, string userName, string ipAddress, out string msgAutorizacion)
        {
            msgAutorizacion = "";

            if (this.Action == "ACEPTAR")
            {
                var model = new ConvenioPrecioViewModel()
                {
                    Cantidad = this.Solicitud.Cantidad,
                    EsPiso = false,
                    Habilitado = true,
                    IdContrato = this.IdContrato.Value,
                    IdMoneda = this.Solicitud.IdMoneda,
                    PrecioUnidad = this.Solicitud.PrecioUnidad,
                    Sucursales = (from suc in this.Solicitud.SolicitudPrecioSucursal
                                  select new SelectorSucursalViewModel {
                                      IdSucursal = suc.IdSucursal,
                                      NombreSucursal = suc.Sucursal.Nombre,
                                      Seleccionado = true
                                  }).ToList(),
                };

                model.LoadLookups(dc);
                model.SetDefaults();

                //Si es autorizado automáticamente, el convenio de precio es creado altiro
                if (model.ValidateAutorizacion(ctx, userName, ipAddress, out int idConvenioPrecioAutorizacion, out string msg))
                {
                    var convenio = model.Persist(ctx, dc, userName, ipAddress, out bool requiereNotificacion);
                    this.Solicitud.IdConvenioPrecio = convenio.IdConvenioPrecio;

                    //if (requiereNotificacion)
                    //{
                    //    //return RedirectToAction("Notificar", new { id = convenio.IdConvenioPrecio, backto = Request["backto"] });
                    //}
                    //else
                    //{
                    //    //if (!String.IsNullOrEmpty(Request["backto"]))
                    //    //    return Redirect(Request["backto"]);
                    //    //else
                    //    //    return RedirectToAction("Index");
                    //}
                }
                else //Es necesario atorizar
                {
                    this.Solicitud.IdConvenioPrecioAutorizacion = idConvenioPrecioAutorizacion;
                    msgAutorizacion = msg;
                    //return RedirectToAction("AutorizacionRequerida", new { id = idConvenioPrecioAutorizacion, msgok = msg });
                }


            }

            this.Solicitud.Procesado = true;
            this.Solicitud.FechaHoraProc = DateTime.Now;
            this.Solicitud.UserProc = userName;
            this.Solicitud.IpProc = ipAddress;


            dc.SubmitChanges();

            return this.Solicitud;
        }

    }
}