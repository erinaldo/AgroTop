using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CTR_PlanificacionSemanal
    {
        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        public string Chofer { get; set; }
        public string RutChofer { get; set; }
        public string EmailChofer { get; set; }
        public string TipoCamion { get; set; }
        public bool EditarLC { get; set; }
        public DateTime? fDia { get; set; }
        public int? IdEmpresaSelect { get; set; }
        public string Patente { get; set; }
        public string RUT { get; set; }
        public string Telefono { get; set; }
        public string Transportista { get; set; }


        public PlanificacionPdf CrearPdfPlanificacion()
        {
            if (!fDia.HasValue)
            {
                return CrearPdfPlanificacion(Año, Semana, IdEmpresa);
            }
            else
            {
                return CrearPdfPlanificacion(Año, Semana, IdEmpresa, fDia);
            }
        }

        public PlanificacionPdf CrearPdfPlanificacion(int año, int nroSemana, int? idEmpresaSelect)
        {
            #region Template

            List<CTR_PlanificacionSemanal> planificaciones = dc.CTR_PlanificacionSemanal.Where(X => X.FechaLunes.Year == año && X.Semana == nroSemana && X.Habilitado == true && ((idEmpresaSelect ?? 0) == 0 || X.IdEmpresa == idEmpresaSelect)).ToList();
            string template = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/planificacion_semanal/planificacion_semanal_template.html"), System.Text.Encoding.UTF8);
            string guid = Guid.NewGuid().ToString();

            CTR_PlanificacionSemanal planificacionSemanal = planificaciones.FirstOrDefault();
            string fLunes = "", fMartes = "", fMiercoles = "", fJueves = "", fViernes = "", fSabado = "", fDomingo = "";
            if (planificacionSemanal != null)
            {
                fLunes = string.Format("{0:dd/MM}", planificacionSemanal.FechaLunes);
                fMartes = string.Format("{0:dd/MM}", planificacionSemanal.FechaMartes);
                fMiercoles = string.Format("{0:dd/MM}", planificacionSemanal.FechaMiercoles);
                fJueves = string.Format("{0:dd/MM}", planificacionSemanal.FechaJueves);
                fViernes = string.Format("{0:dd/MM}", planificacionSemanal.FechaViernes);
                fSabado = string.Format("{0:dd/MM}", planificacionSemanal.FechaSabado);
                fDomingo = string.Format("{0:dd/MM}", planificacionSemanal.FechaDomingo);
            }

            string s = "";
            foreach (CTR_PlanificacionSemanal planificacion in planificaciones)
            {
                s += "<tr valign=\"top\">";
                s += "    <td>" + planificacion.Semana + "</td>";
                s += "    <td>" + planificacion.PlantaProduccion.Nombre + "</td>";
                s += "    <td>" + planificacion.Empresa.Nombre + "</td>";
                s += "    <td>" + planificacion.CTR_Producto.Nombre + "</td>";
                s += "    <td>" + (planificacion.CTR_Envase == null ? "" : planificacion.CTR_Envase.Descripcion) + "</td>";
                s += "    <td>" + (planificacion.Cliente == null ? "" : planificacion.Cliente.RazonSocial) + "</td>";
                s += "    <td>" + (planificacion.Cliente.IDAvenatop == null ? "" : planificacion.Cliente.IDAvenatop) + (planificacion.Cliente.IDOleotop == null ? "" : planificacion.Cliente.IDOleotop) + (planificacion.Cliente.IDGranotop == null ? "" : planificacion.Cliente.IDGranotop) + "</td>";
                s += "    <td>" + (planificacion.IdTransportista == null ? "ND" : planificacion.LOG_Transportista.Nombre) + "</td>";
                s += "    <td>" + planificacion.Destino + "</td>";
                s += "    <td>" + planificacion.Pais.PaisNombre + "</td>";
                s += "    <td>" + planificacion.OC.ToTrimmedString() + "</td>";
                s += "    <td>" + planificacion.LC.ToString("C0") + "</td>";
                s += "    <td>" + planificacion.Lote + "</td>";
                s += "    <td>" + planificacion.DUS + "</td>";
                s += "    <td>" + planificacion.Reserva + "</td>";
                s += "    <td class=\"" + planificacion.GetCssStyle(planificacion.Lunes) + "\">" + planificacion.Lunes + " " + planificacion.GetPresentados(planificacion.FechaLunes) + "</td>";
                s += "    <td class=\"" + planificacion.GetCssStyle(planificacion.Martes) + "\">" + planificacion.Martes + " " + planificacion.GetPresentados(planificacion.FechaMartes) + "</td>";
                s += "    <td class=\"" + planificacion.GetCssStyle(planificacion.Miercoles) + "\">" + planificacion.Miercoles + " " + planificacion.GetPresentados(planificacion.FechaMiercoles) + "</td>";
                s += "    <td class=\"" + planificacion.GetCssStyle(planificacion.Jueves) + "\">" + planificacion.Jueves + " " + planificacion.GetPresentados(planificacion.FechaJueves) + "</td>";
                s += "    <td class=\"" + planificacion.GetCssStyle(planificacion.Viernes) + "\">" + planificacion.Viernes + " " + planificacion.GetPresentados(planificacion.FechaViernes) + "</td>";
                s += "    <td class=\"" + planificacion.GetCssStyle(planificacion.Sabado) + "\">" + planificacion.Sabado + " " + planificacion.GetPresentados(planificacion.FechaSabado) + "</td>";
                s += "    <td class=\"" + planificacion.GetCssStyle(planificacion.Domingo) + "\">" + planificacion.Domingo + " " + planificacion.GetPresentados(planificacion.FechaDomingo) + "</td>";
                s += "    <td class=\"" + planificacion.GetCssStyle(planificacion.GetSubtotal()) + "\">" + planificacion.GetSubtotal() + "</td>";
                s += "</tr>";
            }

            Util.RepTemp(ref template, "SEMANA", nroSemana.ToString());
            Util.RepTemp(ref template, "AÑO", año.ToString());
            Util.RepTemp(ref template, "DATETIME", string.Format("{0:dd/MM/yyyy hh:mm:ss}", DateTime.Now));
            Util.RepTemp(ref template, "FECHALUNES", fLunes);
            Util.RepTemp(ref template, "FECHAMARTES", fMartes);
            Util.RepTemp(ref template, "FECHAMIERCOLES", fMiercoles);
            Util.RepTemp(ref template, "FECHAJUEVES", fJueves);
            Util.RepTemp(ref template, "FECHAVIERNES", fViernes);
            Util.RepTemp(ref template, "FECHASABADO", fSabado);
            Util.RepTemp(ref template, "FECHADOMINGO", fDomingo);
            Util.RepTemp(ref template, "PLANIFICACIONES", s);
            Util.RepTemp(ref template, "CLASSTOTALLUNES", planificacionSemanal.GetCssStyle(planificaciones.Sum(X => X.Lunes)));
            Util.RepTemp(ref template, "CLASSTOTALMARTES", planificacionSemanal.GetCssStyle(planificaciones.Sum(X => X.Martes)));
            Util.RepTemp(ref template, "CLASSTOTALMIERCOLES", planificacionSemanal.GetCssStyle(planificaciones.Sum(X => X.Miercoles)));
            Util.RepTemp(ref template, "CLASSTOTALJUEVES", planificacionSemanal.GetCssStyle(planificaciones.Sum(X => X.Jueves)));
            Util.RepTemp(ref template, "CLASSTOTALVIERNES", planificacionSemanal.GetCssStyle(planificaciones.Sum(X => X.Viernes)));
            Util.RepTemp(ref template, "CLASSTOTALSABADO", planificacionSemanal.GetCssStyle(planificaciones.Sum(X => X.Sabado)));
            Util.RepTemp(ref template, "CLASSTOTALDOMINGO", planificacionSemanal.GetCssStyle(planificaciones.Sum(X => X.Domingo)));
            Util.RepTemp(ref template, "CLASSTOTALSEMANAL", @planificacionSemanal.GetCssStyle(planificacionSemanal.GetTotal(planificaciones)));
            Util.RepTemp(ref template, "TOTALLUNES", planificaciones.Sum(X => X.Lunes).ToString());
            Util.RepTemp(ref template, "TOTALMARTES", planificaciones.Sum(X => X.Martes).ToString());
            Util.RepTemp(ref template, "TOTALMIERCOLES", planificaciones.Sum(X => X.Miercoles).ToString());
            Util.RepTemp(ref template, "TOTALJUEVES", planificaciones.Sum(X => X.Jueves).ToString());
            Util.RepTemp(ref template, "TOTALVIERNES", planificaciones.Sum(X => X.Sabado).ToString());
            Util.RepTemp(ref template, "TOTALSABADO", planificaciones.Sum(X => X.Domingo).ToString());
            Util.RepTemp(ref template, "TOTALDOMINGO", planificaciones.Sum(X => X.Lunes).ToString());
            Util.RepTemp(ref template, "TOTALSEMANAL", planificacionSemanal.GetTotal(planificaciones).ToString());

            #endregion

            try
            {
                string path = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/pdfs/controltiempo/planificaciones"), string.Format("{0}.pdf", string.Format("{0}", guid)));

                FileStream fileStream = new FileStream(Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/pdfs/controltiempo/planificaciones"), string.Format("{0}_debug.html", guid)),
                                                       FileMode.OpenOrCreate,
                                                       FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine(template);
                writer.Close();

                NReco.PdfGenerator.HtmlToPdfConverter pdfGenerator = new NReco.PdfGenerator.HtmlToPdfConverter();
                pdfGenerator.Orientation = NReco.PdfGenerator.PageOrientation.Landscape;
                pdfGenerator.Margins.Top = 5;
                pdfGenerator.Margins.Left = 5;
                pdfGenerator.Margins.Bottom = 5;
                pdfGenerator.Margins.Right = 5;
                var pdfBytes = pdfGenerator.GeneratePdf(template);

                System.IO.File.WriteAllBytes(path, pdfBytes);
            }
            catch (Exception ex)
            {
                FileStream fileStream = new FileStream(Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/pdfs/controltiempo/planificaciones"), string.Format("{0}.txt", guid)),
                                                       FileMode.OpenOrCreate,
                                                       FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine(ex.ToString());
                writer.Close();
            }

            return new PlanificacionPdf()
            {
                Guid = guid,
                Titulo = string.Format("Planificación Semanal-{0}-{1}.pdf", año, nroSemana)
            };
        }

        public PlanificacionPdf CrearPdfPlanificacion(int año, int nroSemana, int? idEmpresaSelect, DateTime? fDia)
        {
            #region Template

            List<CTR_PlanificacionSemanal> planificaciones = dc.CTR_PlanificacionSemanal.Where(X => X.FechaLunes.Year == Año && X.Semana == Semana && X.Habilitado == true && (IdEmpresa == 0 || X.IdEmpresa == IdEmpresa)).ToList();
            string template = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/planificacion_semanal/planificacion_diaria_template.html"), System.Text.Encoding.UTF8);
            string guid = Guid.NewGuid().ToString();

            CTR_PlanificacionSemanal planificacionSemanal = planificaciones.FirstOrDefault();
            string fLunes = "", fMartes = "", fMiercoles = "", fJueves = "", fViernes = "", fSabado = "", fDomingo = "";
            if (planificacionSemanal != null)
            {
                fLunes = string.Format("{0:dd/MM}", planificacionSemanal.FechaLunes);
                fMartes = string.Format("{0:dd/MM}", planificacionSemanal.FechaMartes);
                fMiercoles = string.Format("{0:dd/MM}", planificacionSemanal.FechaMiercoles);
                fJueves = string.Format("{0:dd/MM}", planificacionSemanal.FechaJueves);
                fViernes = string.Format("{0:dd/MM}", planificacionSemanal.FechaViernes);
                fSabado = string.Format("{0:dd/MM}", planificacionSemanal.FechaSabado);
                fDomingo = string.Format("{0:dd/MM}", planificacionSemanal.FechaDomingo);
            }

            string s = "";
            foreach (CTR_PlanificacionSemanal planificacion in planificaciones)
            {
                s += "<tr valign=\"top\">";
                s += "    <td>" + planificacion.Semana + "</td>";
                s += "    <td>" + planificacion.PlantaProduccion.Nombre + "</td>";
                s += "    <td>" + planificacion.Empresa.Nombre + "</td>";
                s += "    <td>" + planificacion.CTR_Producto.Nombre + "</td>";
                s += "    <td>" + (planificacion.CTR_Envase == null ? "" : planificacion.CTR_Envase.Descripcion) + "</td>";
                s += "    <td>" + (planificacion.Cliente == null ? "" : planificacion.Cliente.RazonSocial) + "</td>";
                s += "    <td>" + (planificacion.Cliente.IDAvenatop == null ? "" : planificacion.Cliente.IDAvenatop) + (planificacion.Cliente.IDOleotop == null ? "" : planificacion.Cliente.IDOleotop) + "</td>";
                s += "    <td>" + (planificacion.IdTransportista == null ? "ND" : planificacion.LOG_Transportista.Nombre) + "</td>";
                s += "    <td>" + planificacion.Destino + "</td>";
                s += "    <td>" + planificacion.Pais.PaisNombre + "</td>";
                s += "    <td>" + planificacion.OC.ToTrimmedString() + "</td>";
                s += "    <td>" + planificacion.LC.ToString("C0") + "</td>";
                s += "    <td>" + planificacion.Lote + "</td>";
                s += "    <td>" + planificacion.DUS + "</td>";
                s += "    <td>" + planificacion.Reserva + "</td>";
                if (fDia == planificacion.FechaLunes)
                    s += "    <td class=\"" + planificacion.GetCssStyle(planificacion.Lunes) + "\">" + planificacion.Lunes + " " + planificacion.GetPresentados(planificacion.FechaLunes) + "</td>";
                if (fDia == planificacion.FechaMartes)
                    s += "    <td class=\"" + planificacion.GetCssStyle(planificacion.Martes) + "\">" + planificacion.Martes + " " + planificacion.GetPresentados(planificacion.FechaMartes) + "</td>";
                if (fDia == planificacion.FechaMiercoles)
                    s += "    <td class=\"" + planificacion.GetCssStyle(planificacion.Miercoles) + "\">" + planificacion.Miercoles + " " + planificacion.GetPresentados(planificacion.FechaMiercoles) + "</td>";
                if (fDia == planificacion.FechaJueves)
                    s += "    <td class=\"" + planificacion.GetCssStyle(planificacion.Jueves) + "\">" + planificacion.Jueves + " " + planificacion.GetPresentados(planificacion.FechaJueves) + "</td>";
                if (fDia == planificacion.FechaViernes)
                    s += "    <td class=\"" + planificacion.GetCssStyle(planificacion.Viernes) + "\">" + planificacion.Viernes + " " + planificacion.GetPresentados(planificacion.FechaViernes) + "</td>";
                if (fDia == planificacion.FechaSabado)
                    s += "    <td class=\"" + planificacion.GetCssStyle(planificacion.Sabado) + "\">" + planificacion.Sabado + " " + planificacion.GetPresentados(planificacion.FechaSabado) + "</td>";
                if (fDia == planificacion.FechaDomingo)
                    s += "    <td class=\"" + planificacion.GetCssStyle(planificacion.Domingo) + "\">" + planificacion.Domingo + " " + planificacion.GetPresentados(planificacion.FechaDomingo) + "</td>";
                s += "</tr>";
            }

            Util.RepTemp(ref template, "PLANIFICACIONES", s);
            Util.RepTemp(ref template, "SEMANA", nroSemana.ToString());
            Util.RepTemp(ref template, "DIA", TraduceDiaLargo(fDia.Value.DayOfWeek));
            Util.RepTemp(ref template, "AÑO", año.ToString());
            Util.RepTemp(ref template, "DATETIME", string.Format("{0:dd/MM/yyyy hh:mm:ss}", DateTime.Now));
            if (fDia == planificacionSemanal.FechaLunes)
            {
                Util.RepTemp(ref template, "FECHADIA", fLunes);
                Util.RepTemp(ref template, "CLASSTOTALDIA", planificacionSemanal.GetCssStyle(planificaciones.Sum(X => X.Lunes)));
                Util.RepTemp(ref template, "TOTALDIA", planificaciones.Sum(X => X.Lunes).ToString());
            }
            if (fDia == planificacionSemanal.FechaMartes)
            {
                Util.RepTemp(ref template, "FECHADIA", fMartes);
                Util.RepTemp(ref template, "CLASSTOTALDIA", planificacionSemanal.GetCssStyle(planificaciones.Sum(X => X.Martes)));
                Util.RepTemp(ref template, "TOTALDIA", planificaciones.Sum(X => X.Martes).ToString());
            }
            if (fDia == planificacionSemanal.FechaMiercoles)
            {
                Util.RepTemp(ref template, "FECHADIA", fMiercoles);
                Util.RepTemp(ref template, "CLASSTOTALDIA", planificacionSemanal.GetCssStyle(planificaciones.Sum(X => X.Miercoles)));
                Util.RepTemp(ref template, "TOTALDIA", planificaciones.Sum(X => X.Miercoles).ToString());
            }
            if (fDia == planificacionSemanal.FechaJueves)
            {
                Util.RepTemp(ref template, "FECHADIA", fJueves);
                Util.RepTemp(ref template, "CLASSTOTALDIA", planificacionSemanal.GetCssStyle(planificaciones.Sum(X => X.Jueves)));
                Util.RepTemp(ref template, "TOTALDIA", planificaciones.Sum(X => X.Jueves).ToString());
            }
            if (fDia == planificacionSemanal.FechaViernes)
            {
                Util.RepTemp(ref template, "FECHADIA", fViernes);
                Util.RepTemp(ref template, "CLASSTOTALDIA", planificacionSemanal.GetCssStyle(planificaciones.Sum(X => X.Viernes)));
                Util.RepTemp(ref template, "TOTALDIA", planificaciones.Sum(X => X.Sabado).ToString());
            }
            if (fDia == planificacionSemanal.FechaSabado)
            {
                Util.RepTemp(ref template, "FECHADIA", fSabado);
                Util.RepTemp(ref template, "CLASSTOTALDIA", planificacionSemanal.GetCssStyle(planificaciones.Sum(X => X.Sabado)));
                Util.RepTemp(ref template, "TOTALDIA", planificaciones.Sum(X => X.Domingo).ToString());
            }
            if (fDia == planificacionSemanal.FechaDomingo)
            {
                Util.RepTemp(ref template, "FECHADIA", fDomingo);
                Util.RepTemp(ref template, "CLASSTOTALDIA", planificacionSemanal.GetCssStyle(planificaciones.Sum(X => X.Domingo)));
                Util.RepTemp(ref template, "TOTALDIA", planificaciones.Sum(X => X.Lunes).ToString());
            }

            #endregion

            try
            {
                string path = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/pdfs/controltiempo/planificaciones"), string.Format("{0}.pdf", string.Format("{0}", guid)));

                FileStream fileStream = new FileStream(Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/pdfs/controltiempo/planificaciones"), string.Format("{0}_debug.html", guid)),
                                                       FileMode.OpenOrCreate,
                                                       FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine(template);
                writer.Close();

                NReco.PdfGenerator.HtmlToPdfConverter pdfGenerator = new NReco.PdfGenerator.HtmlToPdfConverter();
                pdfGenerator.Orientation = NReco.PdfGenerator.PageOrientation.Landscape;
                pdfGenerator.Margins.Top = 5;
                pdfGenerator.Margins.Left = 5;
                pdfGenerator.Margins.Bottom = 5;
                pdfGenerator.Margins.Right = 5;
                var pdfBytes = pdfGenerator.GeneratePdf(template);

                System.IO.File.WriteAllBytes(path, pdfBytes);
            }
            catch (Exception ex)
            {
                FileStream fileStream = new FileStream(Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/pdfs/controltiempo/planificaciones"), string.Format("{0}.txt", guid)),
                                                       FileMode.OpenOrCreate,
                                                       FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine(ex.ToString());
                writer.Close();
            }

            return new PlanificacionPdf()
            {
                Guid = guid,
                Titulo = string.Format("Planificación Diaria-{0}-{1}.pdf", (fDia.Value.Day < 10 ? string.Format("0{0}", fDia.Value.Day) : fDia.Value.Day.ToString()), fDia.Value.Month)
            };
        }

        #region SelectList

        public IEnumerable<SelectListItem> GetDiasSemana(int año, int nroSemana)
        {
            DateTime dateTime = new DateTime(año, 1, 1);
            List<CTR_GetSemanasPorAñoResult> semanaList = dc.CTR_GetSemanasPorAño(dateTime).ToList();
            CTR_GetSemanasPorAñoResult semana = semanaList.Single(X => X.WeekNumber == nroSemana);
            List<CTR_GetDiasDeLaSemanaPorDiaResult> diaList = dc.CTR_GetDiasDeLaSemanaPorDia(semana.StartDate).ToList();

            IEnumerable<SelectListItem> selectList = from X in diaList
                                                     select new SelectListItem
                                                     {
                                                         Selected = (fDia.HasValue && X.dates.Value.Date.ToShortDateString() == fDia.Value.Date.ToShortDateString()),
                                                         Text = string.Format("{0:dd/MM/yyyy} - {1}", X.dates, TraduceDia(X.theday)),
                                                         Value = X.dates.Value.ToShortDateString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetPlantaProduccion(int? IdPlantaProduccion, int userID)
        {
            IEnumerable<SelectListItem> selectList = from pp in dc.PlantaProduccion
                                                     join pu in dc.PlantaUsuario on pp.IdPlantaProduccion equals pu.IdPlantaProduccion
                                                     where pu.UserID == userID
                                                     && pp.Habilitado
                                                     select new SelectListItem
                                                     {
                                                         Value = pp.IdPlantaProduccion.ToString(),
                                                         Text = pp.Nombre,
                                                         Selected = (pp.IdPlantaProduccion == IdPlantaProduccion && IdPlantaProduccion != null)
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetEmpresas(int? IdEmpresa)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.Empresa
                                                     orderby X.Nombre
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdEmpresa == IdEmpresa && IdEmpresa != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdEmpresa.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetPaises(string PaisCodigo)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.Pais
                                                     orderby X.PaisNombre
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.PaisCodigo == PaisCodigo && PaisCodigo != null),
                                                         Text = X.PaisNombre,
                                                         Value = X.PaisCodigo.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetTransportistas(int? IdTransportista)
        {
            IEnumerable<SelectListItem> selectList = from x in dc.LOG_Transportista
                                                     where x.Habilitado == true
                                                     orderby x.Nombre
                                                     select new SelectListItem
                                                     {
                                                         Selected = (x.IdTransportista == IdTransportista && IdTransportista != null),
                                                         Text = x.Nombre,
                                                         Value = x.IdTransportista.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetPlantaProduccion(int? IdPlantaProduccion)
        {
            IEnumerable<SelectListItem> selectList = from x in dc.PlantaProduccion
                                                     where x.Habilitado == true
                                                     orderby x.IdPlantaProduccion
                                                     select new SelectListItem
                                                     {
                                                         Selected = (x.IdPlantaProduccion == IdPlantaProduccion && IdPlantaProduccion != null),
                                                         Text = x.Nombre,
                                                         Value = x.IdPlantaProduccion.ToString()
                                                     };
            return selectList;
        }

        #endregion

        public List<CTR_GetSubtotalPlanificacionSemanalResult> GetSubtotales(int año, int nroSemana, int IdEmpresa, int IdProducto, int IdEnvase, int IdCliente, int IdPlantaProduccion)
        {
            return dc.CTR_GetSubtotalPlanificacionSemanal(año, nroSemana, IdEmpresa, IdProducto, IdEnvase, IdCliente, IdPlantaProduccion).OrderBy(X => X.IdEmpresa).ThenBy(X => X.NomPro).ToList();
        }

        #region Notificaciones

        public bool NotificarCreacionPlanificacionSemanal()
        {
            try
            {
                string destinatarios = "";
                var receptores = dc.ReceptoresNotificacionCreacionPlanificacionSemanal().ToList();

                if (receptores.Count == 0)
                    throw new Exception("No hay destinatarios disponibles para notificar la alerta");

                foreach (var receptor in receptores)
                {
                    var email = receptor.Email;
                    destinatarios += (destinatarios != "" ? ", " : "") + email;
                }

                string baseTemplate = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/crearplanificacion_template.html"), Encoding.UTF8);
                Util.RepTempAngularStyle(ref baseTemplate, "LOGO", ResolverLogoEmpresa(this.IdEmpresa));
                Util.RepTempAngularStyle(ref baseTemplate, "EMPRESA", this.Empresa.Nombre);
                Util.RepTempAngularStyle(ref baseTemplate, "SEMANA", this.Semana.ToString());
                Util.RepTempAngularStyle(ref baseTemplate, "AÑO", this.Año.ToString());
                Util.RepTempAngularStyle(ref baseTemplate, "DATETIME", string.Format("{0:dd/MM/yyyy}", DateTime.Now));
                Util.RepTempAngularStyle(ref baseTemplate, "CLIENTE", this.Cliente.RazonSocial);
                Util.RepTempAngularStyle(ref baseTemplate, "LCREQUERIDA", this.LC.ToString("C0"));
                Util.RepTempAngularStyle(ref baseTemplate, "YEAR", DateTime.Now.Year.ToString());

                string subject = string.Format(string.Format("Se ha creado una nueva planificación semanal - {0}", this.Empresa.Nombre));

                Util.SendMail(destinatarios, baseTemplate, subject);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool NotificarEdicionPlanificacionSemanal()
        {
            try
            {
                string destinatarios = "";
                var receptores = dc.ReceptoresNotificacionEdicionPlanificacionSemanal().ToList();

                if (receptores.Count == 0)
                    throw new Exception("No hay destinatarios disponibles para notificar la alerta");

                foreach (var receptor in receptores)
                {
                    var email = receptor.Email;
                    destinatarios += (destinatarios != "" ? ", " : "") + email;
                }

                string baseTemplate = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/editarplanificacion_template.html"), Encoding.UTF8);
                Util.RepTempAngularStyle(ref baseTemplate, "LOGO", ResolverLogoEmpresa(this.IdEmpresa));
                Util.RepTempAngularStyle(ref baseTemplate, "EMPRESA", this.Empresa.Nombre);
                Util.RepTempAngularStyle(ref baseTemplate, "SEMANA", this.Semana.ToString());
                Util.RepTempAngularStyle(ref baseTemplate, "AÑO", this.Año.ToString());
                Util.RepTempAngularStyle(ref baseTemplate, "DATETIME", string.Format("{0:dd/MM/yyyy}", DateTime.Now));
                Util.RepTempAngularStyle(ref baseTemplate, "CLIENTE", this.Cliente.RazonSocial);
                Util.RepTempAngularStyle(ref baseTemplate, "LCREQUERIDA", this.LC.ToString("C0"));
                Util.RepTempAngularStyle(ref baseTemplate, "YEAR", DateTime.Now.Year.ToString());

                string subject = string.Format(string.Format("Se ha editado la planificación semanal - {0}", this.Empresa.Nombre));

                Util.SendMail(destinatarios, baseTemplate, subject);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        public string ResolverLogoEmpresa(int IdEmpresa)
        {
            switch (IdEmpresa)
            {
                case 1:
                    return "oleotop";
                case 2:
                    return "avenatop";
                case 3:
                    return "granotop";
                case 4:
                    return "saprosem";
                case 5:
                    return "ici";
            }

            return "agrotop";
        }

        public string TraduceDia(string dia)
        {
            switch (dia)
            {
                case "Monday":
                    return "Lunes";
                case "Tuesday":
                    return "Martes";
                case "Wednesday":
                    return "Miércoles";
                case "Thursday":
                    return "Jueves";
                case "Friday":
                    return "Viernes";
                case "Saturday":
                    return "Sábado";
                case "Sunday":
                    return "Domingo";
            }

            return "";
        }

        public string TraduceDia(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return "Lu";
                case DayOfWeek.Tuesday:
                    return "Ma";
                case DayOfWeek.Wednesday:
                    return "Mi";
                case DayOfWeek.Thursday:
                    return "Ju";
                case DayOfWeek.Friday:
                    return "Vi";
                case DayOfWeek.Saturday:
                    return "Sá";
                case DayOfWeek.Sunday:
                    return "Do";
            }

            return "";
        }

        public string TraduceDiaLargo(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return "Lunes";
                case DayOfWeek.Tuesday:
                    return "Martes";
                case DayOfWeek.Wednesday:
                    return "Miércoles";
                case DayOfWeek.Thursday:
                    return "Jueves";
                case DayOfWeek.Friday:
                    return "Viernes";
                case DayOfWeek.Saturday:
                    return "Sábado";
                case DayOfWeek.Sunday:
                    return "Domingo";
            }

            return "";
        }

        #region *** Validaciones ***

        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            string actionName = "";
            string controllerName = "";

            var routeValues = HttpContext.Current.Request.RequestContext.RouteData.Values;
            if (routeValues != null)
            {
                if (routeValues.ContainsKey("action"))
                {
                    actionName = routeValues["action"].ToString();
                }
                if (routeValues.ContainsKey("controller"))
                {
                    controllerName = routeValues["controller"].ToString();
                }
            }
            if (this.IdPlantaProduccion == 0)
                yield return new RuleViolation("La planta es requerida", "IdPlantaProduccion");

            if (this.Año == 0)
                yield return new RuleViolation("El año es requerido", "Año");

            if (this.Semana == 0)
                yield return new RuleViolation("La semana es requerido", "Semana");

            if (this.IdEmpresa == 0)
                yield return new RuleViolation("La empresa es requerido", "IdEmpresa");

            if (this.IdProducto == 0)
                yield return new RuleViolation("El producto es requerido", "IdProducto");

            /*if (this.IdEnvase == 0)
                yield return new RuleViolation("El envase es requerido", "IdEnvase");*/

            if (this.IdCliente == 0)
                yield return new RuleViolation("El cliente es requerido", "IdCliente");

            if (string.IsNullOrEmpty(this.PaisCodigo))
                yield return new RuleViolation("El país es requerido", "PaisCodigo");


            #region Validacion de vacíos y nulos, controltiempo

            if ((controllerName.ToLower() == "ctplanificacionsemanal" && actionName.ToLower() != "eliminar") &&
                (controllerName.ToLower() == "ctplanificacionsemanal" && actionName.ToLower() != "eliminarplanificacion") &&
                string.IsNullOrEmpty(this.Destino))
                yield return new RuleViolation("El destino es requerido", "Destino");

            if ((controllerName.ToLower() == "ctporteria" && actionName.ToLower() == "registrarllegada") &&
                string.IsNullOrEmpty(this.RUT))
                yield return new RuleViolation("El RUT es requerido", "RUT");  

            if ((controllerName.ToLower() == "ctporteria" && actionName.ToLower() == "registrarllegada") &&
                string.IsNullOrEmpty(this.Transportista))
                yield return new RuleViolation("El transportista es requerido", "Transportista");

            if ((controllerName.ToLower() == "ctporteria" && actionName.ToLower() == "registrarllegada") &&
                string.IsNullOrEmpty(this.Patente))
                yield return new RuleViolation("La patente es requerida", "Patente");

            if ((controllerName.ToLower() == "ctporteria" && actionName.ToLower() == "registrarllegada") &&
                string.IsNullOrEmpty(this.Chofer))
                yield return new RuleViolation("El nombre del chofer es requerido", "Chofer");

            if ((controllerName.ToLower() == "ctporteria" && actionName.ToLower() == "registrarllegada") &&
                string.IsNullOrEmpty(this.RutChofer))
                yield return new RuleViolation("El RUT del chofer es requerido", "RutChofer");

            if ((controllerName.ToLower() == "ctporteria" && actionName.ToLower() == "registrarllegada") &&
                string.IsNullOrEmpty(this.TipoCamion))
                yield return new RuleViolation("El tipo de camión es requerido", "TipoCamion");

            #endregion

            #region Validaciones de escritura, controltiempo

            if ((controllerName.ToLower() == "ctporteria" && actionName.ToLower() == "registrarllegada") &&
                (Rut.ValidaRut(this.RutChofer) == false))
                yield return new RuleViolation("El RUT del chofer es inválido", "RutChofer");

            if ((controllerName.ToLower() == "ctporteria" && actionName.ToLower() == "registrarllegada") &&
                (Rut.ValidaRut(this.RUT) == false))
                yield return new RuleViolation("El RUT es inválido", "RUT");

            if ((controllerName.ToLower() == "ctporteria" && actionName.ToLower() == "registrarllegada" && !string.IsNullOrEmpty(this.Patente)))
                    if(this.Patente.Length < 6)
                        yield return new RuleViolation("La patente no puede tener menos de 6 carácteres incluyendo espacios", "Patente");


            #endregion


            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }

        #endregion

        #region *** Vistas ***

        public string GetCssStyle(int value)
        {
            return (value > 0 ? "ct-HayTraslados" : "ct-NoHayTraslados");
        }

        public string GetCssStyleRechazado(bool? value)
        {
            if (!value.HasValue)
                return "";
            else
                return (value == true ? "ct-rechazado" : "ct-aprobado");
        }

        public string GetPresentados(DateTime dateTime)
        {
            int presentados = dc.CTR_Porteria.Count(X => X.IdPlanificacionSemanal == this.IdPlanificacionSemanal && X.FechaHoraIns.Date == dateTime.Date && X.Presentado);
            if (presentados == 0)
                return string.Empty;
            else
                return string.Format("<br /><small>Pst.: {0}</small>", presentados);
        }

        public int GetSubtotal()
        {
            return (this.Lunes + this.Martes + this.Miercoles + this.Jueves + this.Viernes + this.Sabado + this.Domingo);
        }

        public int GetTotal(List<CTR_PlanificacionSemanal> list)
        {
            return (list.Sum(X => X.Lunes) + list.Sum(X => X.Martes) + list.Sum(X => X.Miercoles) + list.Sum(X => X.Jueves) + list.Sum(X => X.Viernes) + list.Sum(X => X.Sabado) + list.Sum(X => X.Domingo));
        }



        #endregion

    }

    public class PlanificacionPdf
    {
        public string Guid { get; set; }

        public string Titulo { get; set; }
    }
}