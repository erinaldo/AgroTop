using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class OPR_RegistroTurno
    {
        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public int ComprobarYCrearTurnosFantasmas(OPR_Turno turno, OPR_RegistroTurno registroTurno, string ipIns, int userID, OperacionesDBDataContext dc)
        {
            int contadorTurnosFantasmas = 0;
            int I = 0;
            var OK = true;

            var dw = turno.DiaDeLaSemana;
            var dt = DateTime.Now;
            var tt = turno.IdTipoTurno;

            List<OPR_RegistroTurno> turnosOlvidados = new List<OPR_RegistroTurno>();
            int correlativo = registroTurno.Correlativo;

            var version = dc.OPR_Version.Single(X => X.Activa == true);

            while (OK)
            {
                if (tt == 1)
                {
                    dw -= 1;
                    if (dw == 0)
                        dw = 7;
                    dt = dt.AddDays(-1);
                }

                if (dt.Date != DateTime.Now.Date)
                {
                    if (tt == 1)
                    {
                        if (dw <= 6)
                        {
                            tt = 4; //t + 1
                        }
                        else if (dw == 7)
                        {
                            tt = 3; //t + 1
                        }
                    }
                }

                if (tt > 1)
                {
                    tt -= 1;
                }

                var turnoOlvidado = dc.OPR_RegistroTurno.SingleOrDefault(X => X.FechaHoraIns.Date == dt.Date && X.OPR_Turno.DiaDeLaSemana == dw && X.OPR_Turno.IdTipoTurno == tt && X.Habilitado == true);
                if (turnoOlvidado == null)
                {
                    contadorTurnosFantasmas++;
                    registroTurno.Correlativo++;
                    correlativo++;

                    var turnoAnterior = dc.OPR_Turno.Single(X => X.IdTipoTurno == tt && X.DiaDeLaSemana == dw);

                    turnoOlvidado = new OPR_RegistroTurno();
                    turnoOlvidado.IdTurno = turnoAnterior.IdTurno;
                    turnoOlvidado.UserIns = "automatico";
                    turnoOlvidado.FechaHoraIns = dt;
                    turnoOlvidado.IpIns = ipIns;
                    turnoOlvidado.UserID = userID;
                    turnoOlvidado.Habilitado = true;
                    turnoOlvidado.Correlativo = 0;
                    turnoOlvidado.IdVersion = version.IdVersion;
                    turnoOlvidado.Observaciones = "";

                    turnosOlvidados.Add(turnoOlvidado);

                    dc.SubmitChanges();
                }

                I++;

                //Buscamos turnos olvidados hasta una semana atrás
                if (I > 120)
                    OK = false;
            }

            foreach (var turnoOlvidado in turnosOlvidados)
            {
                correlativo--;
                turnoOlvidado.Correlativo = correlativo;
                dc.OPR_RegistroTurno.InsertOnSubmit(turnoOlvidado);
                dc.SubmitChanges();
            }

            return contadorTurnosFantasmas;
        }

        public string CrearPdfObservaciones()
        {
            string s = "";

            var template = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/observacion_operaciones_template.html"), System.Text.Encoding.UTF8);
            Util.RepTemp(ref template, "TURNO", string.Format("#{0} {1}", this.Correlativo, this.OPR_Turno.OPR_TipoTurno.Descripcion));
            Util.RepTemp(ref template, "OPERADOR", (GetUser(this.UserID) != null ? GetUser(this.UserID).FullName : ""));
            Util.RepTemp(ref template, "HORAS", string.Format("{0} hrs.", this.OPR_Turno.Horas));
            Util.RepTemp(ref template, "DATETIME", this.FechaHoraIns.ToString("dd/MM/yyyy"));
            Util.RepTemp(ref template, "OBSERVACION", this.Observaciones);

            try
            {
                string guid = Guid.NewGuid().ToString();
                string path = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/pdfs/operaciones/observaciones"), string.Format("{0}.pdf", string.Format("{0}-{1}", this.Correlativo, guid)));
                var pdfBytes = (new NReco.PdfGenerator.HtmlToPdfConverter()).GeneratePdf(template);
                System.IO.File.WriteAllBytes(path, pdfBytes);

                s = guid;
            }
            catch (Exception ex)
            {
                FileStream fileStream = new FileStream(Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/pdfs/operaciones/observaciones"), string.Format("{0}-{1}", this.Correlativo, Guid.NewGuid().ToString())), FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine(ex.ToString());
                writer.Close();
            }

            return s;
        }

        public SYS_User GetUser(int UserID)
        {
            return dc.SYS_User.SingleOrDefault(X => X.UserID == UserID);
        }
    }
}