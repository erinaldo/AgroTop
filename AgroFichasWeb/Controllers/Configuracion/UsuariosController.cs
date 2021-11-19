using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Configuracion
{
    [WebsiteAuthorize]
    public class UsuariosController : BaseApplicationController
    {
        UsuarioRepository db = new UsuarioRepository();

        public UsuariosController()
        {
            SetCurrentModulo(2); //Configuración;
        }

        public ActionResult Index(int? id)
        {
            CheckPermisoAndRedirect(2);
            int pageSize = this.DefaultPageSize;
            int pageIndex = id ?? 1;
            if (pageIndex < 1)
                pageIndex = 1;
            var key = (Request["key"] ?? "");
            var usuarios = db.GetAdminList(key);
            var pagina = new PaginatedList<SYS_User>(usuarios, pageIndex, pageSize);

            return View(pagina);
        }

        private void LoadLookups(string userName, int userID, int? IdSeccion)
        {
            ViewData["modulos"] = db.db.SYS_Modulo.OrderBy(m => m.Orden).ToList();
            ViewData["permisos"] = db.db.SYS_PermisosUsuario(userName).ToList();

            var zonasSucursales = new Dictionary<string, List<SYS_SucursalesUsuarioResult>>();
            var plantas = new Dictionary<string, List<SYS_PlantasUsuarioResult>>();
            zonasSucursales.Add("Recepción", db.db.SYS_SucursalesUsuario(userID, "recepcion").ToList());
            plantas.Add("", db.db.SYS_PlantasUsuario(userID).ToList());
            ViewData["zonasSucursales"] = zonasSucursales;
            ViewData["plantas"] = plantas;

            SetSecciones(IdSeccion);
        }

        public ActionResult Crear()
        {
            CheckPermisoAndRedirect(2);
            SYS_User SYS_User = new SYS_User()
            {
                Disabled = false,
            };
            LoadLookups("", 0, null);
            return View(SYS_User);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Crear(SYS_User SYS_User, FormCollection formValues)
        {
            CheckPermisoAndRedirect(2);
            if (ModelState.IsValid)
            {
                try
                {
                    SYS_User.SubmitedPassword = SYS_User.Password;
                    SYS_User.PasswordDefined = true;
                    SYS_User.Password = SYS_User.HashPassword(SYS_User.Password);
                    SYS_User.Disabled = false;
                    SYS_User.IsNew = true;
                    SYS_User.FechaHoraIns = DateTime.Now;
                    SYS_User.UserIns = SYS_User.Current().UserName;
                    SYS_User.IpIns = Request.UserHostAddress;
                    db.Add(SYS_User);

                    //Permisos del usuario
                    string[] ids = { };
                    if (formValues["chkPermiso"] != null && formValues["chkPermiso"] != "")
                        ids = formValues["chkPermiso"].Split(',');

                    //Insertar los permisos seleccionados
                    foreach (string idx in ids)
                    {
                        var newPermiso = new SYS_PermisoUsuario()
                        {
                            UserName = SYS_User.UserName,
                            IdPermiso = int.Parse(idx),
                            UserIns = User.Identity.Name,
                            FechaHoraIns = DateTime.Now,
                            IpIns = RemoteAddr()
                        };
                        SYS_User.SYS_PermisoUsuario.Add(newPermiso);
                    }

                    //Sucursales del usuario
                    string[] sucPairs = { };
                    if (formValues["chkSucursal"] != null && formValues["chkSucursal"] != "")
                        sucPairs = formValues["chkSucursal"].Split(',');

                    //Insertamos las sucursales
                    foreach (var pair in sucPairs)
                    {
                        var pairArray = pair.Split('-');
                        SYS_User.SucursalUsuario.Add(new SucursalUsuario()
                        {
                            IdSucursal = int.Parse(pairArray[1]),
                            ZoneToken = pairArray[0],
                            UserIns = User.Identity.Name,
                            FechaHoraIns = DateTime.Now,
                            IpIns = RemoteAddr()
                        });
                    }

                    //Planta de usuario
                    string[] plaPairs = { };
                    if (formValues["chkPlanta"] != null && formValues["chkPlanta"] != "")
                        plaPairs = formValues["chkPlanta"].Split(',');

                    //Para cada planta de la nueva lista, revisamos si ya lo tenía. Si no lo tenía lo agregamos
                    foreach (var pair in plaPairs)
                    {
                        var pairArray = pair.Split('-');
                        SYS_User.PlantaUsuario.Add(new PlantaUsuario()
                        {
                            IdPlantaProduccion = int.Parse(pairArray[0]),
                            UserIns = User.Identity.Name,
                            FechaHoraIns = DateTime.Now,
                            IpIns = RemoteAddr()
                        });
                        
                    }

                    db.Save();

                    //Enviamos contraseña por correo
                    string msg = "";
                    string msgok = "";
                    string msgerr = "";
                    if (SYS_User.NotificarContraseña(out msg))
                        msgok = msg;
                    else
                        msgerr = msg;

                    return RedirectToAction("Index", new { msgok = msgok, msgerr = msgerr });
                }
                catch
                {
                    var rv = SYS_User.GetRuleViolations();
                    if (rv.Count() > 0)
                        ModelState.AddRuleViolations(rv);
                    else
                        throw;
                }
            }

            LoadLookups("", 0, SYS_User.IdSeccion);
            return View(SYS_User);
        }

        public ActionResult Editar(string id)
        {
            CheckPermisoAndRedirect(2);
            SYS_User SYS_User = db.GetByUserName(id);
            if (SYS_User == null)
            {
                return View("AdminNotFound");
            }
            else
            {
                LoadLookups(SYS_User.UserName, SYS_User.UserID, SYS_User.IdSeccion);
                return View(SYS_User);
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Editar(string id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(2);
            SYS_User SYS_User = db.GetByUserName(id);
            SYS_User.PasswordDefined = formValues["SubmitedPassword"] != "";
            SYS_User.IsNew = false;

            try
            {
                List<String> fields = new List<string>() { "NoNotificarCambioPassword", "FullName", "Email", "IdSeccion", "Telefono" };
                if (SYS_User.PasswordDefined)
                    fields.Add("SubmitedPassword");

                UpdateModel(SYS_User, fields.ToArray());

                if (SYS_User.PasswordDefined)
                    SYS_User.Password = SYS_User.HashPassword(SYS_User.SubmitedPassword);

                SYS_User.FechaHoraUpd = DateTime.Now;
                SYS_User.UserUpd = SYS_User.Current().UserName;
                SYS_User.IpUpd = Request.UserHostAddress;

                //Permisos del usuario
                string[] ids = { };

                if (formValues["chkPermiso"] != null && formValues["chkPermiso"] != "")
                    ids = formValues["chkPermiso"].Split(',');

                //Para cada permiso que tenía el usuario revisamos si sigue en la lista. Si no está en la lista nueva lo eliminamos
                foreach (var permiso in SYS_User.SYS_PermisoUsuario)
                    if (ids.SingleOrDefault(idx => idx == permiso.IdPermiso.ToString()) == null)
                        db.db.SYS_PermisoUsuario.DeleteOnSubmit(permiso);

                //Para cada permiso de la nueva lista, revisamos si ya lo tenía. Si no lo tenía lo agregamos
                foreach (string idx in ids)
                {
                    if (SYS_User.SYS_PermisoUsuario.SingleOrDefault(c => c.IdPermiso == int.Parse(idx)) == null)
                    {
                        var newPermiso = new SYS_PermisoUsuario()
                        {
                            UserName = SYS_User.UserName,
                            IdPermiso = int.Parse(idx),
                            UserIns = User.Identity.Name,
                            FechaHoraIns = DateTime.Now,
                            IpIns = RemoteAddr()
                        };
                        SYS_User.SYS_PermisoUsuario.Add(newPermiso);
                    }
                }

                //Sucursales del usuario
                string[] sucPairs = { };
                if (formValues["chkSucursal"] != null && formValues["chkSucursal"] != "")
                    sucPairs = formValues["chkSucursal"].Split(',');

                var currentSucursales = SYS_User.SucursalUsuario.ToList();

                //Para cada sucursal que tenía el usuario revisamos si sigue en la lista. Si no está en la lista nueva lo eliminamos
                foreach (var suc in currentSucursales)
                    if (sucPairs.SingleOrDefault(sp => sp == suc.ZoneToken + "-" + suc.IdSucursal.ToString()) == null)
                        db.db.SucursalUsuario.DeleteOnSubmit(suc);

                //Para cada sucursal de la nueva lista, revisamos si ya lo tenía. Si no lo tenía lo agregamos
                foreach (var pair in sucPairs)
                {
                    if (currentSucursales.SingleOrDefault(cu => pair == cu.ZoneToken + "-" + cu.IdSucursal.ToString()) == null)
                    {
                        var pairArray = pair.Split('-');
                        SYS_User.SucursalUsuario.Add(new SucursalUsuario()
                        {
                            IdSucursal = int.Parse(pairArray[1]),
                            ZoneToken = pairArray[0],
                            UserIns = User.Identity.Name,
                            FechaHoraIns = DateTime.Now,
                            IpIns = RemoteAddr()
                        });
                    }
                }

                //db.Save();

                //Planta de usuario
                string[] plaPairs = { };
                if (formValues["chkPlanta"] != null && formValues["chkPlanta"] != "")
                    plaPairs = formValues["chkPlanta"].Split(',');

                var currentPlantas = SYS_User.PlantaUsuario.ToList();

                foreach (var pla in currentPlantas)
                    if (plaPairs.SingleOrDefault(sp => sp == pla.IdPlantaProduccion.ToString()) == null)
                        db.db.PlantaUsuario.DeleteOnSubmit(pla);

                //Para cada planta de la nueva lista, revisamos si ya lo tenía. Si no lo tenía lo agregamos
                foreach (var pair in plaPairs)
                {
                    if (currentPlantas.SingleOrDefault(cu => pair == cu.IdPlantaProduccion.ToString()) == null)
                    {
                        var pairArray = pair.Split('-');
                        SYS_User.PlantaUsuario.Add(new PlantaUsuario()
                        {
                            IdPlantaProduccion = int.Parse(pairArray[0]),
                            UserIns = User.Identity.Name,
                            FechaHoraIns = DateTime.Now,
                            IpIns = RemoteAddr()
                        });
                    }
                }

                db.Save();

                //Enviamos contraseña por correo
                if (SYS_User.PasswordDefined && !SYS_User.NoNotificarCambioPassword)
                {
                    string msg = "";
                    string msgok = "";
                    string msgerr = "";
                    if (SYS_User.NotificarContraseña(out msg))
                        msgok = msg;
                    else
                        msgerr = msg;

                    return RedirectToAction("Index", new { msgok = msgok, msgerr = msgerr });
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                ModelState.AddRuleViolations(SYS_User.GetRuleViolations());
                LoadLookups(SYS_User.UserName, SYS_User.UserID, SYS_User.IdSeccion);
                return View(SYS_User);
            }
        }

        public ActionResult Eliminar(string id)
        {
            CheckPermisoAndRedirect(2);
            SYS_User SYS_User = db.GetByUserName(id);
            if (SYS_User != null)
            {
                SYS_User.Disabled = true;
                SYS_User.FechaHoraUpd = DateTime.Now;
                SYS_User.UserUpd = SYS_User.Current().UserName;
                SYS_User.IpUpd = Request.UserHostAddress;
                //db.Delete(SYS_User);
                db.Save();
            }
            return RedirectToAction("Index");
        }

        public ActionResult Agricultores(int id)
        {
            CheckPermisoAndRedirect(2);

            var data = db.db.SelectAgricultoresUsuario(id).ToList();

            return View(data);
        }

        [HttpPost]
        public ActionResult Agricultores(int id, FormCollection formValues)
        {
            CheckPermisoAndRedirect(2);

            var user = db.db.SYS_User.Single(u => u.UserID == id);

            string[] ids = { };

            if (formValues["ia"] != null && formValues["ia"] != "")
                ids = formValues["ia"].Split(',');

            //Para cada agricultor que tenía el usuario revisamos si sigue en la lista. Si no está en la lista nueva lo eliminamos
            foreach (var agricultor in user.UsuarioAgricultor)
                if (ids.SingleOrDefault(idx => idx == agricultor.IdAgricultor.ToString()) == null)
                    db.db.UsuarioAgricultor.DeleteOnSubmit(agricultor);

            //Para cada agricultor de la nueva lista, revisamos si ya lo tenía. Si no lo tenía lo agregamos
            foreach (string idx in ids)
            {
                if (user.UsuarioAgricultor.SingleOrDefault(c => c.IdAgricultor == int.Parse(idx)) == null)
                {
                    var newAgricultor = new UsuarioAgricultor()
                    {
                        UserID = user.UserID,
                        IdAgricultor = int.Parse(idx),
                        MobileTag = "",
                        UserIns = User.Identity.Name,
                        FechaHoraIns = DateTime.Now,
                        IpIns = RemoteAddr()
                    };
                    user.UsuarioAgricultor.Add(newAgricultor);
                }
            }

            db.Save();
            return RedirectToAction("Index");
        }

        private void SetSecciones(int? IdSeccion)
        {
            IEnumerable<SelectListItem> selectList = from x in db.GetSecciones()
                                                     select new SelectListItem
                                                     {
                                                         Selected = (x.IdSeccion == IdSeccion && IdSeccion != null),
                                                         Text = x.Descripcion,
                                                         Value = x.IdSeccion.ToString()
                                                     };
            ViewData["seccionesList"] = selectList;
        }
    }
}
