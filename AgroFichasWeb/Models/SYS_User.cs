using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class SYS_User
    {
        private List<AdminMenuItem> modulos = null;
        private Dictionary<int, List<AdminMenuItem>> menus = null;
        private int idModuloActivo;
        private int idSubMenuActivo;

        public bool PasswordDefined { get; set; }
        public string SubmitedPassword { get; set; }
        public bool IsNew { get; set; }
        public bool NoNotificarCambioPassword { get; set; }

        public int IdModuloActivo
        {
            get
            {
                return idModuloActivo;
            }
            set
            {
                idModuloActivo = value;
                Modulos.Single(m => m.ID == idModuloActivo).Selected = true;
            }
        }

        public List<AdminMenuItem> Modulos
        {
            get
            {
                if (modulos == null)
                {
                    var dc = new AgroFichasDBDataContext();

                    modulos = (from modulo in dc.SYS_ModulosUsuario(this.UserName)
                               select new AdminMenuItem()
                               {
                                   ID = modulo.IdModulo,
                                   Nombre = modulo.Nombre,
                                   Url = "~/modulos/index/" + modulo.IdModulo,
                                   Selected = false,
                                   IdPermiso = 0
                               }).ToList();
                }

                return modulos;
            }
        }

        public List<AdminMenuItem> ItemsMenu(int idModulo)
        {
            if (menus == null)
                menus = new Dictionary<int, List<AdminMenuItem>>();

            if (!menus.ContainsKey(idModulo))
            {
                var dc = new AgroFichasDBDataContext();
                menus[idModulo] = (from menu in dc.SYS_MenusUsuario(this.UserName, idModulo)
                                   select new AdminMenuItem()
                                   {
                                       ID = menu.IdMenu,
                                       Nombre = menu.Nombre,
                                       Url = menu.Url,
                                       ContieneSubMenu = menu.ContieneSubmenu.Value,
                                       Selected = false,
                                       IdPermiso = 0
                                   }).ToList();
            }

            return menus[idModulo];
        }

        public List<AdminMenuItem> ItemsSubMenu(int idMenu)
        {
            if (menus == null)
                menus = new Dictionary<int, List<AdminMenuItem>>();

            if (!menus.ContainsKey(idMenu))
            {
                var dc = new AgroFichasDBDataContext();
                menus[idMenu] = (from submenu in dc.SYS_SubMenusUsuario(this.UserName, idMenu)
                                 select new AdminMenuItem()
                                 {
                                     ID = submenu.IdSubMenu,
                                     Nombre = submenu.Nombre,
                                     Url = submenu.Url,
                                     Selected = false,
                                     IdPermiso = 0
                                 }).ToList();
            }
            return menus[idMenu];
        }

        public List<AdminMenuItem> ItemsMenuActivo()
        {
            return ItemsMenu(IdModuloActivo);
        }

        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            var db = new UsuarioRepository();

            if (String.IsNullOrEmpty(this.UserName))
            {
                yield return new RuleViolation("El Nombre de Usuario es requerido", "UserName");
            }
            if (String.IsNullOrEmpty(this.FullName))
            {
                yield return new RuleViolation("El Nombre Completo es requerido", "FullName");
            }
            if (String.IsNullOrEmpty(this.Email))
            {
                yield return new RuleViolation("El Email es requerido", "Email");
            }
            else if (this.IsNew)
            {
                if (db.GetByUserName(this.UserName) != null)
                {
                    yield return new RuleViolation("El Nombre de Usuario ya existe", "UserName");
                }
            }

            if (PasswordDefined)
            {
                //TODO: 1 Validar password con regex
                if (String.IsNullOrEmpty(this.SubmitedPassword))
                {
                    yield return new RuleViolation("La Contraseña es requerida", "Password");
                }
                else if (this.SubmitedPassword.Length < 4)
                {
                    yield return new RuleViolation("La Contraseña debe tener al menos 4 caracteres", "Password");
                }
                else
                {
                    var existing = db.GetByPassword(this.SubmitedPassword);
                    if (existing != null && existing.UserID != this.UserID)
                    {
                        yield return new RuleViolation("Ya existe un usuario con esa contraseña. Las contraseñas deben ser únicas", "Password");
                    }
                }
            }
            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }


        public bool HasPermiso(int idPermiso)
        {
            return this.SYS_PermisoUsuario.SingleOrDefault(p => p.IdPermiso == idPermiso) != null;
        }


        public bool TieneAccesoSucursal(int idSucursal, string zoneToken)
        {
            return this.SucursalUsuario.SingleOrDefault(su => su.IdSucursal == idSucursal && su.ZoneToken == zoneToken) != null;
        }


        public bool TieneAccesoAAgricultor(int idAgricultor)
        {
            if (HasPermiso(5))
                return true;

            return this.UsuarioAgricultor.SingleOrDefault(ua => ua.IdAgricultor == idAgricultor) != null;
        }

        public bool NotificarContraseña(out string msg)
        {
            msg = "";

            if (this.Email == "nomail@empresasagrotop.cl")
            {
                msg = "El correo nomail@empresasagrotop.cl fue ignorado";
                return true;
            }

            try
            {
                var loginLink = string.Format("<a href=\"{0}\">{0}</a>", "http://sys.empresasagrotop.cl");

                var objMM = new MailMessage();
                objMM.From = new MailAddress("notificaciones@saprosem.cl", "Notificaciones Agrotop");
                objMM.To.Add(this.Email);
                //objMM.Bcc.Add("cdonoso@woc.cl");
                objMM.Subject = string.Format("Acceso a Sistema de Cosecha de Empresas Agrotop");
                objMM.IsBodyHtml = true;

                string Template = null;
                Template = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/newpassword_user_template.html"), Encoding.UTF7);
                RepTemp(ref Template, "NOMBRE", this.FullName);
                RepTemp(ref Template, "LOGINLINK", loginLink);
                RepTemp(ref Template, "PASSWORD", this.SubmitedPassword);
                RepTemp(ref Template, "USUARIO", this.UserName);

                objMM.Body = Template;

                var objSmtp = new SmtpClient();
                objSmtp.Send(objMM);

                msg = "Se ha notificado la contraseña a los siguientes correos: " + this.Email;
                return true;
            }
            catch (Exception ex)
            {
                msg = "Ocurrió un error al intentar notificar por correo: " + ex.Message;
                return false;
            }
        }

        private void RepTemp(ref string template, string key, string value)
        {
            template = template.Replace("***" + key + "***", value);
        }


        /* STATICS 
         * **********************************************************************************************/

        public static string HashPassword(string password)
        {
            if (password == "")
                return "";

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] data = md5.ComputeHash(Encoding.Default.GetBytes("pad#%$&i=(/WOX(9821cusi-.," + password));

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
                sb.Append(data[i].ToString("x2"));

            return sb.ToString();
        }

        private static SYS_User CreateFromUserName(string userName)
        {
            var dc = new UsuarioRepository();
            return dc.GetByUserName(userName);
        }

        public static SYS_User UserFromPin(string cleanPassword, int idPermiso)
        {
            var db = new UsuarioRepository();
            var user = db.GetByPassword(cleanPassword);
            if (user != null && !user.Disabled && user.HasPermiso(idPermiso))
                return user;

            return null;
        }

        public static SYS_User UserFromPin(string cleanPassword, int[] idsPermiso)
        {
            var db = new UsuarioRepository();
            var user = db.GetByPassword(cleanPassword);
            if (user != null && !user.Disabled)
                foreach (var idPermiso in idsPermiso)
                    if (user.HasPermiso(idPermiso))
                        return user;

            return null;
        }


        public static SYS_User Current()
        {
            IPrincipal user = HttpContext.Current.User;
            if (user == null || !user.Identity.IsAuthenticated)
                return null;

            if (HttpContext.Current.Items["CurrentUser"] == null)
                HttpContext.Current.Items["CurrentUser"] = CreateFromUserName(user.Identity.Name);

            return (SYS_User)HttpContext.Current.Items["CurrentUser"];
        }
    }
}