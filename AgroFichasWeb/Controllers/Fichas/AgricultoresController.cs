using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using AgroFichasWeb.AppLayer;

namespace AgroFichasWeb.Controllers
{
    [WebsiteAuthorize]
    public class AgricultoresController : BaseApplicationController
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public AgricultoresController()
        {
            SetCurrentModulo(1); //Fichas;
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(3);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            string key = Request.QueryString["key"] ?? "";

            var adminTodos = CurrentUser.HasPermiso(5);
            IQueryable<Agricultor> items;
            if (adminTodos)
            {
                items = dc.Agricultor.OrderBy(a => a.Nombre).Where(a => key == "" || a.Nombre.Contains(key) || a.Rut.Contains(key));
            }
            else
            {
                items = from ag in dc.Agricultor
                        join us in dc.UsuarioAgricultor on ag.IdAgricultor equals us.IdAgricultor
                        where ag.Habilitado
                           && us.UserID == CurrentUser.UserID
                           && (key == "" || ag.Nombre.Contains(key))
                        orderby ag.Nombre
                        select ag;
            }

            var pagina = new PaginatedList<Agricultor>(items, pageIndex, pageSize);

            ViewData["key"] = key;
            ViewData["adminTodos"] = adminTodos;
            return View(pagina);
        }

        public ActionResult IndexExport()
        {
            CheckPermisoAndRedirect(3);
            var items = dc.vw_Agricultor.ToList();

            return new CsvActionResult<vw_Agricultor>(items.ToList(), "Agricultores.csv", 1, ';', null);
        }

        public ActionResult SendPassword(int id)
        {
            CheckPermisoAndRedirect(3);
            var agricultor = dc.Agricultor.SingleOrDefault(p => p.IdAgricultor == id);

            if (agricultor == null || !CurrentUser.TieneAccesoAAgricultor(agricultor.IdAgricultor))
                throw new HttpException((int)HttpStatusCode.NotFound, "Agricultor Not Found");

            var newPassword = System.Web.Security.Membership.GeneratePassword(5, 0);

            dc.PasswordLog.InsertOnSubmit(new PasswordLog()
            {
                DateTimeIns = DateTime.Now,
                IpIns = RemoteAddr(),
                NewPassword = Agricultor.HashPassword(newPassword),
                OldPassword = agricultor.Password,
                UserIns = "admin:" + User.Identity.Name,
                UserName = agricultor.Rut
            });

            agricultor.Password = Agricultor.HashPassword(newPassword);
            agricultor.MustChangePassword = true;

            //Send
            string msg;
            string msgok = "";
            string msgerr = "";
            if (SendPasswordResetEmail(agricultor, newPassword, out msg))
            {
                dc.SubmitChanges();
                msgok = "El email con la nueva contraseña ha sido enviado con éxito";
            }
            else
            {
                msgerr = "Ocurrió un error al enviar la nueva contraseña: " + msg;
            }

            return RedirectToAction("Index", new { id = Request.QueryString["pageIndex"] ?? "0", key = Request.QueryString["key"] ?? "", msgok = msgok, msgerr=msgerr });
        }

