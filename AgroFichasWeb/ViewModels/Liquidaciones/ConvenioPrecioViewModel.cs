using AgroFichasWeb.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml;

namespace AgroFichasWeb.ViewModels.Liquidaciones
{
    public class ConvenioPrecioViewModel
    {
        public int IdConvenioPrecio { get; set; }
        public int IdContrato { get; set; }
        public int IdMoneda { get; set; }
        public int Cantidad { get; set; }
        public bool EsPiso { get; set; }
        public decimal PrecioUnidad { get; set; }
        public bool Habilitado { get; set; }
        public string Comentarios { get; set; }
        public int Prioridad { get; set; }
        public int NextAjusteId { get; set; }

        [JsonIgnore]
        public Moneda Moneda { get; set; }
        [JsonIgnore]
        public Contrato Contrato { get; set; }

        public List<ConvenioPrecioAjusteViewModel> Ajustes { get; set; }
        public List<SelectorSucursalViewModel> Sucursales { get; set; }

        [JsonIgnore]
        public List<SelectorSucursalViewModel> SucursalesHabilitadas { get; set; }
        [JsonIgnore]
        public List<Moneda> Monedas { get; set; }
        [JsonIgnore]
        public List<MotivoAjustePrecio> MotivosAjuste { get; set; }

        public static ConvenioPrecioViewModel CreateEmpty(AgroFichasDBDataContext dc, int? idContrato)
        {
            var model = new ConvenioPrecioViewModel()
            {
                IdContrato = idContrato ?? 0,
                Habilitado = true,
                Sucursales = SelectorSucursalViewModel.ForConvenioPrecio(dc, 0),
                Ajustes = new List<ConvenioPrecioAjusteViewModel>(),
                NextAjusteId = NextId(dc)
            };

            model.LoadLookups(dc);

            return model;
        }

        public static ConvenioPrecioViewModel Create(AgroFichasDBDataContext dc, int idConvenioPrecio)
        {
            var convenio = dc.ConvenioPrecio.Single(cp => cp.IdConvenioPrecio == idConvenioPrecio);

            var model = new ConvenioPrecioViewModel()
            {
                IdConvenioPrecio = convenio.IdConvenioPrecio,
                Cantidad = convenio.Cantidad,
                Comentarios = convenio.Comentarios,
                EsPiso = convenio.EsPiso,
                Habilitado = convenio.Habilitado,
                IdContrato = convenio.IdContrato,
                IdMoneda = convenio.IdMoneda,
                PrecioUnidad = convenio.PrecioUnidad,
                Prioridad = convenio.Prioridad,
                Sucursales = SelectorSucursalViewModel.ForConvenioPrecio(dc, idConvenioPrecio),
                Ajustes = (from aj in convenio.ConvenioPrecioAjuste
                           select new ConvenioPrecioAjusteViewModel()
                           {
                               EsBono = aj.PrecioUnidad >= 0,
                               Cantidad = aj.Cantidad,
                               Comentarios = aj.Comentarios,
                               IdConvenioPrecioAjuste = aj.IdConvenioPrecioAjuste,
                               IdMotivoAjustePrecio = aj.IdMotivoAjustePrecio,
                               NombreMotivoAjustePrecio = aj.MotivoAjustePrecio.Nombre,
                               PrecioUnidad = Math.Abs(aj.PrecioUnidad),
                               Sucursales = SelectorSucursalViewModel.ForConvenioPrecioAjuste(dc, aj.IdConvenioPrecioAjuste)
                           }).ToList(),
                NextAjusteId = NextId(dc)
            };

            model.LoadLookups(dc);

            return model;
        }

        public static int NextId(AgroFichasDBDataContext dc)
        {
            var last = dc.ConvenioPrecioAjuste.Max(ic => (int?)ic.IdConvenioPrecioAjuste);
            return (last ?? 0) + 1000;
        }

