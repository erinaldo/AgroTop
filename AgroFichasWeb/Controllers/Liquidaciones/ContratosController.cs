using AgroFichasWeb.AppLayer;
using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.Models.Logistica.Utilidades;
using AgroFichasWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AgroFichasWeb.Controllers
{
    [WebsiteAuthorize]
    public class ContratosController : BaseApplicationController
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();
        string DOCTO_OFFICIAL_EXTENSION = ".pdf";

        public ContratosController()
        {
            SetCurrentModulo(4); //Liquidaciones
        }

        [HttpPost]
        public JsonResult ContratosFinderSearch(string keyword, int type, int idContratoBaseArgument)
        {
            var result = new List<Object>();
            var contratoBase = dc.Contrato.SingleOrDefault(c => c.IdContrato == idContratoBaseArgument);

            ////Los contratos candidatos deben coincidir en agricultor-temporara-cultivo(base)
            //if (contratoBase != null)
            //{
            //    var contratos = from co in dc.Contrato
            //                    join ag in dc.Agricultor on co.IdAgricultor equals ag.IdAgricultor
            //                    where co.IdTemporada == contratoBase.IdTemporada
            //                       && ag.IdAgricultor == contratoBase.IdAgricultor
            //                       && co.CultivoContrato.IdCultivo == contratoBase.CultivoContrato.IdCultivo
            //                       && co.IdContrato != contratoBase.IdContrato
            //                       && co.Habilitado
            //                       && ag.Habilitado
            //                       && (ag.Nombre.Contains(keyword) || ag.Rut.Contains(keyword) || co.NumeroContrato.Contains(keyword))
            //                    select co;
            //    foreach (var contrato in contratos)
            //    {
            //        result.Add(new
            //        {
            //            IdContrato = contrato.IdContrato,
            //            NumeroContrato = contrato.NumeroContrato,
            //            Nombre = contrato.Agricultor.Nombre,
            //            NombreCultivo = contrato.CultivoContrato.Nombre,
            //            NombreEmpresa = contrato.Empresa.Nombre
            //        });
            //    }
            //}

            return Json(result);
        }

        [HttpPost]
        public JsonResult ContratosSelectorSearch(string keyword, int idTemporada)
        {
            var result = new List<Object>();

            var contratos = from co in dc.Contrato
                            join ag in dc.Agricultor on co.IdAgricultor equals ag.IdAgricultor
                            where co.IdTemporada == idTemporada
                               && co.Habilitado
                               && ag.Habilitado
                               && (ag.Nombre.Contains(keyword) || ag.Rut.Contains(keyword) || co.NumeroContrato.Contains(keyword))
                            select co;

            foreach (var contrato in contratos)
            {
                result.Add(new
                {
                    IdContrato = contrato.IdContrato,
                    NumeroContrato = contrato.NumeroContrato,
                    Nombre = contrato.Agricultor.Nombre,
                    NombreCultivo = contrato.DescripcionCultivos("<br/>"),
                    NombreEmpresa = contrato.Empresa.Nombre,
                    IdTemporada = contrato.IdTemporada,
                    NombreTemporada = contrato.Temporada.Nombre
                });
            }

            return Json(result);
        }

        public ActionResult Crear(int? idTemporada)
        {
            CheckPermisoAndRedirect(12);

            List<Temporada> temporadas = ResolveTemporadas(dc, null, out Temporada temporada);

            var contrato = new Contrato
            {
                Habilitado = true,
                IdTemporada = temporada.IdTemporada,
                IdComuna = 999999,
                IdSucursal = null
            };

            ViewData["items"] = new List<ItemContratoViewModel>();
            ViewData["temporada"] = temporada;
            ViewData["temporadas"] = temporadas;
            ViewData["nextItemId"] = ItemContratoViewModel.NextId(dc);

            SetLookupLists();
            ViewData["tipoContratoList"] = SetTipoContrato(null);
            ViewData["indexRouteValues"] = IndexRouteValues(null);
            return View("Contrato", contrato);
        }

        [AcceptVerbs(HttpVerbs.Post), ValidateInput(false)]
        public ActionResult Crear(Contrato contrato, FormCollection formValues)
        {
            CheckPermisoAndRedirect(12);

            var items = new List<ItemContratoViewModel>();
            UpdateModel(items, "Items");
            ModelState.AddRuleViolations(contrato.ValidateItems(dc, items));

            if (ModelState.IsValid)
            {
                try
                {
                    if (contrato.Comentarios == null)
                        contrato.Comentarios = "";

                    contrato.UserIns = User.Identity.Name;
                    contrato.FechaHoraIns = DateTime.Now;
                    contrato.IpIns = RemoteAddr();

                    dc.Contrato.InsertOnSubmit(contrato);

                    //Items
                    //Nuevos
                    foreach (var item in items)
                    {
                        contrato.ItemContrato.Add(new ItemContrato()
                        {
                            IdCultivoContrato = item.IdCultivoContrato,
                            Cantidad = item.Cantidad,
                            Superficie = item.Superficie,
                            FechaHoraIns = DateTime.Now,
                            IpIns = RemoteAddr(),
                            UserIns = User.Identity.Name,
                        });
                    }

                    dc.SubmitChanges();
                    return RedirectToAction("Index", new { idTemporada = contrato.IdTemporada });
                }
                catch
                {
                    var rv = contrato.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }
            else
            {
                contrato.Agricultor = dc.Agricultor.SingleOrDefault(a => a.IdAgricultor == contrato.IdAgricultor);
            }

            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, contrato.IdTemporada, out temporada);

            ViewData["items"] = items;
            ViewData["temporada"] = temporada;
            ViewData["temporadas"] = temporadas;
            ViewData["nextItemId"] = int.Parse(formValues["nextItemId"]);

            SetLookupLists();
            ViewData["tipoContratoList"] = SetTipoContrato(contrato.IdTipoContrato);
            ViewData["indexRouteValues"] = IndexRouteValues(null);
            return View("Contrato", contrato);
        }

        public ActionResult CrearDocto(int? id)
        {
            CheckPermisoAndRedirect(1006);

            var contrato = dc.Contrato.SingleOrDefault(x => x.IdContrato == id);
            if (contrato == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Contrato Not Found"); }

            var model = new DoctoContrato();
            model.IdContrato = contrato.IdContrato;

            ViewData["contrato"] = contrato;
            ViewData["tipoDoctoList"] = LogisticaSelectList.SetTipoDoctoContrato(null);
            return View("CrearDocto", model);
        }

        [AcceptVerbs(HttpVerbs.Post), ValidateInput(false)]
        public ActionResult CrearDocto(DoctoContrato doctoContrato, FormCollection formValues)
        {
            CheckPermisoAndRedirect(1006);

            if (ModelState.IsValid)
            {
                var contrato = dc.Contrato.SingleOrDefault(x => x.IdContrato == doctoContrato.IdContrato);
                if (contrato == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Contrato Not Found"); }

                doctoContrato.DoctoValido = true;
                doctoContrato.UserIns = User.Identity.Name;
                doctoContrato.FechaHoraIns = DateTime.Now;
                doctoContrato.IpIns = RemoteAddr();

                if (doctoContrato.IdTipoDoctoContrato == 1)
                    doctoContrato.Correlativo = 0;

                try
                {
                    var file = Request.Files["docto"];
                    if (file.ContentLength == 0)
                    {
                        return RedirectToAction("creardocto", new { id = doctoContrato.IdContrato, msgerr = "Debes adjuntar un docto con extensión pdf." });
                    }

                    var doctoExistente = dc.DoctoContrato.SingleOrDefault(x => x.IdContrato == contrato.IdContrato && x.IdTipoDoctoContrato == doctoContrato.IdTipoDoctoContrato && x.Correlativo == doctoContrato.Correlativo && x.DoctoValido == true);
                    if (doctoExistente != null)
                    {
                        return RedirectToAction("creardocto", new { id = doctoContrato.IdContrato, msgerr = "Ya has subido este docto anteriormente por favor comprobar tipo de docto y correlativo." });
                    }

                    var extension = Path.GetExtension(file.FileName);
                    if (extension != ".pdf" && extension != ".PDF")
                    {
                        return RedirectToAction("creardocto", new { id = doctoContrato.IdContrato, msgerr = "El archivo debe ser un docto con extensión pdf." });
                    }

                    var filename = "";
                    var path = "";
                    //Validación extra para que no guarde el archivo
                    if (!(doctoContrato.IdTipoDoctoContrato == 2 && doctoContrato.Correlativo == 0))
                    {
                        switch (doctoContrato.IdTipoDoctoContrato)
                        {
                            case 1:
                                filename = RemoveForbiddenCharacters(contrato.NumeroContrato);
                                break;
                            case 2:
                                filename = string.Format("{0}-Anexo-{1}", RemoveForbiddenCharacters(contrato.NumeroContrato), doctoContrato.Correlativo);
                                break;
                        }
                        path = String.Format("~/App_Data/pdfs/liquidaciones/contratos/{0}", RemoveForbiddenCharacters(contrato.NumeroContrato));
                        if (Directory.Exists(Server.MapPath(path)) == false)
                            Directory.CreateDirectory(Server.MapPath(path));
                        var filepath = Path.Combine(Server.MapPath(path), filename) + extension;
                        file.SaveAs(filepath);
                    }

                    doctoContrato.RutaArchivo = path + "/" + filename + extension;

                    dc.DoctoContrato.InsertOnSubmit(doctoContrato);
                    dc.SubmitChanges();
                    return RedirectToAction("doctos", new { id = doctoContrato.IdContrato });
                }
                catch
                {
                    var rv = doctoContrato.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            ViewData["contrato"] = dc.Contrato.SingleOrDefault(x => x.IdContrato == doctoContrato.IdContrato);
            ViewData["tipoDoctoList"] = LogisticaSelectList.SetTipoDoctoContrato(doctoContrato.IdTipoDoctoContrato);
            return View("CrearDocto", doctoContrato);
        }

        public ActionResult DescargarContrato(int id)
        {
            var doctoContrato = dc.DoctoContrato.SingleOrDefault(x => x.IdDoctoContrato == id && x.DoctoValido == true);
            if (doctoContrato == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Documento Not Found"); }

            string filename = GetFileName(id) + this.DOCTO_OFFICIAL_EXTENSION;
            string filepath = Server.MapPath(doctoContrato.RutaArchivo);
            byte[] filedata = System.IO.File.ReadAllBytes(filepath);
            string contentType = MimeMapping.GetMimeMapping(filepath);

            var contentDisposition = new System.Net.Mime.ContentDisposition
            {
                FileName = filename,
                Inline = true,
            };

            Response.AppendHeader("Content-Disposition", contentDisposition.ToString());

            return File(filedata, contentType);
        }

        public ActionResult Doctos(int? id)
        {
            CheckPermisoAndRedirect(1006);

            var contrato = dc.Contrato.SingleOrDefault(x => x.IdContrato == id);
            if (contrato == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Contrato Not Found"); }

            //Items
            var items = from c in dc.DoctoContrato
                        where c.IdContrato == contrato.IdContrato
                           && c.DoctoValido == true
                        orderby c.IdTipoDoctoContrato ascending
                        orderby c.Correlativo ascending
                        select c;

            var model = new PaginatedList<DoctoContrato>(items, 0, this.DefaultPageSize);

            ViewData["contrato"] = contrato;
            return View(model);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(13);

            var contrato = dc.Contrato.SingleOrDefault(p => p.IdContrato == id);
            if (contrato == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Contrato Not Found");

            ViewData["items"] = ItemContratoViewModel.FromDB(dc, contrato);
            ViewData["nextItemId"] = ItemContratoViewModel.NextId(dc);

            SetLookupLists();
            ViewData["tipoContratoList"] = SetTipoContrato(contrato.IdTipoContrato);
            ViewData["indexRouteValues"] = IndexRouteValues(null);
            return View("Contrato", contrato);
        }

        [AcceptVerbs(HttpVerbs.Post), ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(13);

            var contrato = dc.Contrato.SingleOrDefault(p => p.IdContrato == id);
            var fields = new string[] { "NumeroContrato", "IdAgricultor", "IdCultivoContrato", "IdEmpresa", "Cantidad", "Habilitado", "Comentarios", "IdTipoContrato", "IdComuna", "IdSucursal" };
            var items = new List<ItemContratoViewModel>();

            if (TryUpdateModel(contrato, fields))
            {
                UpdateModel(items, "Items");
                ModelState.AddRuleViolations(contrato.ValidateItems(dc, items));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (contrato.Comentarios == null)
                        contrato.Comentarios = "";

                    contrato.UserUpd = User.Identity.Name;
                    contrato.FechaHoraUpd = DateTime.Now;
                    contrato.IpUpd = RemoteAddr();

                    //Items
                    //Elimiandos
                    var eliminados = contrato.ItemContrato.Where(exs => !(from item in items select item.IdItemContrato).Contains(exs.IdItemContrato));
                    dc.ItemContrato.DeleteAllOnSubmit(eliminados);

                    //Nuevos y Editados
                    var existentes = contrato.ItemContrato;
                    foreach (var item in items)
                    {
                        var existente = existentes.SingleOrDefault(r => r.IdItemContrato == item.IdItemContrato);
                        if (existente == null)
                        {
                            contrato.ItemContrato.Add(new ItemContrato()
                            {
                                IdCultivoContrato = item.IdCultivoContrato,
                                Cantidad = item.Cantidad,
                                Superficie = item.Superficie,
                                FechaHoraIns = DateTime.Now,
                                IpIns = RemoteAddr(),
                                UserIns = User.Identity.Name,
                            });
                        }
                        else
                        {
                            if (existente.IdCultivoContrato != item.IdCultivoContrato ||
                                existente.Cantidad != item.Cantidad ||
                                existente.Superficie != item.Superficie)
                            {
                                existente.IdCultivoContrato = item.IdCultivoContrato;
                                existente.Cantidad = item.Cantidad;
                                existente.Superficie = item.Superficie;
                                existente.FechaHoraUpd = DateTime.Now;
                                existente.IpUpd = RemoteAddr();
                                existente.UserUpd = User.Identity.Name;
                            }
                        }
                    }

                    dc.SubmitChanges();
                    return RedirectToAction("Index", IndexRouteValues(null));
                }
                catch
                {
                    var rv = contrato.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }
            else
            {
                if (contrato.Agricultor == null)
                {
                    contrato.Agricultor = dc.Agricultor.SingleOrDefault(a => a.IdAgricultor == contrato.IdAgricultor);
                }
            }

            ViewData["items"] = items;
            ViewData["nextItemId"] = int.Parse(formValues["nextItemId"]);

            SetLookupLists();
            ViewData["tipoContratoList"] = SetTipoContrato(contrato.IdTipoContrato);
            ViewData["indexRouteValues"] = IndexRouteValues(null);

            return View("Contrato", contrato);
        }

        public ActionResult Eliminar(int id)
        {
            CheckPermisoAndRedirect(14);
            var contrato = dc.Contrato.SingleOrDefault(p => p.IdContrato == id);

            var routeValues = new RouteValueDictionary();
            try
            {
                if (contrato != null)
                {
                    dc.Contrato.DeleteOnSubmit(contrato);
                    dc.SubmitChanges();
                }
                routeValues.Add("msgok", "El contrato fue eliminado con éxito.");
            }
            catch (Exception ex)
            {
                string msgerr = "No es posible eliminar el contrato";
                if (ex.Message.Contains("FK_ConvenioPrecio_Contrato"))
                    msgerr += " porque tiene al menos un convenio de precio asociado";
                else if (ex.Message.Contains("FK_ConvenioCambioMoneda_Contrato"))
                    msgerr += " porque tiene al menos un convenio de cambio de moneda asociado";
                else
                    msgerr += ": " + ex.Message;

                routeValues.Add("msgerr", msgerr);
            }

            return RedirectToAction("Index", IndexRouteValues(routeValues));
        }

        public ActionResult EliminarDocto(int id)
        {
            CheckPermisoAndRedirect(1006);

            var doctoContrato = dc.DoctoContrato.SingleOrDefault(x => x.IdDoctoContrato == id && x.DoctoValido == true);
            if (doctoContrato == null) { throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Documento Not Found"); }

            doctoContrato.DoctoValido = false;
            doctoContrato.UserUpd = User.Identity.Name;
            doctoContrato.FechaHoraUpd = DateTime.Now;
            doctoContrato.IpUpd = RemoteAddr();

            var path = String.Format("~/App_Data/pdfs/liquidaciones/contratos/{0}", RemoveForbiddenCharacters(doctoContrato.Contrato.NumeroContrato));

            var filename = GetFileName(id);
            var filenameNulo = GetFileNameNulo(id);

            var sourceFileName = Path.Combine(Server.MapPath(path), filename) + this.DOCTO_OFFICIAL_EXTENSION;
            var destFileName = Path.Combine(Server.MapPath(path), filenameNulo) + this.DOCTO_OFFICIAL_EXTENSION;

            try
            {
                System.IO.File.Move(sourceFileName, destFileName);
            }
            catch
            {
                // Es un DoctoContrato de ForceManager CRM
                sourceFileName = Server.MapPath(doctoContrato.RutaArchivo);
                filenameNulo = string.Format("Nulo-{0}", System.Guid.NewGuid());
                destFileName = Path.Combine(Server.MapPath(path), filenameNulo) + this.DOCTO_OFFICIAL_EXTENSION;

                System.IO.File.Move(sourceFileName, destFileName);
            }

            doctoContrato.RutaArchivoNulo = path + "/" + filenameNulo + this.DOCTO_OFFICIAL_EXTENSION;

            dc.SubmitChanges();

            return RedirectToAction("doctos", new { id = doctoContrato.IdContrato });
        }

        public ActionResult Index(int? pageIndex, int? idTemporada, int? idEmpresa, string key = "")
        {
            CheckPermisoAndRedirect(11);

            //Temporadas
            Temporada temporada;
            List<Temporada> temporadas = ResolveTemporadas(dc, idTemporada, out temporada);

            //Items
            var idEmpresaSelect = idEmpresa ?? 0;
            var items = from c in dc.Contrato
                        where c.IdTemporada == temporada.IdTemporada
                           && (idEmpresaSelect == 0 || c.IdEmpresa == idEmpresaSelect)
                           && (key == "" ||
                               c.Agricultor.Nombre.Contains(key) ||
                               c.Empresa.Nombre.Contains(key) ||
                               c.NumeroContrato.Contains(key) ||
                               c.Sucursal.Nombre.Contains(key))
                        orderby c.NumeroContrato descending
                        select c;

            var model = new PaginatedList<Contrato>(items, -1, this.DefaultPageSize);
            model.BaseRouteValues = new RouteValueDictionary()
            {
                { "key", key },
                { "pageIndex", model.PageIndex },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" },
                { "idSucursal", Request.QueryString["idSucursal"] ?? "" }
            };

            //ViewData
            try
            {
                ViewData["totalToneladas"] = items.Sum(i => i.ItemContrato.Sum(p => p.Cantidad)) / 1000;
            }
            catch
            {
                ViewData["totalToneladas"] = 0;
            }
            ViewData["idEmpresa"] = idEmpresa ?? 0;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["cultivos"] = dc.CultivoContrato.ToArray();

            ViewData["temporadas"] = temporadas;
            ViewData["temporada"] = temporada;
            ViewData["key"] = key;

            return View(model);
        }

        public ActionResult IndexExport(int idTemporada, int? idEmpresa, string key = "")
        {
            CheckPermisoAndRedirect(11);

            var idEmpresaSelect = idEmpresa ?? 0;
            var items = from c in dc.Contrato
                        where c.IdTemporada == idTemporada
                           && (idEmpresaSelect == 0 || c.IdEmpresa == idEmpresaSelect)
                           && (key == "" ||
                               c.Agricultor.Nombre.Contains(key) ||
                               c.Empresa.Nombre.Contains(key) ||
                               c.NumeroContrato.Contains(key))
                        orderby c.NumeroContrato descending
                        select ContratosExportModel.FromContrato(c);

            return new CsvActionResult<ContratosExportModel>(items.ToList(), "Contratos.csv", 1, ';', null);
        }

        private string GetFileName(int id)
        {
            var doctoContrato = dc.DoctoContrato.Single(x => x.IdDoctoContrato == id);
            var contrato = dc.Contrato.Single(x => x.IdContrato == doctoContrato.IdContrato);
            string filename = "";
            switch (doctoContrato.IdTipoDoctoContrato)
            {
                case 1:
                    filename = RemoveForbiddenCharacters(contrato.NumeroContrato);
                    break;
                case 2:
                    filename = string.Format("{0}-Anexo-{1}", RemoveForbiddenCharacters(contrato.NumeroContrato), doctoContrato.Correlativo);
                    break;
            }

            return filename;
        }

        private string GetFileNameNulo(int id)
        {
            var doctoContrato = dc.DoctoContrato.Single(x => x.IdDoctoContrato == id);
            var contrato = dc.Contrato.Single(x => x.IdContrato == doctoContrato.IdContrato);

            var J = 1;
            var filename = "";
            var filepath = "";
            var possibleFileName = "";
            var path = String.Format("~/App_Data/pdfs/liquidaciones/contratos/{0}", RemoveForbiddenCharacters(contrato.NumeroContrato));

            switch (doctoContrato.IdTipoDoctoContrato)
            {
                case 1:
                    possibleFileName = string.Format("{0}-Nulo-{1}", RemoveForbiddenCharacters(contrato.NumeroContrato), J);
                    break;
                case 2:
                    possibleFileName = string.Format("{0}-Anexo-{1}-Nulo-{2}", RemoveForbiddenCharacters(contrato.NumeroContrato), doctoContrato.Correlativo, J);
                    break;
            }

            filepath = Path.Combine(Server.MapPath(path), possibleFileName) + this.DOCTO_OFFICIAL_EXTENSION;

            while (System.IO.File.Exists(filepath))
            {
                J++;

                switch (doctoContrato.IdTipoDoctoContrato)
                {
                    case 1:
                        possibleFileName = string.Format("{0}-Nulo-{1}", RemoveForbiddenCharacters(contrato.NumeroContrato), J);
                        break;
                    case 2:
                        possibleFileName = string.Format("{0}-Anexo-{1}-Nulo-{2}", RemoveForbiddenCharacters(contrato.NumeroContrato), doctoContrato.Correlativo, J);
                        break;
                }

                filepath = Path.Combine(Server.MapPath(path), possibleFileName) + this.DOCTO_OFFICIAL_EXTENSION;
            }

            // Nombre listo
            filename = possibleFileName;

            return filename;
        }

        private RouteValueDictionary IndexRouteValues(RouteValueDictionary routeValues)
        {
            var result = new RouteValueDictionary()
            {
                { "key", Request.QueryString["key"] ?? "" },
                { "pageIndex", Request.QueryString["pageIndex"] ?? "0" },
                { "idTemporada", Request.QueryString["idTemporada"] ?? "" },
                { "idEmpresa", Request.QueryString["idEmpresa"] ?? "" },
            };

            if (routeValues != null)
                foreach (var rv in routeValues)
                    result.Add(rv.Key, rv.Value);

            return result;
        }

        private string RemoveForbiddenCharacters(string str)
        {
            return str.Replace("<", "").Replace(">", "").Replace(":", "").Replace("\"", "").Replace("/", "").Replace("\\", "").Replace("|", "").Replace("?", "").Replace("*", "");
        }

        private void SetLookupLists()
        {
            ViewData["cultivos"] = dc.CultivoContrato.ToList();
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["comunas"] = dc.Comuna.ToList();
            ViewData["sucursales"] = dc.Sucursal.ToList();
        }

        private IEnumerable<SelectListItem> SetTipoContrato(int? IdTipoContrato)
        {
            IEnumerable<SelectListItem> selectList = from x in dc.TipoContrato
                                                     select new SelectListItem
                                                     {
                                                         Selected = (x.IdTipoContrato == IdTipoContrato && IdTipoContrato != null),
                                                         Text = x.Descripcion,
                                                         Value = x.IdTipoContrato.ToString()
                                                     };
            return selectList;
        }
    }
}