        private bool SendPasswordResetEmail(Agricultor agricultor, string newPassword, out string msg)
        {
            msg = "";
            try
            {
                var loginLink = string.Format("<a href=\"{0}\">{0}</a>", "http://clientes.empresasagrotop.cl");

                MailMessage objMM = new MailMessage();

                objMM.From = new MailAddress("notificaciones@saprosem.cl", "Notificaciones Agrotop");
                objMM.To.Add(agricultor.Email);
                //objMM.Bcc.Add("cdonoso@woc.cl");
                objMM.Subject = string.Format("Acceso a zona clientes de Empresas Agrotop");
                objMM.IsBodyHtml = true;

                string Template = null;
                Template = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/newpassword_template.html"), Encoding.UTF7);
                RepTemp(ref Template, "NOMBRE", agricultor.Nombre);
                RepTemp(ref Template, "LOGINLINK", loginLink);
                RepTemp(ref Template, "PASSWORD", newPassword);
                RepTemp(ref Template, "RUT", agricultor.Rut);

                objMM.Body = Template;

                var objSmtp = new SmtpClient();
                objSmtp.Send(objMM);

                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }


        private static void RepTemp(ref string template, string key, string value)
        {
            template = template.Replace("***" + key + "***", value);
        }

        public ActionResult Fichas(int id)
        {
            CheckPermisoAndRedirect(new int[] { 3, 10 });

            var agricultor = dc.Agricultor.SingleOrDefault(a => a.IdAgricultor == id);
            if (agricultor == null)
                throw new HttpException((int)HttpStatusCode.NotFound, "Agricultor Not Found");

            if (!CurrentUser.HasPermiso(10) && !CurrentUser.TieneAccesoAAgricultor(agricultor.IdAgricultor))
                throw new HttpException((int)HttpStatusCode.NotFound, "Agricultor Not Found");

            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            ViewData["temporadas"] = temporadas;

            return View("Fichas", agricultor);
        }

        public ActionResult Recientes(int? id)
        {
            CheckPermisoAndRedirect(3);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;

            var adminTodos = CurrentUser.HasPermiso(5);
            IQueryable<vw_FichasInbox> items;

            if (adminTodos)
            {
                items = dc.vw_FichasInbox.OrderByDescending(fi => fi.FechaHora);
            }
            else
            {
                items = from ag in dc.Agricultor 
                        join ib in dc.vw_FichasInbox on ag.IdAgricultor equals ib.IdAgricultor
                        join us in dc.UsuarioAgricultor on ag.IdAgricultor equals us.IdAgricultor
                        where ag.Habilitado
                           && us.UserID == CurrentUser.UserID
                        orderby ib.FechaHora
                        select ib;
            }

            var pagina = new PaginatedList<vw_FichasInbox>(items, pageIndex, pageSize);

            return View(pagina);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(3);
            var agricultor = new Agricultor
            {
                Habilitado = true,
                IdProveedor = 1,
                IsNew = true
            };

            ViewData["relacionados"] = new List<AgricultorRelacionadoViewModel>();
            ViewData["cuentas"] = new List<CuentaBancariaViewModel>();
            ViewData["nextCuentaId"] = CuentaBancariaViewModel.NextId(dc);

            SetLookupLists();
            SetProveedores(agricultor.IdProveedor);
            return View("Agricultor", agricultor);
        }

        [AcceptVerbs(HttpVerbs.Post), ValidateInput(false)]
        public ActionResult Crear(Agricultor agricultor, FormCollection formValues)
        {
            CheckPermisoAndRedirect(3);
            
            var relacionados = new List<AgricultorRelacionadoViewModel>();
            var cuentas = new List<CuentaBancariaViewModel>();
            UpdateModel(relacionados, "Relacionados");
            UpdateModel(cuentas, "Cuentas");

            if (ModelState.IsValid)
            {
                try
                {
                    agricultor.SetDefaults();

                    agricultor.SubmitedPassword = agricultor.Password;
                    agricultor.PasswordDefined = true;
                    agricultor.Password = (!String.IsNullOrEmpty(agricultor.Password)) ? Agricultor.HashPassword(agricultor.Password) : "Not Set";
                    agricultor.MustChangePassword = true;
                    agricultor.Rut = Rut.NomarlizarRut(agricultor.Rut);
                    agricultor.UserIns = User.Identity.Name;
                    agricultor.FechaHoraIns = DateTime.Now;
                    agricultor.IpIns = RemoteAddr();
                    agricultor.MobileTag = "";
                    agricultor.Origen = 0;
                    agricultor.IsNew = true;
                    dc.Agricultor.InsertOnSubmit(agricultor);

                    //Cuentas Bancarias
                    //Nuevas
                    foreach (var cuenta in cuentas)
                    {
                        agricultor.CuentaBancaria.Add(new CuentaBancaria()
                        {
                            IdTipoCuentaBancaria = cuenta.IdTipoCuentaBancaria,
                            IdBanco = cuenta.IdBanco,
                            NumeroCuenta = cuenta.NumeroCuenta,
                            Comentarios = cuenta.Comentarios ?? "",
                            FechaHoraIns = DateTime.Now,
                            IpIns = RemoteAddr(),
                            UserIns = User.Identity.Name
                        });
                    }
                    dc.SubmitChanges();

                    //damos automáticamente acceso al usuario que creó al agricultor
                    if (!CurrentUser.HasPermiso(5))
                    {
                        agricultor.UsuarioAgricultor.Add(new UsuarioAgricultor()
                        {
                            UserID = CurrentUser.UserID,
                            MobileTag = "",
                            UserIns = "api",
                            FechaHoraIns = DateTime.Now,
                            IpIns = "127.0.0.1"
                        });
                        dc.SubmitChanges();
                    }

                    //Relacionados
                    //Nuevos
                    foreach (var rel in relacionados)
                        dc.AddAgricultorRelacionado(agricultor.IdAgricultor, rel.IdAgricultor, User.Identity.Name, RemoteAddr());

                    return RedirectToAction("Index");
                }
                catch
                {
                    var rv = agricultor.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }
            ViewData["relacionados"] = relacionados;
            ViewData["cuentas"] = cuentas;
            ViewData["nextCuentaId"] = int.Parse(formValues["nextCuentaId"]);

            SetLookupLists();
            SetProveedores(agricultor.IdProveedor);
            return View("Agricultor", agricultor);
        }

        public ActionResult Editar(int id)
        {
            CheckPermisoAndRedirect(3);

            var agricultor = dc.Agricultor.SingleOrDefault(p => p.IdAgricultor == id);
            if (agricultor == null)
                throw new HttpException((int)System.Net.HttpStatusCode.NotFound, "Agricultor Not Found");

            if (!CurrentUser.TieneAccesoAAgricultor(agricultor.IdAgricultor))
                throw new HttpException((int)HttpStatusCode.NotFound, "Agricultor Not Found");

            ViewData["relacionados"] = AgricultorRelacionadoViewModel.FromDB(dc, agricultor);
            ViewData["cuentas"] = CuentaBancariaViewModel.FromDB(dc, agricultor);
            ViewData["nextCuentaId"] = CuentaBancariaViewModel.NextId(dc);

            SetLookupLists();
            SetProveedores(agricultor.IdProveedor);
            return View("Agricultor", agricultor);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Editar(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(3);

            var agricultor = dc.Agricultor.SingleOrDefault(p => p.IdAgricultor == id);
            var relacionados = new List<AgricultorRelacionadoViewModel>();
            var cuentas = new List<CuentaBancariaViewModel>();

            if (!CurrentUser.TieneAccesoAAgricultor(agricultor.IdAgricultor))
                throw new HttpException((int)HttpStatusCode.NotFound, "Agricultor Not Found");

            agricultor.PasswordDefined = formValues["SubmitedPassword"] != "";
            agricultor.IsNew = false;

            try
            {
                var fields = new List<String>() { "Rut", "Nombre", "NombreRepresentate", "RutRepresentate", "IdRegion", "IdProvincia", "IdComuna", "Direccion", "DireccionPredio", "RolAvaluo", "InscripcionFS", "InscripcionNum", "InscripcionAno", "CoberturaSeguro", "IdTituloExplotacion", "Email", "Fono1", "Fono2", "IdProveedor", "Habilitado", "IDOleotop", "IDAvenatop", "IDGranotop", "IDSaprosem", "IdForceManager" };
                if (agricultor.PasswordDefined)
                    fields.Add("SubmitedPassword");

                TryUpdateModel(agricultor, fields.ToArray());
                TryUpdateModel(relacionados, "Relacionados");
                TryUpdateModel(cuentas, "Cuentas");

                if (agricultor.PasswordDefined)
                {
                    dc.PasswordLog.InsertOnSubmit(new PasswordLog()
                    {
                        DateTimeIns = DateTime.Now,
                        IpIns = RemoteAddr(),
                        NewPassword = Agricultor.HashPassword(agricultor.SubmitedPassword),
                        OldPassword = agricultor.Password,
                        UserIns = "admin:" + User.Identity.Name,
                        UserName = agricultor.Rut
                    });
                    agricultor.Password = Agricultor.HashPassword(agricultor.SubmitedPassword);
                    agricultor.MustChangePassword = true;
                }

                agricultor.SetDefaults();

                agricultor.Rut = Rut.NomarlizarRut(agricultor.Rut);
                agricultor.UserUpd = User.Identity.Name;
                agricultor.FechaHoraUpd = DateTime.Now;
                agricultor.IpUpd = RemoteAddr();

                //Cuentas
                //Eliminadas
                var eliminadas = agricultor.CuentaBancaria.Where(cb => !(from cta in cuentas select cta.IdCuentaBancaria).Contains(cb.IdCuentaBancaria));
                dc.CuentaBancaria.DeleteAllOnSubmit(eliminadas);

                //Nuevas y Editadas
                var existentes = agricultor.CuentaBancaria;
                foreach (var cta in cuentas)
                {
                    var existente = existentes.SingleOrDefault(r => r.IdCuentaBancaria == cta.IdCuentaBancaria);
                    if (existente == null)
                    {
                        agricultor.CuentaBancaria.Add(new CuentaBancaria()
                        {
                            IdTipoCuentaBancaria = cta.IdTipoCuentaBancaria,
                            IdBanco = cta.IdBanco,
                            NumeroCuenta = cta.NumeroCuenta,
                            Comentarios = cta.Comentarios ?? "",
                            FechaHoraIns = DateTime.Now,
                            IpIns = RemoteAddr(),
                            UserIns = User.Identity.Name
                        });
                    }
                    else
                    {
                        if (existente.IdTipoCuentaBancaria != cta.IdTipoCuentaBancaria ||
                            existente.IdBanco != cta.IdBanco ||
                            existente.NumeroCuenta != cta.NumeroCuenta ||
                            existente.Comentarios != cta.Comentarios)
                        {
                            existente.IdTipoCuentaBancaria = cta.IdTipoCuentaBancaria;
                            existente.IdBanco = cta.IdBanco;
                            existente.NumeroCuenta = cta.NumeroCuenta;
                            existente.Comentarios = cta.Comentarios ?? "";
                            existente.FechaHoraUpd = DateTime.Now;
                            existente.IpUpd = RemoteAddr();
                            existente.UserUpd = User.Identity.Name;
                        }
                    }
                }


                dc.SubmitChanges();

                //Relacionados
                //Nuevos
                var hijos = agricultor.RelacionadosHijos(false);
                foreach (var rel in relacionados)
                    if (hijos.SingleOrDefault(r => r.IdAgricultor == rel.IdAgricultor) == null)
                        dc.AddAgricultorRelacionado(agricultor.IdAgricultor, rel.IdAgricultor, User.Identity.Name, RemoteAddr());

                //Eliminados
                var relEliminados = hijos.Where(r => !(from nuevo in relacionados select nuevo.IdAgricultor).Contains(r.IdAgricultor));
                foreach (var rel in relEliminados)
                    dc.RemoveAgricultorRelacionado(agricultor.IdAgricultor, rel.IdAgricultor);

                
                return RedirectToAction("Index", new { id = Request.QueryString["pageIndex"] ?? "0", key = Request.QueryString["key"] ?? "" });
            }
            catch
            {
                var rv = agricultor.GetRuleViolations();
                if (rv.Count() > 0)
                    ModelState.AddRuleViolations(rv);
                else
                    throw;
            }

            ViewData["relacionados"] = relacionados;
            SetProveedores(agricultor.IdProveedor);
            return View("Agricultor", agricultor);
        }

        private void SetLookupLists()
        {
            ViewData["bancos"] = dc.Banco.ToList();
            ViewData["tiposCuenta"] = dc.TipoCuentaBancaria.ToList();
        }

        private void SetProveedores(int idProveedor)
        {
            ViewData["proveedores"] = (from pr in dc.Proveedor
                                       where pr.Habilitado || pr.IdProveedor == idProveedor
                                       orderby pr.Nombre
                                       select new SelectListItem()
                                       {
                                           Text = pr.Nombre,
                                           Value = pr.IdProveedor.ToString(),
                                           Selected = false
                                       }).ToList();
        }


        public ActionResult Usuarios(int id)
        {
            CheckPermisoAndRedirect(3);
            var agricultor = dc.Agricultor.SingleOrDefault(p => p.IdAgricultor == id);
            var data = dc.SelectUsuariosAgricultor(id).ToList();

            ViewData["agricultor"] = agricultor;

            return View(data);
        }

        [HttpPost]
        public ActionResult Usuarios(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(3);
            var agricultor = dc.Agricultor.SingleOrDefault(p => p.IdAgricultor == id);

            string[] ids = { };

            if (formValues["ia"] != null && formValues["ia"] != "")
                ids = formValues["ia"].Split(',');

            //Para cada usuario que tenía el agricultor revisamos si sigue en la lista. Si no está en la lista nueva lo eliminamos
            foreach (var usuario in agricultor.UsuarioAgricultor)
                if (ids.SingleOrDefault(idx => idx == usuario.UserID.ToString()) == null)
                    dc.UsuarioAgricultor.DeleteOnSubmit(usuario);

            //Para cada usuario de la nueva lista, revisamos si ya lo tenía. Si no lo tenía lo agregamos
            foreach (string idx in ids)
            {
                if (agricultor.UsuarioAgricultor.SingleOrDefault(c => c.UserID == int.Parse(idx)) == null)
                {
                    var newUsuario = new UsuarioAgricultor()
                    {
                        UserID = int.Parse(idx),
                        MobileTag = "",
                        UserIns = User.Identity.Name,
                        FechaHoraIns = DateTime.Now,
                        IpIns = RemoteAddr()
                    };
                    agricultor.UsuarioAgricultor.Add(newUsuario);
                }
            }

            dc.SubmitChanges();
            return RedirectToAction("Index");
        }

        public ActionResult DeleteSiembra(int id, int idAgricultor)
        {
            CheckPermisoAndRedirect(9);

            var siembra = dc.Siembra.SingleOrDefault(item => item.IdSiembra == id);

            if (!CurrentUser.TieneAccesoAAgricultor(siembra.Predio.IdAgricultor))
                throw new HttpException((int)HttpStatusCode.NotFound, "Agricultor Not Found");

            if (siembra != null)
            {
                dc.SiembraPotrero.DeleteAllOnSubmit(siembra.SiembraPotrero);
                dc.Siembra.DeleteOnSubmit(siembra);
                dc.SubmitChanges();
            }

            return Fichas(idAgricultor);
        }

        public ActionResult DeleteFicha(int id, int idAgricultor)
        {
            CheckPermisoAndRedirect(9);

            var ficha = dc.Ficha.SingleOrDefault(item => item.IdFicha == id);

            if (!CurrentUser.TieneAccesoAAgricultor(ficha.Predio.IdAgricultor))
                throw new HttpException((int)HttpStatusCode.NotFound, "Agricultor Not Found");
            
            if (ficha != null)
            {
                dc.Ficha.DeleteOnSubmit(ficha);
                dc.SubmitChanges();
            }

            return Fichas(idAgricultor);
        }

        public ActionResult DeleteFichaPreSiembra(int id, int idAgricultor)
        {
            CheckPermisoAndRedirect(9);

            var ficha = dc.FichaPreSiembra.SingleOrDefault(item => item.IdFichaPreSiembra == id);

            if (!CurrentUser.TieneAccesoAAgricultor(ficha.Predio.IdAgricultor))
                throw new HttpException((int)HttpStatusCode.NotFound, "Agricultor Not Found");

            if (ficha != null)
            {
                dc.FichaPreSiembra.DeleteOnSubmit(ficha);
                dc.SubmitChanges();
            }

            return Fichas(idAgricultor);
        }

        public ActionResult DeleteRecomendacion(int id, int idAgricultor)
        {
            CheckPermisoAndRedirect(9);

            var rec = dc.Recomendacion.SingleOrDefault(item => item.IdRecomendacion == id);

            if (!CurrentUser.TieneAccesoAAgricultor(rec.Ficha.Predio.IdAgricultor))
                throw new HttpException((int)HttpStatusCode.NotFound, "Agricultor Not Found");

            if (rec != null)
            {
                dc.Recomendacion.DeleteOnSubmit(rec);
                dc.SubmitChanges();
            }

            return Fichas(idAgricultor);
        }

        public ActionResult DeleteRecomendacionPreSiembra(int id, int idAgricultor)
        {
            CheckPermisoAndRedirect(9);

            var rec = dc.RecomendacionPreSiembra.SingleOrDefault(item => item.IdRecomendacionPreSiembra == id);

            if (!CurrentUser.TieneAccesoAAgricultor(rec.FichaPreSiembra.Predio.IdAgricultor))
                throw new HttpException((int)HttpStatusCode.NotFound, "Agricultor Not Found");

            if (rec != null)
            {
                dc.RecomendacionPreSiembra.DeleteOnSubmit(rec);
                dc.SubmitChanges();
            }

            return Fichas(idAgricultor);
        }

        [HttpPost]
        public JsonResult AgregarCuentaBancaria(
            int idAgricultor,
            int idTipoCuenta,
            int idBanco,
            string numeroCuenta,
            string comentarios)
        {
            try
            {
                var cuenta = new CuentaBancaria()
                {
                    IdAgricultor = idAgricultor,
                    IdTipoCuentaBancaria = idTipoCuenta,
                    IdBanco = idBanco,
                    NumeroCuenta = numeroCuenta,
                    Comentarios = comentarios ?? "",
                    FechaHoraIns = DateTime.Now,
                    IpIns = RemoteAddr(),
                    UserIns = User.Identity.Name
                };

                dc.CuentaBancaria.InsertOnSubmit(cuenta);
                dc.SubmitChanges();

                return Json(new 
                {
                    OK = true,
                    IdCuenta = cuenta.IdCuentaBancaria,
                    Msg = ""
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    OK = false,
                    IdCuenta = 0,
                    Msg = ex.Message
                });
            }
        }
    }
}
