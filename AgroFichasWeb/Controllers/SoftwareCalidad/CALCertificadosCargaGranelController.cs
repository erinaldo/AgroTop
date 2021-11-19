using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.SoftwareCalidad
{
    [WebsiteAuthorize]
    public class CALCertificadosCargaGranelController : BaseApplicationController
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();

        public CALCertificadosCargaGranelController()
        {
            SetCurrentModulo(10);
        }

        // GET: CALCertificadosCargaGranel
        public ActionResult Index(int? id, int? IdPlantaProduccion)
        {
            CheckPermisoAndRedirect(377);


            var IdPlantaProduccionSelect = IdPlantaProduccion ?? 0;

            IEnumerable<SelectListItem> plantas = from us in dcAgroFichas.PlantaUsuario
                                                  join pl in dcAgroFichas.PlantaProduccion on us.IdPlantaProduccion equals pl.IdPlantaProduccion
                                                  where us.UserID == CurrentUser.UserID
                                                  && pl.Habilitado
                                                  select new SelectListItem
                                                  {
                                                      Value = pl.IdPlantaProduccion.ToString(),
                                                      Text = pl.Nombre,
                                                      Selected = IdPlantaProduccionSelect == pl.IdPlantaProduccion
                                                  };
            PlantaUsuario pu = CurrentUser.PlantaUsuario.FirstOrDefault();

            if (!object.ReferenceEquals(pu, null) && object.ReferenceEquals(IdPlantaProduccion, null))
            {
                IdPlantaProduccionSelect = pu.IdPlantaProduccion;
            }

            List<CAL_CertificadoCargaGranel> list = dcSoftwareCalidad.CAL_CertificadoCargaGranel
                .Where(X => X.Habilitado == true && X.CAL_LECargaGranel.CAL_OrdenProduccion.IdPlanta == IdPlantaProduccionSelect)
                .OrderByDescending(X => X.FechaHoraIns)
                .OrderByDescending(X => X.IdCertificadoCargaGranel).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["PlantaProduccion"] = plantas;
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(378),
                                                      CheckPermiso(377),
                                                      CheckPermiso(379),
                                                      CheckPermiso(380));
            return View(list);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(378);
            CAL_CertificadoCargaGranel certificadoCargaGranel = new CAL_CertificadoCargaGranel();
            certificadoCargaGranel.NombrePepsico = "Oats Grade B";
            certificadoCargaGranel.Producto = "Oats Groats";
            certificadoCargaGranel.CodigoPepsico = "110000103992";
            certificadoCargaGranel.MaterialEmpaque = "Granel";
            return View(certificadoCargaGranel);
        }

        [HttpPost]
        public ActionResult Crear(CAL_CertificadoCargaGranel certificadoCargaGranel, FormCollection formCollection)
        {
            CheckPermisoAndRedirect(378);
            if (ModelState.IsValid)
            {
                try
                {
                    CAL_GetFichaTecnicaPorDetalleOrdenProduccion_CargaGranelResult result = dcSoftwareCalidad.CAL_GetFichaTecnicaPorDetalleOrdenProduccion_CargaGranel(certificadoCargaGranel.IdDetalleOrdenProduccion).FirstOrDefault();
                    if (result == null)
                        return RedirectToAction("Index", new { errMsg = "No se ha encontrado la ficha técnica", okMsg = "" });

                    //Traemos el producto correspondiente

                    CAL_DetalleOrdenProduccion detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.Where(D => D.IdDetalleOrdenProduccion == certificadoCargaGranel.IdDetalleOrdenProduccion).FirstOrDefault();

                    //Definimos el parametro de busqueda

                    string parametro_busqueda = "";

                    /**
                     * Dependiendo de la id del subproducto el parametro va cambiando
                     * En este caso estaremos buscando el producto groat granel con id 4
                     * Los productos groat granel tienen un correlativo de 000-GG pero todos tienen en comun el -GG por lo que lo definimos como parametro a buscar
                    **/
                    if (detalleOrdenProduccion.IdSubproducto == 4) parametro_busqueda = "-GG";
                    if (detalleOrdenProduccion.IdSubproducto == 3) parametro_busqueda = "-GS";

                    /**
                     * Con el parametro de busqueda ya definidos buscamos todos los certificados que cumplan con esa condición
                     * Después los ordenamos descentemente para poder obtener el último registro ingresado
                     **/

                    CAL_CertificadoCargaGranel comprobacion = (from CG in dcSoftwareCalidad.CAL_CertificadoCargaGranel 
                                                               where CG.NCertificado.ToUpper().Contains(parametro_busqueda) && CG.Habilitado == true
                                                               select CG).OrderBy(CG => CG.IdCertificadoCargaGranel).FirstOrDefault();

                     // Si el objeto comprobación viene nulo quiere decir que no hay ningun certificado que cumpla con dicha condicion y se encuentre vigente, por ende es el primero
                    

                    if (comprobacion == null) certificadoCargaGranel.NCertificado = "000" + parametro_busqueda;

                    /**
                     * Ahora si el dicho objeto viene con contenido, deberemos hacer un substring a los primeros 3 caracteres de NCertificado
                     * 0 , 3

                     */

                    if (comprobacion != null)
                    {
                        var ncertificado = comprobacion.NCertificado.Substring(0, 3);
                        //numero = numero de documento antes del -.A numero se le suma 1 para que en el caso de que sea 000 o sea 0 sea 1 y en el caso que sea 1 se convierta en 2 y así.
                        int numero = Convert.ToInt32(ncertificado) + 1;
                        //Ahora preguntamos el tamaño del número, si es < que 10 se le agregan dos 00 si es > que 9  se le agrega 1 y si es > 99 no se hace nada
                        string string_numero = "";
                        if (numero < 10) string_numero = "00" + numero;
                        if (numero > 9) string_numero = "0" + numero;
                        if (numero > 99) string_numero =  Convert.ToString(numero);
                        //Ahora nuestro número X se junta con el parametro de busqueda y se le asigna a NCertificado
                        certificadoCargaGranel.NCertificado = string_numero + parametro_busqueda;
                    }


                    // Actualizar Luego
                    certificadoCargaGranel.CodigoInterno = result.Codigo;
                    certificadoCargaGranel.NEspecificacion = result.Version;
                    certificadoCargaGranel.Version = result.Version;
                    certificadoCargaGranel.LoteComercial = "";
                    certificadoCargaGranel.Toneladas = 0;
                    certificadoCargaGranel.Reserva = "";
                    certificadoCargaGranel.PaisOrigen = "";
                    certificadoCargaGranel.PaisDestino = "";
                    certificadoCargaGranel.Embarcador = "";
                    certificadoCargaGranel.Consignatario = "";
                    certificadoCargaGranel.certkey = "";
                    certificadoCargaGranel.NFactura = "0";
                    // Rescatados del Form
                    certificadoCargaGranel.NombrePepsico = certificadoCargaGranel.NombrePepsico.ToUpper();
                    certificadoCargaGranel.Producto = certificadoCargaGranel.Producto.ToUpper();
                    certificadoCargaGranel.CodigoPepsico = certificadoCargaGranel.CodigoPepsico.ToUpper();
                    certificadoCargaGranel.FechaElaboracion = certificadoCargaGranel.FechaElaboracion.ToUpper();
                    certificadoCargaGranel.MaterialEmpaque = certificadoCargaGranel.MaterialEmpaque.ToUpper();
                    certificadoCargaGranel.CodigoProceso = certificadoCargaGranel.CodigoProceso.ToUpper();
                    certificadoCargaGranel.Habilitado = true;
                    certificadoCargaGranel.FechaHoraIns = DateTime.Now;
                    certificadoCargaGranel.IpIns = RemoteAddr();
                    certificadoCargaGranel.UserIns = User.Identity.Name;
                    dcSoftwareCalidad.CAL_CertificadoCargaGranel.InsertOnSubmit(certificadoCargaGranel);
                    dcSoftwareCalidad.SubmitChanges();
                    // Actualizado
                    certificadoCargaGranel.LoteComercial = certificadoCargaGranel.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.LoteComercial;
                    certificadoCargaGranel.Toneladas = certificadoCargaGranel.CAL_LECargaGranel.PesoNetoTotal;
                    certificadoCargaGranel.Reserva = string.Format("{0} {1} {2}", certificadoCargaGranel.CAL_LECargaGranel.Carrier.Nombre, certificadoCargaGranel.CAL_LECargaGranel.Barco.Nombre, certificadoCargaGranel.CAL_LECargaGranel.NReserva);
                    certificadoCargaGranel.PaisOrigen = "CHILE";
                    certificadoCargaGranel.PaisDestino = certificadoCargaGranel.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.GetPais(certificadoCargaGranel.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.PaisCodigo).PaisNombre.ToUpper();
                    certificadoCargaGranel.Embarcador = "AVENATOP SpA";
                    certificadoCargaGranel.Consignatario = certificadoCargaGranel.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.GetCliente(certificadoCargaGranel.CAL_DetalleOrdenProduccion.CAL_OrdenProduccion.IdCliente).RazonSocial.ToUpper();
                    certificadoCargaGranel.NFactura = string.Format("{0}", certificadoCargaGranel.CAL_LECargaGranel.NFactura);
                    dcSoftwareCalidad.SubmitChanges();

                    CAL_CertificadoCargaGranelAnalisisPeriodico certificadoCargaGranelAnalisisPeriodico = new CAL_CertificadoCargaGranelAnalisisPeriodico()
                    {
                        IdCertificadoCargaGranel = certificadoCargaGranel.IdCertificadoCargaGranel,
                        FechaHoraIns = DateTime.Now,
                        IpIns = RemoteAddr(),
                        UserIns = User.Identity.Name
                    };
                    if (!string.IsNullOrEmpty(formCollection["IdAnalisisPeriodicoPesticidas"]))
                        if (int.TryParse(formCollection["IdAnalisisPeriodicoPesticidas"], out int IdAnalisisPeriodicoPesticidas))
                            certificadoCargaGranelAnalisisPeriodico.IdAnalisisPeriodicoPesticidas = IdAnalisisPeriodicoPesticidas;
                    if (!string.IsNullOrEmpty(formCollection["IdAnalisisPeriodicoMetalesPesados"]))
                        if (int.TryParse(formCollection["IdAnalisisPeriodicoMetalesPesados"], out int IdAnalisisPeriodicoMetalesPesados))
                            certificadoCargaGranelAnalisisPeriodico.IdAnalisisPeriodicoMetalesPesados = IdAnalisisPeriodicoMetalesPesados;
                    if (!string.IsNullOrEmpty(formCollection["IdAnalisisPeriodicoMicotoxinas"]))
                        if (int.TryParse(formCollection["IdAnalisisPeriodicoMicotoxinas"], out int IdAnalisisPeriodicoMicotoxinas))
                            certificadoCargaGranelAnalisisPeriodico.IdAnalisisPeriodicoMicotoxinas = IdAnalisisPeriodicoMicotoxinas;
                    if (!string.IsNullOrEmpty(formCollection["IdAnalisisPeriodicoMicrobiologia"]))
                        if (int.TryParse(formCollection["IdAnalisisPeriodicoMicrobiologia"], out int IdAnalisisPeriodicoMicrobiologia))
                            certificadoCargaGranelAnalisisPeriodico.IdAnalisisPeriodicoMicrobiologia = IdAnalisisPeriodicoMicrobiologia;
                    if (!string.IsNullOrEmpty(formCollection["IdAnalisisPeriodicoNutricional"]))
                        if (int.TryParse(formCollection["IdAnalisisPeriodicoNutricional"], out int IdAnalisisPeriodicoNutricional))
                            certificadoCargaGranelAnalisisPeriodico.IdAnalisisPeriodicoNutricional = IdAnalisisPeriodicoNutricional;
                    dcSoftwareCalidad.CAL_CertificadoCargaGranelAnalisisPeriodico.InsertOnSubmit(certificadoCargaGranelAnalisisPeriodico);
                    dcSoftwareCalidad.SubmitChanges();

                    string[] ids = { };

                    if (!string.IsNullOrEmpty(formCollection["IdMetodologias"]))
                        ids = formCollection["IdMetodologias"].Split(',');

                    foreach (string idx in ids)
                    {
                        CAL_CertificadoCargaGranelMetodologia certificadoCargaGranelMetodologia = new CAL_CertificadoCargaGranelMetodologia()
                        {
                            IdCertificadoCargaGranel = certificadoCargaGranel.IdCertificadoCargaGranel,
                            IdMetodologia = int.Parse(idx),
                            FechaHoraIns = DateTime.Now,
                            IpIns = RemoteAddr(),
                            UserIns = User.Identity.Name
                        };
                        dcSoftwareCalidad.CAL_CertificadoCargaGranelMetodologia.InsertOnSubmit(certificadoCargaGranelMetodologia);
                        dcSoftwareCalidad.SubmitChanges();
                    }
                    certificadoCargaGranel.NotificarCreacion();
                    return RedirectToAction("Index");
                }
                catch
                {
                    var rv = certificadoCargaGranel.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            return View(certificadoCargaGranel);
        }

        public ActionResult CrearPDF(int id)
        {
            CheckPermisoAndRedirect(377);
            CAL_CertificadoCargaGranel certificadoCargaGranel = dcSoftwareCalidad.CAL_CertificadoCargaGranel.SingleOrDefault(X => X.IdCertificadoCargaGranel == id && X.Habilitado == true);
            if (certificadoCargaGranel == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            var consig = certificadoCargaGranel.Consignatario;

            CAL_ResponsableArea responsableArea = dcSoftwareCalidad.CAL_ResponsableArea.FirstOrDefault(X => X.IdArea == 2);


            string htmlContent = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/certificados/calidad_final_carga_a_granel_template.html"), Encoding.UTF8);
            if (consig.Equals("CEREALES EN GENERAL S.A.S.") || consig.Equals("ALIMENTOS S.A.") || consig.Equals("CEREALES DE CENTROAMERICA S.A."))
            {
                htmlContent = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/certificados/calidad_final_carga_a_granel_template_caricam.html"), Encoding.UTF8);
            }
            RepTemp(ref htmlContent, "FECHA", DateTime.Now.ToString("dddd, dd MMMM yyyy"));
            RepTemp(ref htmlContent, "NO", certificadoCargaGranel.NCertificado.ToString());
            RepTemp(ref htmlContent, "AÑO", DateTime.Now.Year.ToString());
            RepTemp(ref htmlContent, "PEPNOM", certificadoCargaGranel.NombrePepsico);
            RepTemp(ref htmlContent, "PROD", certificadoCargaGranel.Producto);
            RepTemp(ref htmlContent, "PEPCOD", certificadoCargaGranel.CodigoPepsico);
            RepTemp(ref htmlContent, "FTCOD", certificadoCargaGranel.CodigoInterno);
            RepTemp(ref htmlContent, "FTVER", certificadoCargaGranel.Version.ToString());
            RepTemp(ref htmlContent, "FTVER", certificadoCargaGranel.Version.ToString());
            RepTemp(ref htmlContent, "LOTCOM", certificadoCargaGranel.LoteComercial);
            RepTemp(ref htmlContent, "FECELAB", certificadoCargaGranel.FechaElaboracion);
            RepTemp(ref htmlContent, "MATEMP", certificadoCargaGranel.MaterialEmpaque);
            RepTemp(ref htmlContent, "CODPROC", certificadoCargaGranel.CodigoProceso);
            RepTemp(ref htmlContent, "TON", string.Format("{0:N2}", certificadoCargaGranel.Toneladas / 1000));
            RepTemp(ref htmlContent, "RSRVA", certificadoCargaGranel.Reserva);
            RepTemp(ref htmlContent, "ORG", certificadoCargaGranel.PaisOrigen);
            RepTemp(ref htmlContent, "DST", certificadoCargaGranel.PaisDestino);
            RepTemp(ref htmlContent, "EMB", certificadoCargaGranel.Embarcador);
            RepTemp(ref htmlContent, "CONSIG", certificadoCargaGranel.Consignatario);
            RepTemp(ref htmlContent, "FACT", string.Format("{0}", certificadoCargaGranel.NFactura));
            RepTemp(ref htmlContent, "RESPONSABLE", responsableArea != null ? responsableArea.GetUser(responsableArea.UserID.Value) != null ? responsableArea.GetUser(responsableArea.UserID.Value).FullName : "" : "");

            // Análisis
            StringBuilder builderGranulometria = new StringBuilder();
            List<CAL_GetPromedioTestGranulometria_CargaGranelResult> testsGranulometria = dcSoftwareCalidad.CAL_GetPromedioTestGranulometria_CargaGranel(certificadoCargaGranel.IdDetalleOrdenProduccion, certificadoCargaGranel.IdLECargaGranel).ToList();
            foreach (var test in testsGranulometria)
            {
                builderGranulometria.AppendLine("<tr>");
                builderGranulometria.AppendLine(string.Format("<td class=\"text-align-left\">{0}</td>", test.Nombre));
                builderGranulometria.AppendLine(string.Format("<td class=\"text-align-center\">{0}</td>", test.UM));
                builderGranulometria.AppendLine(string.Format("<td class=\"text-align-center\">{0}</td>", Formatter.Format(test.Promedio.Value, test.UM, test.FormatString, test.IdParametroAnalisis)));
                builderGranulometria.AppendLine(string.Format("<td class=\"text-align-center\">{0}</td>", string.Format("<table><tr><td align=\"center\">{0}</td><td align=\"center\">{1}</td></tr></table>", Formatter.Format(test.MinValidValue.Value, test.UM, test.FormatString, test.IdParametroAnalisis), Formatter.Format(test.MaxValidValue.Value, test.UM, test.FormatString, test.IdParametroAnalisis))));
                builderGranulometria.AppendLine("<td class=\"text-align-center\">Por Lote</td>");
                builderGranulometria.AppendLine("</tr>");
            }
            RepTemp(ref htmlContent, "GRANULOMETRIA", builderGranulometria.ToString());

            CAL_CertificadoCargaGranelAnalisisPeriodico certificadoCargaGranelAnalisisPeriodico = dcSoftwareCalidad.CAL_CertificadoCargaGranelAnalisisPeriodico.SingleOrDefault(X => X.IdCertificadoCargaGranel == certificadoCargaGranel.IdCertificadoCargaGranel);
            if (certificadoCargaGranelAnalisisPeriodico != null)
            {
                if (certificadoCargaGranelAnalisisPeriodico.IdAnalisisPeriodicoPesticidas.HasValue)
                {
                    StringBuilder builder = new StringBuilder();
                    List<CAL_GetTestAnalisisPesticidaResult> tests = GetTestAnalisisPesticida(certificadoCargaGranelAnalisisPeriodico.IdAnalisisPeriodicoPesticidas.Value);
                    foreach (var test in tests)
                    {
                        builder.AppendLine("<tr>");
                        builder.AppendLine(string.Format("<td class=\"text-align-left\">{0}</td>", test.Nombre));
                        builder.AppendLine(string.Format("<td class=\"text-align-center\">{0}</td>", test.UM));
                        builder.AppendLine(string.Format("<td class=\"text-align-center\">{0}</td>", Formatter.Format(test.Value, test.UM, (test.FormatStringTest.Value ? test.FormatString : "#"))));
                        builder.AppendLine(string.Format("<td class=\"text-align-center\">{0}</td>", string.Format("<table><tr><td align=\"center\">{0}</td><td align=\"center\">{1}</td></tr></table>", Formatter.Format(test.MinValidValue.Value, test.UM, test.FormatString), Formatter.Format(test.MaxValidValue.Value, test.UM, test.FormatString))));
                        builder.AppendLine(string.Format("<td class=\"text-align-right\">{0}</td>", test.Frecuencia));
                        builder.AppendLine("</tr>");
                    }
                    RepTemp(ref htmlContent, "PARAMETROSPESTICIDAS", builder.ToString());
                    RepTemp(ref htmlContent, "TITULOPARAMETROSPESTICIDAS", "");
                    RepTemp(ref htmlContent, "CUERPOPARAMETROSPESTICIDAS", "");
                }
                else
                {
                    RepTemp(ref htmlContent, "PARAMETROSPESTICIDAS", string.Empty);
                    RepTemp(ref htmlContent, "TITULOPARAMETROSPESTICIDAS", "display: none");
                    RepTemp(ref htmlContent, "CUERPOPARAMETROSPESTICIDAS", "display: none");
                }

                if (certificadoCargaGranelAnalisisPeriodico.IdAnalisisPeriodicoMetalesPesados.HasValue)
                {
                    StringBuilder builder = new StringBuilder();
                    List<CAL_GetTestAnalisisMetalPesadoResult> tests = GetTestAnalisisMetalPesado(certificadoCargaGranelAnalisisPeriodico.IdAnalisisPeriodicoMetalesPesados.Value);
                    foreach (var test in tests)
                    {
                        builder.AppendLine("<tr>");
                        builder.AppendLine(string.Format("<td class=\"text-align-left\">{0}</td>", test.Nombre));
                        builder.AppendLine(string.Format("<td class=\"text-align-center\">{0}</td>", test.UM));
                        builder.AppendLine(string.Format("<td class=\"text-align-center\">{0}</td>", Formatter.Format(test.Value, test.UM, (test.FormatStringTest.Value ? test.FormatString : "#"))));
                        builder.AppendLine(string.Format("<td class=\"text-align-center\">{0}</td>", string.Format("<table><tr><td align=\"center\">{0}</td><td align=\"center\">{1}</td></tr></table>", Formatter.Format(test.MinValidValue.Value, test.UM, test.FormatString), Formatter.Format(test.MaxValidValue.Value, test.UM, test.FormatString))));
                        builder.AppendLine(string.Format("<td class=\"text-align-right\">{0}</td>", test.Frecuencia));
                        builder.AppendLine("</tr>");
                    }
                    RepTemp(ref htmlContent, "PARAMETROSMETALESPESADOS", builder.ToString());
                    RepTemp(ref htmlContent, "TITULOPARAMETROSMETALESPESADOS", "");
                    RepTemp(ref htmlContent, "CUERPOPARAMETROSMETALESPESADOS", "");
                }
                else
                {
                    RepTemp(ref htmlContent, "PARAMETROSMETALESPESADOS", string.Empty);
                    RepTemp(ref htmlContent, "TITULOPARAMETROSMETALESPESADOS", "display: none");
                    RepTemp(ref htmlContent, "CUERPOPARAMETROSMETALESPESADOS", "display: none");
                }

                if (certificadoCargaGranelAnalisisPeriodico.IdAnalisisPeriodicoMicotoxinas.HasValue)
                {
                    StringBuilder builder = new StringBuilder();
                    List<CAL_GetTestAnalisisMicotoxinaResult> tests = GetTestAnalisisMicotoxina(certificadoCargaGranelAnalisisPeriodico.IdAnalisisPeriodicoMicotoxinas.Value);
                    foreach (var test in tests)
                    {
                        builder.AppendLine("<tr>");
                        builder.AppendLine(string.Format("<td class=\"text-align-left\">{0}</td>", test.Nombre));
                        builder.AppendLine(string.Format("<td class=\"text-align-center\">{0}</td>", test.UM));
                        builder.AppendLine(string.Format("<td class=\"text-align-center\">{0}</td>", Formatter.Format(test.Value, test.UM, (test.FormatStringTest.Value ? test.FormatString : "#"))));
                        builder.AppendLine(string.Format("<td class=\"text-align-center\">{0}</td>", string.Format("<table><tr><td align=\"center\">{0}</td><td align=\"center\">{1}</td></tr></table>", Formatter.Format(test.MinValidValue.Value, test.UM, test.FormatString), Formatter.Format(test.MaxValidValue.Value, test.UM, test.FormatString))));
                        builder.AppendLine(string.Format("<td class=\"text-align-right\">{0}</td>", test.Frecuencia));
                        builder.AppendLine("</tr>");
                    }
                    RepTemp(ref htmlContent, "PARAMETROSMICOTOXINAS", builder.ToString());
                    RepTemp(ref htmlContent, "TITULOPARAMETROSMICOTOXINAS", "");
                    RepTemp(ref htmlContent, "CUERPOPARAMETROSMICOTOXINAS", "");
                }
                else
                {
                    RepTemp(ref htmlContent, "PARAMETROSMICOTOXINAS", string.Empty);
                    RepTemp(ref htmlContent, "TITULOPARAMETROSMICOTOXINAS", "display: none");
                    RepTemp(ref htmlContent, "CUERPOPARAMETROSMICOTOXINAS", "display: none");
                }

                if (certificadoCargaGranelAnalisisPeriodico.IdAnalisisPeriodicoMicrobiologia.HasValue)
                {
                    StringBuilder builder = new StringBuilder();
                    List<CAL_GetTestAnalisisMicrobiologiaResult> tests = GetTestAnalisisMicrobiologia(certificadoCargaGranelAnalisisPeriodico.IdAnalisisPeriodicoMicrobiologia.Value);
                    foreach (var test in tests)
                    {
                        builder.AppendLine("<tr>");
                        builder.AppendLine(string.Format("<td class=\"text-align-left\">{0}</td>", test.Nombre));
                        builder.AppendLine(string.Format("<td class=\"text-align-center\">{0}</td>", test.UM));
                        builder.AppendLine(string.Format("<td class=\"text-align-center\">{0}</td>", Formatter.Format(test.Value, test.UM, (test.FormatStringTest.Value ? test.FormatString : "#"))));
                        builder.AppendLine(string.Format("<td class=\"text-align-center\">{0}</td>", string.Format("<table><tr><td align=\"center\">{0}</td><td align=\"center\">{1}</td></tr></table>", Formatter.Format(test.MinValidValue.Value, test.UM, test.FormatString), Formatter.Format(test.MaxValidValue.Value, test.UM, test.FormatString))));
                        builder.AppendLine(string.Format("<td class=\"text-align-right\">{0}</td>", test.Frecuencia));
                        builder.AppendLine("</tr>");
                    }
                    RepTemp(ref htmlContent, "PARAMETROSMICROBIOLOGICOS", builder.ToString());
                    RepTemp(ref htmlContent, "TITULOPARAMETROSMICROBIOLOGICOS", "");
                    RepTemp(ref htmlContent, "CUERPOPARAMETROSMICROBIOLOGICOS", "");
                }
                else
                {
                    RepTemp(ref htmlContent, "PARAMETROSMICROBIOLOGICOS", string.Empty);
                    RepTemp(ref htmlContent, "TITULOPARAMETROSMICROBIOLOGICOS", "display: none");
                    RepTemp(ref htmlContent, "CUERPOPARAMETROSMICROBIOLOGICOS", "display: none");
                }

                if (certificadoCargaGranelAnalisisPeriodico.IdAnalisisPeriodicoNutricional.HasValue)
                {
                    StringBuilder builder = new StringBuilder();
                    List<CAL_GetTestAnalisisNutricionalResult> tests = GetTestAnalisisNutricional(certificadoCargaGranelAnalisisPeriodico.IdAnalisisPeriodicoNutricional.Value);
                    foreach (var test in tests)
                    {
                        builder.AppendLine("<tr>");
                        builder.AppendLine(string.Format("<td class=\"text-align-left\">{0}</td>", test.Nombre));
                        builder.AppendLine(string.Format("<td class=\"text-align-center\">{0}</td>", test.UM));
                        builder.AppendLine(string.Format("<td class=\"text-align-center\">{0}</td>", Formatter.Format(test.Value, test.UM, (test.FormatStringTest.Value ? test.FormatString : "#"))));
                        builder.AppendLine(string.Format("<td class=\"text-align-center\">{0}</td>", string.Format("<table><tr><td align=\"center\">{0}</td><td align=\"center\">{1}</td></tr></table>", Formatter.Format(test.MinValidValue.Value, test.UM, test.FormatString), Formatter.Format(test.MaxValidValue.Value, test.UM, test.FormatString))));
                        builder.AppendLine(string.Format("<td class=\"text-align-right\">{0}</td>", test.Frecuencia));
                        builder.AppendLine("</tr>");
                    }
                    RepTemp(ref htmlContent, "PARAMETROSNUTRICIONALES", builder.ToString());
                    RepTemp(ref htmlContent, "TITULOPARAMETROSNUTRICIONALES", "");
                    RepTemp(ref htmlContent, "CUERPOPARAMETROSNUTRICIONALES", "");
                }
                else
                {
                    RepTemp(ref htmlContent, "PARAMETROSNUTRICIONALES", string.Empty);
                    RepTemp(ref htmlContent, "TITULOPARAMETROSNUTRICIONALES", "display: none");
                    RepTemp(ref htmlContent, "CUERPOPARAMETROSNUTRICIONALES", "display: none");
                }
            }
            else
            {
                RepTemp(ref htmlContent, "PARAMETROSPESTICIDAS", string.Empty);
                RepTemp(ref htmlContent, "TITULOPARAMETROSPESTICIDAS", "display: none");
                RepTemp(ref htmlContent, "CUERPOPARAMETROSPESTICIDAS", "display: none");
                RepTemp(ref htmlContent, "PARAMETROSMETALESPESADOS", string.Empty);
                RepTemp(ref htmlContent, "TITULOPARAMETROSMETALESPESADOS", "display: none");
                RepTemp(ref htmlContent, "CUERPOPARAMETROSMETALESPESADOS", "display: none");
                RepTemp(ref htmlContent, "PARAMETROSMICOTOXINAS", string.Empty);
                RepTemp(ref htmlContent, "TITULOPARAMETROSMICOTOXINAS", "display: none");
                RepTemp(ref htmlContent, "CUERPOPARAMETROSMICOTOXINAS", "display: none");
                RepTemp(ref htmlContent, "PARAMETROSMICROBIOLOGICOS", string.Empty);
                RepTemp(ref htmlContent, "TITULOPARAMETROSMICROBIOLOGICOS", "display: none");
                RepTemp(ref htmlContent, "CUERPOPARAMETROSMICROBIOLOGICOS", "display: none");
                RepTemp(ref htmlContent, "PARAMETROSNUTRICIONALES", string.Empty);
                RepTemp(ref htmlContent, "TITULOPARAMETROSNUTRICIONALES", "display: none");
                RepTemp(ref htmlContent, "CUERPOPARAMETROSNUTRICIONALES", "display: none");
            }

            StringBuilder builderMetodologia = new StringBuilder();
            List<CAL_CertificadoCargaGranelMetodologia> metodologiasList = dcSoftwareCalidad.CAL_CertificadoCargaGranelMetodologia.Where(X => X.IdCertificadoCargaGranel == certificadoCargaGranel.IdCertificadoCargaGranel).ToList();
            if (metodologiasList.Count > 0)
            {
                foreach (CAL_CertificadoCargaGranelMetodologia metodologia in metodologiasList)
                {
                    builderMetodologia.AppendLine("<tr>");
                    builderMetodologia.AppendLine(string.Format("<td class=\"text-align-left\"><strong>{0}</strong></td>", metodologia.CAL_Metodologia.Analisis));
                    builderMetodologia.AppendLine(string.Format("<td class=\"text-align-left\">: {0}</td>", metodologia.CAL_Metodologia.Tecnica));
                    builderMetodologia.AppendLine("</tr>");
                }
                RepTemp(ref htmlContent, "METODOLOGIA", builderMetodologia.ToString());
                RepTemp(ref htmlContent, "TITULOMETODOLOGIA", "");
                RepTemp(ref htmlContent, "CUERPOMETODOLOGIA", "");
            }
            else
            {
                RepTemp(ref htmlContent, "METODOLOGIA", string.Empty);
                RepTemp(ref htmlContent, "TITULOMETODOLOGIA", "display: none");
                RepTemp(ref htmlContent, "CUERPOMETODOLOGIA", "display: none");
            }

            try
            {
                string certkey = Guid.NewGuid().ToString();
                FileStream fileStream = new FileStream(Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/certificados"), string.Format("{0}.html", certkey)), FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine(htmlContent); writer.Close();

                string path = Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/certificados"), string.Format("{0}.pdf", certkey));
                byte[] pdfBytes = (new NReco.PdfGenerator.HtmlToPdfConverter()
                {
                    Orientation = NReco.PdfGenerator.PageOrientation.Portrait,
                    Margins = new NReco.PdfGenerator.PageMargins()
                    {
                        Bottom = 15,
                        Left = 15,
                        Right = 15,
                        Top = 15
                    },
                    Size = NReco.PdfGenerator.PageSize.Letter,
                }).GeneratePdf(htmlContent);
                System.IO.File.WriteAllBytes(path, pdfBytes); // Requires System.IO

                certificadoCargaGranel.certkey = certkey;
                dcSoftwareCalidad.SubmitChanges();

                CAL_CertificadoControlVersion controlVersion = new CAL_CertificadoControlVersion()
                {
                    IdTipoCertificado = 9,
                    IdCertificado = certificadoCargaGranel.IdCertificadoCargaGranel,
                    certkey = certificadoCargaGranel.certkey,
                    Habilitado = true,
                    FechaHoraIns = DateTime.Now,
                    IpIns = RemoteAddr(),
                    UserIns = User.Identity.Name
                };
                dcSoftwareCalidad.CAL_CertificadoControlVersion.InsertOnSubmit(controlVersion);
                dcSoftwareCalidad.SubmitChanges();

                return RedirectToAction("DescargarPDF", new { id = certificadoCargaGranel.IdCertificadoCargaGranel, certkey = certkey });
            }
            catch (Exception ex)
            {
                // This code segment write data to file.
                string certkey = Guid.NewGuid().ToString();
                FileStream fileStream = new FileStream(Path.Combine(Server.MapPath("~/App_Data/pdfs/softwarecalidad/certificados"), string.Format("IDCERT{0}_{1}.txt", certificadoCargaGranel.IdCertificadoCargaGranel, certkey)), FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine("No se pudo generar el PDF del certificado");
                writer.WriteLine(ex.ToString());
                writer.Close();

                return RedirectToAction("Index", new { errMsg = "No se pudo generar el PDF del certificado", okMsg = "" });
            }
        }

        public ActionResult DescargarPDF(int id, string certkey)
        {
            CheckPermisoAndRedirect(377);
            CAL_CertificadoCargaGranel certificadoCargaGranel = dcSoftwareCalidad.CAL_CertificadoCargaGranel.SingleOrDefault(X => X.IdCertificadoCargaGranel == id && X.certkey == certkey && X.Habilitado == true);
            if (certificadoCargaGranel == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            try
            {
                string pdf = Server.MapPath(string.Format("~/App_Data/pdfs/softwarecalidad/certificados/{0}.pdf", certificadoCargaGranel.certkey));
                return File(pdf, "application/pdf", String.Format("Certificado de Calidad {0}-{1}.pdf", certificadoCargaGranel.NCertificado, certificadoCargaGranel.FechaHoraIns.Year));
            }
            catch
            {
                return RedirectToAction("Index", new { errMsg = "No se pudo descargar el PDF del certificado", okMsg = "" });
            }
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(380);
            CAL_CertificadoCargaGranel certificadoCargaGranel = dcSoftwareCalidad.CAL_CertificadoCargaGranel.SingleOrDefault(X => X.IdCertificadoCargaGranel == id && X.Habilitado == true);
            if (certificadoCargaGranel == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                certificadoCargaGranel.Habilitado = false;
                certificadoCargaGranel.UserUpd = User.Identity.Name;
                certificadoCargaGranel.FechaHoraUpd = DateTime.Now;
                certificadoCargaGranel.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("El certificado {0} ha sido eliminado", certificadoCargaGranel.IdCertificadoCargaGranel);

                certificadoCargaGranel.NotificarEliminacion();
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("Index", new { errMsg = errMsg, okMsg = okMsg });
        }

        public ActionResult ControlVersiones(int id)
        {
            CheckPermisoAndRedirect(377);
            CAL_CertificadoCargaGranel certificadoCargaGranel = dcSoftwareCalidad.CAL_CertificadoCargaGranel.SingleOrDefault(X => X.IdCertificadoCargaGranel == id && X.Habilitado == true);
            if (certificadoCargaGranel == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            List<CAL_CertificadoControlVersion> list = dcSoftwareCalidad.CAL_CertificadoControlVersion.Where(X => X.IdCertificado == certificadoCargaGranel.IdCertificadoCargaGranel && X.IdTipoCertificado == 9 && X.Habilitado == true).OrderByDescending(X => X.FechaHoraIns).ToList();
            ViewData["errMsg"] = Request["errMsg"];
            ViewData["okMsg"] = Request["okMsg"];
            ViewData["permisosUsuario"] = new Permiso(CheckPermiso(378),
                                                      CheckPermiso(377),
                                                      CheckPermiso(379),
                                                      CheckPermiso(380));
            ViewData["certificadoCargaGranel"] = certificadoCargaGranel;
            return View(list);
        }

        public ActionResult DescargarPDF_ControlVersion(int id, string certkey)
        {
            CheckPermisoAndRedirect(377);
            CAL_CertificadoCargaGranel certificadoCargaGranel = dcSoftwareCalidad.CAL_CertificadoCargaGranel.SingleOrDefault(X => X.IdCertificadoCargaGranel == id && X.Habilitado == true);
            if (certificadoCargaGranel == null)
            {
                return RedirectToAction("Index", new { errMsg = "No se ha encontrado el certificado", okMsg = "" });
            }

            CAL_CertificadoControlVersion controlVersion = dcSoftwareCalidad.CAL_CertificadoControlVersion.SingleOrDefault(X => X.certkey == certkey);
            if (controlVersion == null)
            {
                return RedirectToAction("ControlVersiones", new { errMsg = "No se ha encontrado el control de versión", okMsg = "" });
            }

            try
            {
                string pdf = Server.MapPath(string.Format("~/App_Data/pdfs/softwarecalidad/certificados/{0}.pdf", controlVersion.certkey));
                return File(pdf, "application/pdf", String.Format("Certificado de Calidad {0}-{1} V{2:dd-MM-yy HH;mm;ss}.pdf", certificadoCargaGranel.NCertificado, certificadoCargaGranel.FechaHoraIns.Year, controlVersion.FechaHoraIns));
            }
            catch
            {
                return RedirectToAction("ControlVersiones", new { errMsg = "No se pudo descargar el PDF del certificado", okMsg = "" });
            }
        }

        public ActionResult Eliminar_ControlVersion(int id)
        {
            CheckPermisoAndRedirect(380);
            CAL_CertificadoControlVersion controlVersion = dcSoftwareCalidad.CAL_CertificadoControlVersion.SingleOrDefault(X => X.IdCertificadoControlVersion == id);
            if (controlVersion == null)
            {
                return RedirectToAction("ControlVersiones", new { errMsg = "No se ha encontrado el control de versión", okMsg = "" });
            }

            string errMsg = "";
            string okMsg = "";

            try
            {
                controlVersion.Habilitado = false;
                controlVersion.UserUpd = User.Identity.Name;
                controlVersion.FechaHoraUpd = DateTime.Now;
                controlVersion.IpUpd = RemoteAddr();
                dcSoftwareCalidad.SubmitChanges();
                okMsg = String.Format("El control de versión {0} ha sido eliminado", controlVersion.IdCertificadoControlVersion);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return RedirectToAction("ControlVersiones", new { id = controlVersion.IdCertificado, errMsg = errMsg, okMsg = okMsg });
        }

        #region --- Funciones PRIVADAS ---

        private List<CAL_GetTestAnalisisPesticidaResult> GetTestAnalisisPesticida(int IdAnalisisPeriodicoPesticidas)
        {
            return dcSoftwareCalidad.CAL_GetTestAnalisisPesticida(IdAnalisisPeriodicoPesticidas).ToList();
        }

        private List<CAL_GetTestAnalisisMetalPesadoResult> GetTestAnalisisMetalPesado(int IdAnalisisPeriodicoMetalesPesados)
        {
            return dcSoftwareCalidad.CAL_GetTestAnalisisMetalPesado(IdAnalisisPeriodicoMetalesPesados).ToList();
        }

        private List<CAL_GetTestAnalisisMicotoxinaResult> GetTestAnalisisMicotoxina(int IdAnalisisPeriodicoMicotoxinas)
        {
            return dcSoftwareCalidad.CAL_GetTestAnalisisMicotoxina(IdAnalisisPeriodicoMicotoxinas).ToList();
        }

        private List<CAL_GetTestAnalisisMicrobiologiaResult> GetTestAnalisisMicrobiologia(int IdAnalisisPeriodicoMicrobiologia)
        {
            return dcSoftwareCalidad.CAL_GetTestAnalisisMicrobiologia(IdAnalisisPeriodicoMicrobiologia).ToList();
        }

        private List<CAL_GetTestAnalisisNutricionalResult> GetTestAnalisisNutricional(int IdAnalisisPeriodicoNutricional)
        {
            return dcSoftwareCalidad.CAL_GetTestAnalisisNutricional(IdAnalisisPeriodicoNutricional).ToList();
        }

        private void RepTemp(ref string template, string key, string value)
        {
            template = template.Replace("***" + key + "***", value);
        }

        #endregion
    }
}