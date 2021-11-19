using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.ViewModels.Recepciones
{
    public class Ingreso1StepViewModel
    {
        private AgroFichasDBDataContext dataContext = new AgroFichasDBDataContext();

        public Agricultor Agricultor { get; set; }
        public List<TipoVehiculo> TiposVehiculo { get; set; }
        public List<Sucursal> Sucursales { get; set; }
        public List<Bodega> Bodegas { get; set; }
        public List<Variedad> Variedades { get; set; }
        public Temporada Temporada { get; set; }
        public Cultivo Cultivo { get; set; }
        public List<CultivoContrato> CultivosContrato { get; set; }
        public List<Empresa> Empresas { get; set; }
        public List<Comuna> Comunas { get; set; }
        public List<TipoGuia> TipoGuias { get; set; }

        public int IdProcesoIngreso { get; set; }
        public int IdCultivo { get; set; }
        public int IdVariedad { get; set; }
        public int IdTemporada { get; set; }

        public int IdEmpresa { get; set; }
        public int IdAgricultor { get; set; }
        public int IdSucursal { get; set; }
        public int IdCultivoContrato { get; set; }
        public int IdComunaOrigen { get; set; }
        public int IdTipoVehiculo { get; set; }
        public string Patente { get; set; }
        public string Chofer { get; set; }
        public int NumeroGuia { get; set; }
        public bool UltimaEntrega { get; set; }
        public bool LiquidacionDolar { get; set; }

        public DateTime FechaLlegada { get; set; }
        public int IdBodega { get; set; }
        public int Manga { get; set; }
        public bool Secador { get; set; }
        public int PesoInicial { get; set; }
        public int PesoFinal { get; set; }

        public List<ValorAnalisisViewModel> ValoresAnalisis { get; set; }
        public string ObservacionesAnalisis { get; set; }

        public int? NroIngresoManual { get; set; }
        public int Autorizado { get; set; }
        public int? NroGuiaPropia { get; set; }

        public int IdTipoGuia { get; set; }

        public Ingreso1StepViewModel()
        {

        }

        public Ingreso1StepViewModel(AgroFichasDBDataContext dc, int idCultivo, int idTemporada, int userID)
        {
            this.IdCultivo = idCultivo;
            this.IdTemporada = idTemporada;
            this.FechaLlegada = DateTime.Today;
            this.Autorizado = ProcesoIngreso.ANALISIS_AUTORIZADO;
            this.IdComunaOrigen = 999999;

            if (this.IdCultivo == 1) //Raps
                this.IdEmpresa = 1; //Oleotop
            else if (this.IdCultivo == 2) //Trigo
                this.IdEmpresa = 3; //Granotop
            else if (this.IdCultivo == 3) //Avena
                this.IdEmpresa = 2; //Avenatop
            else if (this.IdCultivo == 4) //Lupino
                this.IdEmpresa = 3; //Granotop
            else if (this.IdCultivo == 16) //Linaza
                this.IdEmpresa = 3; //Granotop

            //Parámetros Análisis
            this.ValoresAnalisis = (from pa in dc.ParametroAnalisis
                                    where pa.IdCultivo == this.IdCultivo
                                      && pa.Habilitado
                                    orderby pa.Orden
                                    select new ValorAnalisisViewModel()
                                    {
                                        IdParametroAnalisis = pa.IdParametroAnalisis,
                                        Nombre = pa.Nombre,
                                        UM = pa.UM,
                                        Orden = pa.Orden,
                                        Requerido = pa.Requerido
                                    }).ToList();

            LoadLookups(dc, userID);
        }

        public void LoadLookups(AgroFichasDBDataContext dc, int userID)
        {
            this.Cultivo = dc.Cultivo.Single(c => c.IdCultivo == this.IdCultivo);
            this.Agricultor = dc.Agricultor.SingleOrDefault(a => a.IdAgricultor == this.IdAgricultor && a.Habilitado);
            this.Bodegas = dc.Bodega.Where(b => b.Habilitada).ToList();
            this.TiposVehiculo = dc.TipoVehiculo.OrderBy(tv => tv.IdTipoVehiculo).ToList();

            this.Sucursales = (from us in dc.SucursalUsuario
                               join su in dc.Sucursal on us.IdSucursal equals su.IdSucursal
                               where us.UserID == userID
                                  && us.ZoneToken == "recepcion"
                                  && su.Habilitada
                               select su).ToList();

            this.Temporada = dc.Temporada.Single(t => t.IdTemporada == this.IdTemporada);
            this.CultivosContrato = dc.CultivoContrato.Where(cc => cc.IdCultivo == this.IdCultivo).ToList();
            this.Empresas = dc.Empresa.ToList();
            this.Variedades = dc.Variedad.Where(v => v.IdCultivo == this.IdCultivo).OrderBy(v => v.Nombre).ToList();
            this.Comunas = dc.Comuna.OrderBy(c => c.IdComuna).ToList();
            this.TipoGuias = dc.TipoGuia.ToList();
        }


        public void Validate(AgroFichasDBDataContext dc, ModelStateDictionary modelState)
        {
            List<ProcesoIngreso> procesos1 = dataContext.ProcesoIngreso.Where(X => X.IdAgricultor == this.IdAgricultor && X.NumeroGuia == this.NumeroGuia && X.IdTipoGuia == this.IdTipoGuia && X.IdEstado != 99).ToList();

            if (this.IdAgricultor <= 0)
                modelState.AddModelError("IdAgricultor", "El Agricultor es requerido");

            if (this.IdSucursal <= 0)
                modelState.AddModelError("IdSucursal", "La Sucursal es requerida");

            if (this.IdComunaOrigen == 999999)
                modelState.AddModelError("IdComunaOrigen", "La Comuna de origen es requerida");

            if (this.IdEmpresa <= 0)
                modelState.AddModelError("IdEmpresa", "La Empresa es requerida");

            if (this.IdCultivoContrato <= 0)
                modelState.AddModelError("IdCultivoContrato", "El Cultivo es requerido");

            if (this.IdTipoVehiculo <= 0)
                modelState.AddModelError("IdTipoVehiculo", "El Tipo de Vehículo es requerido");

            if (this.NumeroGuia <= 0)
                modelState.AddModelError("NumeroGuia", "El Número de la Guía no es válido");
            else if (procesos1.Count == 1)
            {
                ProcesoIngreso procesoIngreso = procesos1.SingleOrDefault(X => X.IdAgricultor == this.IdAgricultor && X.NumeroGuia == this.NumeroGuia && X.IdTipoGuia == this.IdTipoGuia && X.IdEstado != 99);
                if (procesoIngreso != null)
                {
                    modelState.AddModelError("NumeroGuia", "El Número de la Guía está asignado a otra llegada");
                }
                else
                {
                    //Pasa
                }
            }
            else if (procesos1.Count > 1)
            {
                modelState.AddModelError("NumeroGuia", "El Número de la Guía está asignado en múltiples llegadas");
            }
            else
            {
                //Pasa
            }

            if (String.IsNullOrWhiteSpace(this.Patente) || this.Patente.Trim().Length < 6)
                modelState.AddModelError("Patente", "La Patente no es válida");

            if (String.IsNullOrWhiteSpace(this.Chofer))
                modelState.AddModelError("Chofer", "El Chofer es requerido");

            if (this.IdBodega <= 0)
                modelState.AddModelError("IdBodega", "La Bodega es requerida");

            if (dc.Bodega.Single(b => b.IdBodega == this.IdBodega).EsManga && this.Manga <= 0)
                modelState.AddModelError("Manga", "El Número de la Manga es requerido");

            if (this.IdVariedad <= 0)
                modelState.AddModelError("IdVariedad", "La Variedad es requerida");

            if (this.NroIngresoManual.HasValue && this.NroIngresoManual.Value <= 0)
                modelState.AddModelError("NroIngresoManual", "El Nro de Ingreso Manual no es válido. Si no existe, déjelo en blanco");

            if (this.NroGuiaPropia.HasValue && this.NroGuiaPropia.Value <= 0)
                modelState.AddModelError("NroGuiaPropia", "El Nro de Guía Propia no es válido. Si no existe, déjelo en blanco");

            if (this.Autorizado != ProcesoIngreso.ANALISIS_AUTORIZADO && this.Autorizado != ProcesoIngreso.ANALISIS_RECHAZADO)
                modelState.AddModelError("Autorizado", "El valor de Autorización no es válido");

            if (this.Autorizado == ProcesoIngreso.ANALISIS_AUTORIZADO)
            {
                if (this.PesoInicial <= 0 || this.PesoInicial > 100000)
                    modelState.AddModelError("PesoInicial", "El Peso Inicial no es válido");

                if (this.PesoFinal <= 0 || this.PesoFinal > 100000)
                    modelState.AddModelError("PesoFinal", "El Peso Final no es válido");
            }
            else if (this.Autorizado == ProcesoIngreso.ANALISIS_RECHAZADO)
            {
                if (this.PesoInicial != 0)
                    modelState.AddModelError("PesoInicial", "El Peso Inicial debe ser cero por que el ingreso está rechazado");

                if (this.PesoFinal != 0)
                    modelState.AddModelError("PesoFinal", "El Peso Final debe ser cero por que el ingreso está rechazado");
            }

            int i = 0;
            if (this.ValoresAnalisis != null)
            {
                foreach (var valor in this.ValoresAnalisis)
                {
                    valor.Validate(dc, modelState, String.Format("ValoresAnalisis[{0}].Valor", i));
                    i++;
                }
            }
        }

        public ProcesoIngreso Persist(AgroFichasDBDataContext dc, string userName, string ipAddress)
        {
            var ingreso = new ProcesoIngreso()
            {
                IdAgricultor = this.IdAgricultor,
                IdTemporada = this.IdTemporada,
                IdSucursal = this.IdSucursal,
                IdCultivoContrato = this.IdCultivoContrato,
                IdVariedad = this.IdVariedad,
                IdEmpresa = this.IdEmpresa,
                IdComunaOrigen = this.IdComunaOrigen,
                IdTipoVehiculo = this.IdTipoVehiculo,
                Patente = this.Patente.Trim().ToUpper(),
                Chofer = this.Chofer.Trim(),
                NumeroGuia = this.NumeroGuia,
                UltimaEntrega = this.UltimaEntrega,
                LiquidacionDolar = this.LiquidacionDolar,
                ObservacionesAnalisis = this.ObservacionesAnalisis,
                NroIngresoManual = this.NroIngresoManual,
                NroGuiaPropia = this.NroGuiaPropia,

                FechaHoraLlegada = this.FechaLlegada,
                UserLlegada = userName,
                IpLlegada = ipAddress,
                IdBodega = this.IdBodega,
                Secador = this.Secador,

                FechaHoraTomaMuestra = this.FechaLlegada,
                UserTomaMuestra = userName,
                IpTomaMuestra = ipAddress,

                FechaHoraLaboratorio = this.FechaLlegada,
                UserLaboratorio = userName,
                IpLaboratorio = ipAddress,

                FechaHoraAnalisis = this.FechaLlegada,
                UserAnalisis = userName,
                IpAnalisis = ipAddress,

                UserIns = userName,
                FechaHoraIns = DateTime.Now,
                IpIns = ipAddress,
                Nulo = 0,

                IdTipoGuia = this.IdTipoGuia
            };

            if (dc.Bodega.Single(b => b.IdBodega == this.IdBodega).EsManga)
                ingreso.Manga = this.Manga;
            else
                ingreso.Manga = null;

            if (this.Autorizado == ProcesoIngreso.ANALISIS_AUTORIZADO)
            {
                ingreso.PesoInicial = this.PesoInicial;
                ingreso.FechaPesoInicial = this.FechaLlegada;
                ingreso.UserPesoInicial = userName;
                ingreso.IpPesoInicial = ipAddress;

                ingreso.PesoFinal = this.PesoFinal;
                ingreso.FechaPesoFinal = this.FechaLlegada;
                ingreso.UserPesoFinal = userName;
                ingreso.IpPesoFinal = ipAddress;
            }

            dc.ProcesoIngreso.InsertOnSubmit(ingreso);

            //Análisis
            if (this.ValoresAnalisis != null)
            {
                foreach (var valor in this.ValoresAnalisis)
                {
                    ingreso.ValorAnalisis.Add(new ValorAnalisis()
                    {
                        IdParametroAnalisis = valor.IdParametroAnalisis,
                        Valor = valor.Valor,
                        UserIns = userName,
                        FechaHoraIns = DateTime.Now,
                        IpIns = ipAddress
                    });
                }
            }

            //Autorizamos no más
            if (this.Autorizado == ProcesoIngreso.ANALISIS_AUTORIZADO)
            {
                ingreso.AutorizarSinRevisarAnalisis();

                //Calculamos peso normal
                ingreso.PesoBruto = Math.Abs(ingreso.PesoInicial.Value - ingreso.PesoFinal.Value);
                ingreso.PesoNormal = ingreso.CalcularPesoNormal(dc);
            }
            else if (this.Autorizado == ProcesoIngreso.ANALISIS_RECHAZADO)
            {
                ingreso.RechazarSinRevisarAnalisis();

                //No hay peso normal
                ingreso.PesoBruto = null;
                ingreso.PesoNormal = null;
            }
            else
            {
                throw new Exception("Valor de Autorizado no válido");
            }

            dc.SubmitChanges();

            return ingreso;
        }
    }
}