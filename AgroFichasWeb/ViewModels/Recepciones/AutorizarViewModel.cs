using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.ViewModels.Recepciones
{
    public class AutorizarViewModel
    {
        public ProcesoIngreso ProcesoIngreso { get; set; }
        public List<ResumenCtaCteViewModel> Resumenes { get; set; }

        public bool Autorizado { get; set; }

        public int IdProcesoIngreso { get; set; }

        public AutorizarViewModel()
        {

        }

        public AutorizarViewModel(AgroFichasDBDataContext dc, int idProcesoIngreso)
        {
            this.IdProcesoIngreso = idProcesoIngreso;

            LoadLookups(dc);
        }

        public void LoadLookups(AgroFichasDBDataContext dc)
        {
            this.ProcesoIngreso = dc.ProcesoIngreso.Single(pi => pi.IdProcesoIngreso == this.IdProcesoIngreso && pi.IdEstado == 5);

            Resumenes = new List<ResumenCtaCteViewModel>();

            foreach (var relacionado in this.ProcesoIngreso.Agricultor.RelacionadosFull(dc, true))
            {
                var agricultor = dc.Agricultor.Single(ag => ag.IdAgricultor == relacionado.IdAgricultor);
                
                var resumen = new ResumenCtaCteViewModel();

                resumen.Agricultor = agricultor;
                resumen.IngresosValorizados = agricultor.IngresosValorizados(dc, this.ProcesoIngreso.IdTemporada);
                resumen.Descuentos = agricultor.Descuento.Where(d => d.IdTemporada == this.ProcesoIngreso.IdTemporada).ToList();
                resumen.SaldosCtaCte = agricultor.SaldoCtaCte.ToList();

                Resumenes.Add(resumen);
            }
        }

        public void Validate(ModelStateDictionary modelState)
        {
            
        }

        public ProcesoIngreso Persist(AgroFichasDBDataContext dc, string userName, string ipAddress)
        {
            this.ProcesoIngreso.Autorizado = (this.Autorizado ? ProcesoIngreso.ANALISIS_AUTORIZADO : ProcesoIngreso.ANALISIS_RECHAZADO);
            this.ProcesoIngreso.AutorizadoAuto = false;

            this.ProcesoIngreso.FechaHoraAutoriza = DateTime.Now;
            this.ProcesoIngreso.UserAutoriza = userName;
            this.ProcesoIngreso.IpAutoriza = ipAddress;
            this.ProcesoIngreso.UserUpd = userName;
            this.ProcesoIngreso.FechaHoraUpd = DateTime.Now;
            this.ProcesoIngreso.IpUpd = ipAddress;

            dc.SubmitChanges();

            return this.ProcesoIngreso;
        }


        public static void DecodeToken(AgroFichasDBDataContext dc, string h, out int userID, out int idProcesoIngreso)
        {
            var enc = new Encryptor();
            var token = enc.DecryptString(h);
            var pairs = HttpUtility.ParseQueryString(token);

            userID = int.Parse(pairs["userID"]);
            idProcesoIngreso = int.Parse(pairs["idProcesoIngreso"]);

            var parts = pairs["ts"].Split('-');
            var ts = new DateTime(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), 0);

            int userIDb = userID;
            var user = dc.SYS_User.SingleOrDefault(u => u.UserID == userIDb);
            if (user == null)
                throw new Exception("No se encuentra al usuario id: " + userID.ToString());

            if (!user.HasPermiso(30))
                throw new Exception("Usuario no autorizado a realizar esta operación. id: " + userID.ToString());

            int idProcesoIngresob = idProcesoIngreso;
            var ingreso = dc.ProcesoIngreso.SingleOrDefault(pi => pi.IdProcesoIngreso == idProcesoIngresob);
            if (ingreso == null)
                throw new Exception("No se encuentra el ingreso id: " + idProcesoIngreso.ToString());
            if (ingreso.IdEstado != 5)
                throw new Exception("No es posible autorizar el ingreso porque su estado es '" + ingreso.EstadoProcesoIngreso.Nombre + "'. id: " + idProcesoIngreso.ToString());
            //Check Link is still valid
            if (ts.AddHours(24) < DateTime.Now)
            {
                throw new Exception("Este link ha expirado. Por seguridad sólo estuvo vigente durante 24 horas. Para autorizar este ingreso vaya a Autorizar Ingreso en el sistema.");
            }

        }
    }

    public class ResumenCtaCteViewModel
    {
        public Agricultor Agricultor { get; set; }
        public List<Agricultor.IngresoValorizado> IngresosValorizados { get; set; }
        public List<Descuento> Descuentos { get; set; }
        public List<SaldoCtaCte> SaldosCtaCte { get; set; }
    }
}