        public ConvenioPrecioViewModel()
        {
            this.Ajustes = new List<ConvenioPrecioAjusteViewModel>();
        }

        public void LoadLookups(AgroFichasDBDataContext dc, bool loadTable = true)
        {
            this.Monedas = dc.Moneda.ToList();
            this.Moneda = dc.Moneda.SingleOrDefault(m => m.IdMoneda == this.IdMoneda);
            this.MotivosAjuste = dc.MotivoAjustePrecio.ToList();
            this.SucursalesHabilitadas = (from suc in dc.Sucursal
                                          where suc.Habilitada
                                          orderby suc.Nombre
                                          select new SelectorSucursalViewModel()
                                          {
                                              IdSucursal = suc.IdSucursal,
                                              NombreSucursal = suc.Nombre,
                                              Seleccionado = false
                                          }).ToList();


            if (this.IdContrato > 0 && this.Contrato == null)
                this.Contrato = dc.Contrato.SingleOrDefault(c => c.IdContrato == this.IdContrato);
        }

        private int Min(int value1, int value2)
        {
            if (value1 < value2)
                return value1;
            else
                return value2;
        }

        public void SetDefaults()
        {
            if (this.Comentarios == null)
                this.Comentarios = "";

            foreach (var aj in this.Ajustes)
                aj.SetDefaults();
        }

        public void Validate(ModelStateDictionary modelState)
        {
            if (this.IdContrato <= 0)
                modelState.AddModelError("IdContrato", "El Contrato es requerido");
            if (this.IdMoneda <= 0)
                modelState.AddModelError("IdMoneda", "La Moneda es requerida");
            if (this.Cantidad <= 0)
                modelState.AddModelError("Cantidad", "La Cantidad no es válida");
            if (this.PrecioUnidad <= 0)
                modelState.AddModelError("PrecioUnidad", "El Precio no es válido");

            if (IdMoneda > 0 && this.PrecioUnidad > 0)
            {
                var dc = new AgroFichasDBDataContext();
                var moneda = dc.Moneda.Single(m => m.IdMoneda == this.IdMoneda);
                if (this.PrecioUnidad != Math.Round(this.PrecioUnidad, moneda.DecimalesPrecio))
                    modelState.AddModelError("PrecioUnidad", String.Format("El precio debe tener a lo más {0} decimal(es)", moneda.DecimalesPrecio));
            }

            if (this.Sucursales.Where(s => s.Seleccionado).Count() == 0)
                modelState.AddModelError("Sucursales", "Seleccione al menos una sucursal.");

            if (this.Ajustes != null)
            {
                foreach (var aj in this.Ajustes)
                {
                    if (aj.Cantidad <= 0)
                        modelState.AddModelError("", "La cantidad del ajuste no es válida");
                    if (aj.Sucursales == null || aj.Sucursales.Where(s => s.Seleccionado).Count() == 0)
                        modelState.AddModelError("", "Seleccione al menos una sucursal para el ajuste.");
                }
            }

            if (!ConvenioPrecio.EsTablaValida(this.Cantidad, CalcularNuevaTablaPrecio(), out List<string> msgs))
            {
                foreach (var error in msgs)
                {
                    modelState.AddModelError("", error);
                }
            }
        }

        public List<ConvenioPrecioItemTabla> CalcularNuevaTablaPrecio()
        {
            var sucursalesBase = this.Sucursales.Where(s => s.Seleccionado).Select(s => s.IdSucursal).ToList();
            var ajustesValidacion = new List<ConvenioPrecioAjuste>();
            foreach (var aj in this.Ajustes)
            {
                var ajusteValidacion = new ConvenioPrecioAjuste()
                {
                    Cantidad = aj.Cantidad,
                    PrecioUnidad = aj.PrecioUnidad * (aj.EsBono ? 1 : -1),
                    ConvenioPrecioAjusteSucursal = new System.Data.Linq.EntitySet<ConvenioPrecioAjusteSucursal>()
                };

                ajusteValidacion.ConvenioPrecioAjusteSucursal.AddRange(from suc in aj.Sucursales
                                                                       where suc.Seleccionado
                                                                       select new ConvenioPrecioAjusteSucursal()
                                                                       {
                                                                           IdSucursal = suc.IdSucursal
                                                                       });
                ajustesValidacion.Add(ajusteValidacion);
            }
            return ConvenioPrecio.CalcularTabla(this.Cantidad, this.PrecioUnidad, sucursalesBase, ajustesValidacion);
        }

