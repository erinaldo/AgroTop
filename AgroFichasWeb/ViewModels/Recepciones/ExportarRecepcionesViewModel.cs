using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.ViewModels.Recepciones
{
    public class ExportarRecepcionesViewModel
    {
        public Temporada Temporada { get; set; }
        public Empresa Empresa { get; set; }
        public int IdTemporada { get; set; }
        public int IdEmpresa { get; set; }
        public List<ExportarRecepcionesItem> Items { get; set; }

        public ExportarRecepcionesViewModel()
        {

        }

        public ExportarRecepcionesViewModel(AgroFichasDBDataContext dc, int idTemporada, int idEmpresa)
        {
            this.IdTemporada = idTemporada;
            this.IdEmpresa = idEmpresa;

            var estados = new int[] { 10 }; //Cerrado o Liquidado

            Items = (from  pi in dc.ProcesoIngreso
                    where pi.IdEmpresa == idEmpresa
                       && pi.IdTemporada == idTemporada
                       && estados.Contains(pi.IdEstado)
                       && !pi.IdExportBatch.HasValue
                   orderby pi.IdProcesoIngreso
                   select ExportarRecepcionesItem.FromProcesoIngreso(pi)).ToList();

            LoadLookups(dc);
        }

        public void LoadLookups(AgroFichasDBDataContext dc)
        {
            this.Temporada = dc.Temporada.Single(t => t.IdTemporada == this.IdTemporada);
            this.Empresa = dc.Empresa.Single(e => e.IdEmpresa == this.IdEmpresa);
        }

        public void Validate(AgroFichasDBDataContext dc, ModelStateDictionary modelState)
        {
            if (this.Items == null || this.Items.Where(i => i.Seleccionado).Count() == 0)
            {
                modelState.AddModelError("", "Seleccione al menos 1 ingreso a exportar");
            }
        }

        public ExportBatch Persist(AgroFichasDBDataContext dc, string userName, string ipAddress)
        {
            var batch = new ExportBatch()
            {
                IdTipoExportBatch = 1,
                IdEmpresa = this.IdEmpresa,
                IdTemporada = this.IdTemporada,
                Fecha = DateTime.Today,
                UserIns = userName,
                IpIns = ipAddress,
                FechaHoraIns = DateTime.Now,
                Nulo = false
            };

            foreach (var item in this.Items)
            {
                if (item.Seleccionado)
                {
                    var pi = dc.ProcesoIngreso.Single(p => p.IdProcesoIngreso == item.IdProcesoIngreso);
                    batch.ProcesoIngreso.Add(pi);
                }
            }

            dc.ExportBatch.InsertOnSubmit(batch);
            dc.SubmitChanges();

            return batch;
        }
    }

    public class ExportarRecepcionesItem
    {
        public bool Seleccionado { get; set; }
        public bool EsExportable { get; set; }
        public string MotivoNoExportable { get; set; }
        public int IdProcesoIngreso { get; set; }
        public DateTime Fecha { get; set; }
        public string Rut { get; set; }
        public string Nombre { get; set; }
        public int PesoBruto { get; set; }
        public int PesoNormal { get; set; }
        public string NombreSucursal { get; set; }
        public string NombreBodega { get; set; }
        public string NombreCultivoContrato { get; set; }
        public string IdBodegaSAP { get; set; }
        public string IdAgricultorSAP { get; set; }
        public string IdProductoSAP { get; set; }
        public int? IdLiquidacion { get; set; }

        public static ExportarRecepcionesItem FromProcesoIngreso(ProcesoIngreso pi)
        {
            string motivoNoExportable = "";
            var exportable = pi.EsExportable(out motivoNoExportable);

            return new ExportarRecepcionesItem()
            {
                Seleccionado = exportable,
                EsExportable = exportable,
                MotivoNoExportable = motivoNoExportable,
                IdProcesoIngreso = pi.IdProcesoIngreso,
                Fecha = pi.FechaHoraLlegada.Value,
                Rut = pi.Agricultor.Rut,
                Nombre = pi.Agricultor.Nombre,
                PesoBruto = pi.PesoBruto.Value,
                PesoNormal = pi.PesoNormal.Value,
                NombreSucursal = pi.Sucursal.Nombre,
                NombreBodega = pi.Bodega.Nombre,
                NombreCultivoContrato = pi.CultivoContrato.Nombre,
                IdBodegaSAP = pi.Bodega.SAPID(pi.IdEmpresa),
                IdAgricultorSAP = pi.Agricultor.SAPID(pi.IdEmpresa),
                IdProductoSAP = pi.CultivoContrato.SAPID,
                IdLiquidacion = pi.IdPrimeraLiquidacion()
            };
        }
    }
}