using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;


namespace AgroFichasWeb.ViewModels.Recepciones
{
    public class LlegadaViewModel
    {
        private AgroFichasDBDataContext dataContext = new AgroFichasDBDataContext();

        public Agricultor Agricultor { get; set; }
        public List<TipoVehiculo> TiposVehiculo { get; set; }
        public List<Sucursal> Sucursales { get; set; }
        public List<CultivoEmpresa> CultivosEmpresas { get; set; }
        public List<Variedad> Variedades { get; set; }
        public List<CultivoContrato> CultivosContrato { get; set; }
        public List<Comuna> Comunas { get; set; }
        public List<TipoGuia> TipoGuias { get; set; }

        public Temporada Temporada { get; set; }

        public int IdAgricultor { get; set; }
        public int IdTemporada { get; set; }
        public string email { get; set; }
        public string fono1 { get; set; }
        public string fono2 { get; set; }

        public int IdSucursal { get; set; }
        public string IdCultivoEmpresa { get; set; }
        public int IdTipoVehiculo { get; set; }
        public string Patente { get; set; }
        public string Chofer { get; set; }
        public int NumeroGuia { get; set; }
        public bool UltimaEntrega { get; set; }
        public bool LiquidacionDolar { get; set; }
        public int IdVariedad { get; set; }
        public int IdComunaOrigen { get; set; }

        public string Pin { get; set; }
        private SYS_User PinUser = null;

        public int IdTipoGuia { get; set; }

        public int IdTipoServicio { get; set; }

        public LlegadaViewModel()
        {

        }

        public LlegadaViewModel(AgroFichasDBDataContext dc, int idAgricultor, int idTemporada, int userID)
        {
            this.IdAgricultor = idAgricultor;
            this.IdTemporada = idTemporada;
            this.IdComunaOrigen = 999999;

            LoadLookups(dc, userID);
        }

        public void LoadLookups(AgroFichasDBDataContext dc, int userID)
        {
            Agricultor = dc.Agricultor.Single(a => a.IdAgricultor == this.IdAgricultor && a.Habilitado);
            var idsRelacionados = this.Agricultor.IdsRelacionadosHermanos(dc, true, true);

            CultivosEmpresas = (from co in dc.Contrato
                                join ic in dc.ItemContrato on co.IdContrato equals ic.IdContrato
                                where co.Habilitado
                                   && co.IdTemporada == this.IdTemporada
                                   && (co.IdAgricultor == this.Agricultor.IdAgricultor || idsRelacionados.Contains(co.IdAgricultor))
                                select new CultivoEmpresa
                                {
                                    IdCultivoEmpresa = ic.IdCultivoContrato.ToString() + "," + ic.Contrato.IdEmpresa.ToString(),
                                    Nombre = ic.CultivoContrato.Nombre + " " + ic.Contrato.Empresa.Nombre
                                }).ToList().Distinct(new CultivoEmpresaComparer()).ToList();

            //Para permitir ingreso de Raps a Oleotop aunque no haya contrato
            var empresa = dc.Empresa.SingleOrDefault(e => e.IdEmpresa == 1); //oleotop
            if (empresa != null)
            {
                var cultivosSinContrato = (from cu in dc.CultivoContrato
                                           where cu.IdCultivo == 1 //Raps
                                           select new CultivoEmpresa()
                                           {
                                               IdCultivoEmpresa = cu.IdCultivoContrato.ToString() + "," + empresa.IdEmpresa.ToString(),
                                               Nombre = cu.Nombre + " " + empresa.Nombre + " - SIN CONTRATO"
                                           }).ToList();
                foreach (var cc in cultivosSinContrato)
                    if (CultivosEmpresas.Where(ce => ce.IdCultivoEmpresa == cc.IdCultivoEmpresa).Count() == 0)
                        CultivosEmpresas.Add(cc);
            }


            TiposVehiculo = dc.TipoVehiculo.OrderBy(tv => tv.IdTipoVehiculo).ToList();

            Variedades = dc.Variedad.Where(v => v.Habilitado).OrderBy(v => v.Nombre).ToList();
            CultivosContrato = dc.CultivoContrato.ToList();

            Sucursales = (from us in dc.SucursalUsuario
                          join su in dc.Sucursal on us.IdSucursal equals su.IdSucursal
                          where us.UserID == userID
                             && us.ZoneToken == "recepcion"
                             && su.Habilitada
                          select su).ToList();

            Comunas = dc.Comuna.OrderBy(c => c.Nombre).ToList();

            Temporada = dc.Temporada.Single(t => t.IdTemporada == this.IdTemporada);

            TipoGuias = dc.TipoGuia.ToList();
        }