        public List<ConvenioPrecioItemTabla> GetTablaPreciosExistente()
        {
            var dc = new AgroFichasDBDataContext();
            var convenio = dc.ConvenioPrecio.SingleOrDefault(c => c.IdConvenioPrecio == this.IdConvenioPrecio);
            if (convenio != null)
                return convenio.ConvenioPrecioItemTabla.ToList();

            return null;
        }

        public bool ValidateAutorizacion(ControllerContext ctx, string userName, string ipAddress, out int idConvenioPrecioAutorizacion, out string msg)
        {
            idConvenioPrecioAutorizacion = 0;
            msg = "";

            var dc = new AgroFichasDBDataContext();

            //Usamos la tabla de precios para verificar autorización. Todos los items de la tabla de precios deben ser autorizados
            var newTabla = CalcularNuevaTablaPrecio();
            var oldTabla = GetTablaPreciosExistente();
            var itemsPorVerificar = new List<ConvenioPrecioItemTabla>();

            //1. Solo es nececario validar items nuevos. Si ya existían en el convenio suponeos que estaban autorizados.
            if (oldTabla == null || oldTabla.Count == 0)
            {
                itemsPorVerificar.AddRange(newTabla);
            }
            else
            {
                foreach (var newItem in newTabla)
                    if (oldTabla.SingleOrDefault(ot => ot.IdSucursal == newItem.IdSucursal && ot.Cantidad == newItem.Cantidad && ot.PrecioUnidad == newItem.PrecioUnidad) == null)
                        itemsPorVerificar.Add(newItem);
            }

            if (itemsPorVerificar.Count == 0)
                return true;

            //2. Para los items nuevos verificamos si es neceario autorizar alguno.
            var puedeGerencia = SYS_User.Current().HasPermiso(1023);
            var puedeAbastecimiento = SYS_User.Current().HasPermiso(1024);
            var cultivo = Contrato.GetCultivo();

            var rangosAbastecimiento = dc.RangoPrecio.Where(r => r.IdNivelRangoPrecio == 2 && r.IdMoneda == this.IdMoneda && r.IdCultivo == cultivo.IdCultivo);
            var rangosGerencia = dc.RangoPrecio.Where(r => r.IdNivelRangoPrecio == 1 && r.IdMoneda == this.IdMoneda && r.IdCultivo == cultivo.IdCultivo);

            var requiereAutorizacionAba = false;
            var requiereAutorizacionGer = false;

            foreach (var newItem in itemsPorVerificar)
            {
                //El rango de abastecimiento es el que seteó abastecimiento, es decir que cualquier usuario puede ingresar precios que estén en este rango
                var rangoAbastecimiento = rangosAbastecimiento.SingleOrDefault(r => r.IdSucursal == newItem.IdSucursal);

                //El rango de gerencia es el que seteó gerencia, es decir que abastecimiento tiente dominio sobre este rango
                var rangoGerencia = rangosGerencia.SingleOrDefault(r => r.IdSucursal == newItem.IdSucursal);

                decimal? minAba = null;
                decimal? maxAba = null;
                decimal? minGer = null;
                decimal? maxGer = null;

                if (rangoGerencia != null)
                {
                    minGer = rangoGerencia.PrecioMin;
                    maxGer = rangoGerencia.PrecioMax;
                }

                if (rangoAbastecimiento != null)
                {
                    minAba = rangoAbastecimiento.PrecioMin;
                    maxAba = rangoAbastecimiento.PrecioMax;
                }

                if (!minAba.HasValue && minGer.HasValue)
                    minAba = minGer;

                if (!maxAba.HasValue && maxGer.HasValue)
                    maxAba = maxGer;

                //Si está dentro del rango de abastecimiento podemos autorizar automáticamente
                if (EstaEnRango(minAba, maxAba, newItem.PrecioUnidad))
                {
                    //item.IdNivelAutorizacion = 0;
                    //item.NombreNivelAutorizacion = "(Auto)";
                    newItem.Autorizado = true;
                    newItem.Autorizador = SYS_User.Current().UserName + " (Auto)";
                }
                else if (EstaEnRango(minGer, maxGer, newItem.PrecioUnidad))
                {
                    //item.IdNivelAutorizacion = 2;
                    //item.NombreNivelAutorizacion = "Abastecimiento";

                    //Si está en el rango de gerencia, puede autorizar el usuario con permiso 1024
                    if (puedeAbastecimiento)
                    {
                        newItem.Autorizado = true;
                        newItem.Autorizador = SYS_User.Current().UserName;
                    }
                    else
                    {
                        newItem.Autorizado = null;
                        newItem.Autorizador = "(Abastecimiento)";

                        requiereAutorizacionAba = true;
                    }
                }
                else
                {
                    //Está fuera del rango autorizado por gerencia. Gerencia debe autorizar
                    //item.IdNivelAutorizacion = 1;
                    //item.NombreNivelAutorizacion = "Gerencia";
                    if (puedeGerencia)
                    {
                        newItem.Autorizado = true;
                        newItem.Autorizador = SYS_User.Current().UserName;
                    }
                    else
                    {
                        newItem.Autorizado = null;
                        newItem.Autorizador = "(Gerencia)";

                        requiereAutorizacionGer = true;
                    }
                }
            }

            //3. Si es necesatio autorizar se crea la solicitud de autorización. Si no, se guardan los cambios al convenio.
            if (requiereAutorizacionGer || requiereAutorizacionAba)
            {
                var autorizacion = new ConvenioPrecioAutorizacion()
                {
                    Autorizada = null,
                    Data = JsonConvert.SerializeObject(this),
                    TablaPrecios = JsonConvert.SerializeObject(ConvenioPrecio.ToListOfItemTablaPrecio(newTabla)),
                    FechaHoraIns = DateTime.Now,
                    IdContrato = this.IdContrato,
                    IdConvenioPrecio = this.IdConvenioPrecio != 0 ? this.IdConvenioPrecio : (int?)null,
                    IdNivelAutorizacion = requiereAutorizacionGer ? 1 : 2,
                    IpIns = ipAddress,
                    UserIns = userName
                };

                dc.ConvenioPrecioAutorizacion.InsertOnSubmit(autorizacion);
                dc.SubmitChanges();

                idConvenioPrecioAutorizacion = autorizacion.IdConvenioPrecioAutorizacion;

                //Enviamos la notificación a posibles autorizadores               
                autorizacion.NotificarEnRevision(ctx, out msg);

                return false;
            }
            else
            {
                return true;
            }
        }

