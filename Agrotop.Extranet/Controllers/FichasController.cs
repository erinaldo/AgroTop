using Agrotop.Extranet.Controllers.Filters;
using Agrotop.Extranet.Models;
using ForceManagerLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Agrotop.Extranet.Controllers
{
    [ExtranetAuthorize]
    public class FichasController : BaseController
    {
        public ActionResult Index(int? id)
        {
            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);

            Models.Temporada temporada = null;
            if (id.HasValue)
                temporada = temporadas.SingleOrDefault(t => t.IdTemporada == id.Value);

            if (temporada == null)
                temporada = temporadas.Single(t => t.ActivaFichas);

            if (temporada.IdTemporada < 7)
            {
                var fichas = new Dictionary<int, List<Ficha>>();
                var siembras = new Dictionary<int, List<Siembra>>();
                var predios = new Dictionary<int, List<Predio>>();

                var fichasTemporada = (from fi in dc.Ficha
                                       join pr in dc.Predio on fi.IdPredio equals pr.IdPredio
                                       where fi.IdTemporada == temporada.IdTemporada
                                       && pr.IdAgricultor == user.IdAgricultor
                                       select fi).ToList();
                fichas.Add(temporada.IdTemporada, fichasTemporada);

                var siembrasTemporada = (from fi in dc.Siembra
                                         join pr in dc.Predio on fi.IdPredio equals pr.IdPredio
                                         where fi.IdTemporada == temporada.IdTemporada
                                             && pr.IdAgricultor == user.IdAgricultor
                                         select fi).ToList();
                siembras.Add(temporada.IdTemporada, siembrasTemporada);

                var prediosTemporada = (from fi in dc.Ficha
                                        join pr in dc.Predio on fi.IdPredio equals pr.IdPredio
                                        where fi.IdTemporada == temporada.IdTemporada
                                            && pr.IdAgricultor == user.IdAgricultor
                                        select pr).Distinct().ToList();

                predios.Add(temporada.IdTemporada, prediosTemporada);


                ViewData["temporadas"] = temporadas;
                ViewData["agricultor"] = agricultor;
                ViewData["temporada"] = temporada;

                ViewData["fichas"] = fichas;
                ViewData["siembras"] = siembras;
                ViewData["predios"] = predios;

                return View();
            }
            else
            {
                if (agricultor.IdForceManager.HasValue)
                {
                    Proxy FMProxy = new Proxy();
                    List<ForceManagerLib.Activity> activities = FMProxy.GetAllActivities(agricultor.IdForceManager.Value);
                    ViewData["temporadas"] = temporadas;
                    ViewData["agricultor"] = agricultor;
                    ViewData["temporada"] = temporada;
                    ViewData["activities"] = activities;
                    ViewData["msgerr"] = Request["msgerr"];
                    ViewData["msgok"] = Request["msgok"];
                    return View("FMActivities");
                }
                else
                {
                    ViewData["temporadas"] = temporadas;
                    ViewData["agricultor"] = agricultor;
                    ViewData["temporada"] = temporada;
                    return View("FMIDMissing");
                }
            }
        }

        public ActionResult DescargarFicha(int id, int? IdTemporada)
        {
            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);

            Models.Temporada temporada = null;
            if (IdTemporada.HasValue)
                temporada = temporadas.SingleOrDefault(t => t.IdTemporada == IdTemporada.Value);

            if (temporada == null)
                temporada = temporadas.Single(t => t.ActivaFichas);

            Proxy FMProxy = new Proxy();
            Activity activity = FMProxy.GetASpecificActivity(id);
            if (activity != null)
            {
                string htmlContent = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/asistencia_tecnica_en_terreno_FM_no_footer_template.html"), Encoding.UTF8);
                RepTemp(ref htmlContent, "id"             , activity.id.ToString());
                RepTemp(ref htmlContent, "agricultor"     , agricultor.Nombre);
                RepTemp(ref htmlContent, "rut"            , agricultor.Rut);
                RepTemp(ref htmlContent, "email"          , agricultor.Email);
                RepTemp(ref htmlContent, "fecha"          , activity.dateCreated.ToString("dddd, dd MMMM yyyy"));
                RepTemp(ref htmlContent, "responsable"    , activity.salesRepId.value);
                RepTemp(ref htmlContent, "potrero"        , activity.Z_Potrero);
                RepTemp(ref htmlContent, "cultivo"        , activity.Z_idCultivo != null ? activity.Z_idCultivo.value : "");
                RepTemp(ref htmlContent, "tipo_de_gestion", activity.typeId.value);
                RepTemp(ref htmlContent, "comentarios"    , activity.comment);
                RepTemp(ref htmlContent, "z_agro1"        , activity.Z_Agro1 != null ? activity.Z_Agro1.value : "");
                RepTemp(ref htmlContent, "z_dosis1"       , activity.Z_Dosis1);
                RepTemp(ref htmlContent, "z_umedida1"     , activity.Z_UMedida1);
                RepTemp(ref htmlContent, "z_obs1"         , activity.Z_Obs1);
                RepTemp(ref htmlContent, "z_agro2"        , activity.Z_Agro2 != null ? activity.Z_Agro2.value : "");
                RepTemp(ref htmlContent, "z_dosis2"       , activity.Z_Dosis2);
                RepTemp(ref htmlContent, "z_umedida2"     , activity.Z_UMedida2);
                RepTemp(ref htmlContent, "z_obs2"         , activity.Z_Obs2);
                RepTemp(ref htmlContent, "z_agro3"        , activity.Z_Agro3 != null ? activity.Z_Agro3.value : "");
                RepTemp(ref htmlContent, "z_dosis3"       , activity.Z_Dosis3);
                RepTemp(ref htmlContent, "z_umedida3"     , activity.Z_UMedida3);
                RepTemp(ref htmlContent, "z_obs3"         , activity.Z_Obs3);
                RepTemp(ref htmlContent, "z_agro4"        , activity.Z_Agro4 != null ? activity.Z_Agro4.value : "");
                RepTemp(ref htmlContent, "z_dosis4"       , activity.Z_Dosis4);
                RepTemp(ref htmlContent, "z_umedida4"     , activity.Z_UMedida4);
                RepTemp(ref htmlContent, "z_obs4"         , activity.Z_Obs4);
                RepTemp(ref htmlContent, "z_agro5"        , activity.Z_Agro5 != null ? activity.Z_Agro5.value : "");
                RepTemp(ref htmlContent, "z_dosis5"       , activity.Z_Dosis5);
                RepTemp(ref htmlContent, "z_umedida5"     , activity.Z_UMedida5);
                RepTemp(ref htmlContent, "z_obs5"         , activity.Z_Obs5);
                RepTemp(ref htmlContent, "año"            , DateTime.Now.Year.ToString());

                try
                {
                    string path = string.Format(@"{0}\pdf\{1}.pdf", Properties.Settings.Default.ForceManagerData, Guid.NewGuid().ToString());

                    FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
                    StreamWriter writer = new StreamWriter(fileStream);
                    writer.WriteLine(htmlContent);writer.Close();

                    byte[] pdfBytes = (new NReco.PdfGenerator.HtmlToPdfConverter()).GeneratePdf(htmlContent);
                    System.IO.File.WriteAllBytes(path, pdfBytes); // Requires System.IO

                    return File(path, "application/pdf", string.Format("Ficha-{0}.pdf", activity.id));
                }
                catch (Exception ex)
                {
                    // This code segment write data to file.
                    string pdfkey = Guid.NewGuid().ToString();
                    FileStream fileStream = new FileStream(string.Format(@"{0}\error\{1}", Properties.Settings.Default.ForceManagerData, Guid.NewGuid().ToString(), string.Format("FICHA{0}_{1}.txt", activity.id, pdfkey)), FileMode.OpenOrCreate, FileAccess.Write);
                    StreamWriter writer = new StreamWriter(fileStream);
                    writer.WriteLine("No se pudo generar el PDF de la ficha");
                    writer.WriteLine(ex.ToString());
                    writer.Close();

                    return RedirectToAction("Index", new { id = temporada.IdTemporada, msgerr = "No se pudo generar el PDF de la ficha", msgok = "" });
                }
            }
            else
            {
                return RedirectToAction("Index", new { id = temporada.IdTemporada, msgerr = "No se ha encontrado la ficha", msgok = "" });
            }
        }

        #region --- Funciones PRIVADAS ---

        private void RepTemp(ref string template, string key, string value)
        {
            template = template.Replace("***" + key + "***", value);
        }

        #endregion
    }
}