        public void Validate(ModelStateDictionary modelState)
        {
            List<ProcesoIngreso> procesos1 = dataContext.ProcesoIngreso.Where(X => X.IdAgricultor == this.IdAgricultor && X.NumeroGuia == this.NumeroGuia && X.IdTipoGuia == this.IdTipoGuia && X.IdEstado != 99).ToList();

            if (this.IdAgricultor <= 0)
                modelState.AddModelError("IdAgricultor", "El Agricultor es requerido");
            if (this.IdVariedad > 84 && this.IdVariedad < 112 && String.IsNullOrWhiteSpace(this.email))
            {
                modelState.AddModelError("eMail", "El eMail es requerido");
            }
            else
            {
                if (!this.VerificarMail(this.email))
                {
                    modelState.AddModelError("eMail", "Verifique el e-Mail");
                }
            }
            if (this.IdSucursal <= 0)
                modelState.AddModelError("IdSucursal", "La Sucursal es requerida");
            if (String.IsNullOrWhiteSpace(this.IdCultivoEmpresa))
                modelState.AddModelError("IdCultivoEmpresa", "El Cultivo es requerido");
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
            if (this.IdVariedad <= 0)
                modelState.AddModelError("IdVariedad", "La Variedad es requerida");

            this.PinUser = SYS_User.UserFromPin(this.Pin, 23);
            if (PinUser == null)
                modelState.AddModelError("Pin", "El PIN no es válido");
        }

        public ProcesoIngreso Persist(AgroFichasDBDataContext dc, string ipAddress)
        {
            var cultemp = this.IdCultivoEmpresa.Split(',');
            var email = this.email == null ? "" : this.email;
            var fono1 = this.fono1 == null ? "" : this.fono1;
            var fono2 = this.fono2 == null ? "" : this.fono2;
            var ingreso = new ProcesoIngreso()
            {
                IdAgricultor = this.IdAgricultor,
                IdTemporada = this.IdTemporada,
                IdSucursal = this.IdSucursal,
                IdComunaOrigen = this.IdComunaOrigen,
                IdCultivoContrato = int.Parse(cultemp[0]),
                IdEmpresa = int.Parse(cultemp[1]),
                IdTipoVehiculo = this.IdTipoVehiculo,
                IdVariedad = this.IdVariedad,
                Patente = this.Patente.Trim().ToUpper(),
                Chofer = this.Chofer.Trim(),
                NumeroGuia = this.NumeroGuia,
                UltimaEntrega = this.UltimaEntrega,
                LiquidacionDolar = this.LiquidacionDolar,
                FechaHoraLlegada = DateTime.Now,
                UserLlegada = this.PinUser.UserName,
                IpLlegada = ipAddress,
                UserIns = this.PinUser.UserName,
                FechaHoraIns = DateTime.Now,
                IpIns = ipAddress,
                Nulo = 0,
                IdTipoGuia = this.IdTipoGuia,
                IdTipoServicio = this.IdTipoServicio
            };

            dc.ProcesoIngreso.InsertOnSubmit(ingreso);
            dc.SubmitChanges();

            ingreso.SetUpValoresAnalisis(dc);
            ingreso.ActualizaContactoAgricultor(dc, email, fono1, fono2);
            if (ingreso.CultivoContrato.IdCultivo == 10)
            {
                ingreso.IngresoDescuentoServicio(ingreso, PinUser.UserName, ipAddress);
            }

            return ingreso;
        }

        public class CultivoEmpresa
        {
            public string IdCultivoEmpresa { get; set; }
            public string Nombre { get; set; }
        }

        public class CultivoEmpresaComparer : IEqualityComparer<CultivoEmpresa>
        {
            public bool Equals(CultivoEmpresa x, CultivoEmpresa y)
            {
                return x.IdCultivoEmpresa == y.IdCultivoEmpresa;
            }

            public int GetHashCode(CultivoEmpresa obj)
            {
                return obj.IdCultivoEmpresa.GetHashCode();
            }
        }

        private bool VerificarMail(String email)
        {
            String expresion;
            expresion = "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*";
            if (Regex.IsMatch(email, expresion))
            {
                if (Regex.Replace(email, expresion, String.Empty).Length == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}