        private bool EstaEnRango(decimal? minValue, decimal? maxValue, decimal value)
        {
            if (minValue.HasValue && value < minValue.Value)
                return false;

            if (maxValue.HasValue && value > maxValue.Value)
                return false;

            return true;
        }

        public ConvenioPrecio Persist(ControllerContext ctx, AgroFichasDBDataContext dc, string userName, string ipAddress, out bool requiereNotificacion)
        {
            requiereNotificacion = false;

            ConvenioPrecio convenio;
            if (this.IdConvenioPrecio == 0)
            {
                requiereNotificacion = true;

                convenio = new ConvenioPrecio()
                {
                    Prioridad = ConvenioPrecio.NextPrioridad(dc, this.IdContrato),
                    UserIns = userName,
                    FechaHoraIns = DateTime.Now,
                    IpIns = ipAddress,
                };

                dc.ConvenioPrecio.InsertOnSubmit(convenio);
            }
            else
            {
                convenio = dc.ConvenioPrecio.Single(c => c.IdConvenioPrecio == this.IdConvenioPrecio);
                convenio.UserUpd = userName;
                convenio.FechaHoraUpd = DateTime.Now;
                convenio.IpUpd = ipAddress;
            }

            if (convenio.Cantidad != this.Cantidad || convenio.PrecioUnidad != this.PrecioUnidad || convenio.IdMoneda != this.IdMoneda)
                requiereNotificacion = true;

            convenio.Cantidad = this.Cantidad;
            convenio.Comentarios = this.Comentarios;
            convenio.IdContrato = this.IdContrato;
            convenio.EsPiso = this.EsPiso;
            convenio.Habilitado = this.Habilitado;
            convenio.IdMoneda = this.IdMoneda;
            convenio.PrecioUnidad = this.PrecioUnidad;

            //Sucursales
            var oldSucursales = convenio.ConvenioPrecioSucursal.ToList();
            var newSucursales = this.Sucursales.Where(s => s.Seleccionado);

            //Para cada sucursal que tenía el convenio revisamos si sigue en la lista. Si no está en la lista nueva lo eliminamos
            foreach (var suc in oldSucursales)
            {
                if (newSucursales.SingleOrDefault(s => s.IdSucursal == suc.IdSucursal) == null)
                {
                    requiereNotificacion = true;
                    dc.ConvenioPrecioSucursal.DeleteOnSubmit(suc);
                }
            }

            //Para cada sucursal de la nueva lista, revisamos si ya lo tenía. Si no lo tenía lo agregamos
            foreach (var suc in newSucursales)
            {
                if (oldSucursales.SingleOrDefault(cu => cu.IdSucursal == suc.IdSucursal) == null)
                {
                    requiereNotificacion = true;
                    convenio.ConvenioPrecioSucursal.Add(new ConvenioPrecioSucursal()
                    {
                        IdSucursal = suc.IdSucursal,
                        FechaHoraIns = DateTime.Now,
                        UserIns = userName,
                        IpIns = ipAddress
                    });
                }
            }

            //Ajustes de precio
            var oldAjustes = convenio.ConvenioPrecioAjuste.ToList();
            var newAjustes = this.Ajustes;

            foreach (var aj in oldAjustes)
            {
                if (newAjustes.SingleOrDefault(a => a.IdConvenioPrecioAjuste == aj.IdConvenioPrecioAjuste) == null)
                {
                    requiereNotificacion = true;
                    dc.ConvenioPrecioAjuste.DeleteOnSubmit(aj);
                }
            }

            foreach (var aj in newAjustes)
            {
                var ajuste = convenio.ConvenioPrecioAjuste.SingleOrDefault(cpa => cpa.IdConvenioPrecioAjuste == aj.IdConvenioPrecioAjuste);
                if (ajuste == null)
                {
                    requiereNotificacion = true;
                    ajuste = new ConvenioPrecioAjuste()
                    {
                        UserIns = userName,
                        FechaHoraIns = DateTime.Now,
                        IpIns = ipAddress
                    };

                    convenio.ConvenioPrecioAjuste.Add(ajuste);
                }
                else
                {
                    ajuste.UserUpd = userName;
                    ajuste.FechaHoraUpd = DateTime.Now;
                    ajuste.IpUpd = ipAddress;
                }

                var nuevoPrecio = aj.PrecioUnidad * (aj.EsBono ? 1 : -1);

                if (ajuste.Cantidad != aj.Cantidad || ajuste.PrecioUnidad != nuevoPrecio)
                    requiereNotificacion = true;

                ajuste.Cantidad = aj.Cantidad;
                ajuste.Comentarios = aj.Comentarios;
                ajuste.MotivoAjustePrecio = dc.MotivoAjustePrecio.Single(ma => ma.IdMotivoAjustePrecio == aj.IdMotivoAjustePrecio);
                ajuste.PrecioUnidad = nuevoPrecio;

                //Sucursales
                var oldSucursalesAjuste = ajuste.ConvenioPrecioAjusteSucursal.ToList();
                var newSucursalesAjuste = aj.Sucursales.Where(s => s.Seleccionado);

                //Para cada sucursal que tenía el convenio revisamos si sigue en la lista. Si no está en la lista nueva lo eliminamos
                foreach (var suc in oldSucursalesAjuste)
                {
                    if (newSucursalesAjuste.SingleOrDefault(s => s.IdSucursal == suc.IdSucursal) == null)
                    {
                        requiereNotificacion = true;
                        dc.ConvenioPrecioAjusteSucursal.DeleteOnSubmit(suc);
                    }
                }

                //Para cada sucursal de la nueva lista, revisamos si ya lo tenía. Si no lo tenía lo agregamos
                foreach (var suc in newSucursalesAjuste)
                {
                    if (oldSucursalesAjuste.SingleOrDefault(cu => cu.IdSucursal == suc.IdSucursal) == null)
                    {
                        requiereNotificacion = true;
                        ajuste.ConvenioPrecioAjusteSucursal.Add(new ConvenioPrecioAjusteSucursal()
                        {
                            IdSucursal = suc.IdSucursal,
                            FechaHoraIns = DateTime.Now,
                            UserIns = userName,
                            IpIns = ipAddress
                        });
                    }
                }

            }
            
            //Tabla de Precios
            var newTabla = CalcularNuevaTablaPrecio();

            //Eliminados
            foreach (var oldItem in convenio.ConvenioPrecioItemTabla)
            {
                if (newTabla.SingleOrDefault(s => s.IdSucursal == oldItem.IdSucursal && s.PrecioUnidad == oldItem.PrecioUnidad) == null)
                {
                    dc.ConvenioPrecioItemTabla.DeleteOnSubmit(oldItem);
                }
            }

            //Nuevos y cambiados
            foreach (var newItem in newTabla)
            {
                var item = convenio.ConvenioPrecioItemTabla.SingleOrDefault(cu => cu.IdSucursal == newItem.IdSucursal && cu.PrecioUnidad == newItem.PrecioUnidad);
                if (item == null)
                {
                    item = new ConvenioPrecioItemTabla()
                    {
                         Autorizado = null,
                         Autorizador = null,
                         IdSucursal = newItem.IdSucursal,
                         PrecioUnidad = newItem.PrecioUnidad,
                    };

                    convenio.ConvenioPrecioItemTabla.Add(item);
                }
                item.Cantidad = newItem.Cantidad;
            }

            dc.SubmitChanges();

            return convenio;
        }
    }

    public class ConvenioPrecioAjusteViewModel
    {
        public int IdConvenioPrecioAjuste { get; set; }
        public int IdMotivoAjustePrecio { get; set; }
        public string NombreMotivoAjustePrecio { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnidad { get; set; }
        public string Comentarios { get; set; }
        public bool EsBono { get; set; }

        public List<SelectorSucursalViewModel> Sucursales { get; set; }

        public string ListaSucursales()
        {
            var sb = new StringBuilder();
            foreach (var s in this.Sucursales.Where(suc => suc.Seleccionado))
            {
                if (sb.Length > 0)
                    sb.Append(", ");

                sb.Append(s.NombreSucursal);
            }

            return sb.ToString();
        }

        public void SetDefaults()
        {
            if (this.Comentarios == null)
                this.Comentarios = "";
        }
    }
}