using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_FT
    {
        #region 1. DataContexts
        static SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        #endregion
        #region 2. Propiedades
        public int? IdControlFechado { get; set; }
        public int? IdSaco { get; set; }
        #endregion
        #region 3. Funciones
        public static List<CAL_FT> GetFichasTecnicas(int IdCliente, int IdProducto, int IdSubproducto)
        {
            return dcSoftwareCalidad.CAL_FT.Where(X => X.IdCliente == IdCliente && X.IdProducto == IdProducto && X.IdSubproducto == IdSubproducto && X.Habilitado == true).ToList();
        }

        public static bool ResolverFichaTecnica(CAL_DetalleOrdenProduccion detalleOrdenProduccion, out int IdFichaTecnica, out string redirectToAction, out string errMsg)
        {
            IdFichaTecnica = 0;
            redirectToAction = "";
            errMsg = "";

            List<CAL_FT> fts = GetFichasTecnicas(detalleOrdenProduccion.CAL_OrdenProduccion.IdCliente, detalleOrdenProduccion.IdProducto, detalleOrdenProduccion.IdSubproducto);
            if (fts.Count == 0)
            {
                // Si no hay ninguna arrojamos un error
                redirectToAction = "Index";
                errMsg = "No se ha encontrado la ficha técnica asociada a este cliente/familia de productos/producto";
                return false;
            }
            else if (fts.Count > 1)
            {
                // Si hay mas de una redireccionamos
                redirectToAction = "SeleccionarFichaTecnica";
                return true;
            }
            else
            {
                // Si hay solo una utilizamos la que está disponible
                IdFichaTecnica = fts.First().IdFichaTecnica;
                return true;
            }
        }

        public string Formatter(decimal ValidValue, string UM, string FormatString)
        {
            if (ValidValue != 0)
            {
                if (UM == "%")
                    return (ValidValue / 100m).ToString(FormatString);
                else
                    return (ValidValue.ToString(FormatString));
            }
            else
                return "(No tiene)";
        }

        public Cliente GetCliente(int IdCliente)
        {
            return dcAgroFichas.Cliente.Single(X => X.IdCliente == IdCliente);
        }

        public List<CAL_FTControlVersion> GetControlVersiones()
        {
            return dcSoftwareCalidad.CAL_FTControlVersion.Where(X => X.IdFichaTecnica == this.IdFichaTecnica).ToList();
        }

        public List<CAL_FT> GetDetalleFT()
        {
            return dcSoftwareCalidad.CAL_FT.Where(X => X.IdFichaTecnica == this.IdFichaTecnica).ToList();
        }

        public List<CAL_FTDoc> GetDocumentos()
        {
            return dcSoftwareCalidad.CAL_FTDoc.Where(X => X.IdFichaTecnica == this.IdFichaTecnica && X.Habilitado == true).ToList();
        }

        public List<CAL_FTFrecuenciaAnalisis> GetFrecuenciasAnalisis()
        {
            return (from O1 in dcSoftwareCalidad.CAL_FTFrecuenciaAnalisis
                    where O1.IdFichaTecnica == this.IdFichaTecnica
                    select O1).ToList();
        }

        public Pais GetPais(string PaisCodigo)
        {
            return dcAgroFichas.Pais.Single(X => X.PaisCodigo == PaisCodigo);
        }

        public List<CAL_GetParametroAnalisisAsociadosPorFichaTecnicaResult> GetParametroAnalisis()
        {
            return (from O1 in dcSoftwareCalidad.CAL_GetParametroAnalisisAsociadosPorFichaTecnica(this.IdFichaTecnica)
                    select O1).ToList();
        }

        public List<CAL_GetParametroMetalPesadoPorIdFichaTecnicaResult> GetParametroMetalesPesados()
        {
            return (from O1 in dcSoftwareCalidad.CAL_GetParametroMetalPesadoPorIdFichaTecnica(this.IdFichaTecnica)
                    select O1).OrderBy(X => X.Nombre).ToList();
        }

        public List<CAL_GetParametroMicotoxinaPorIdFichaTecnicaResult> GetParametroMicotoxinas()
        {
            return (from O1 in dcSoftwareCalidad.CAL_GetParametroMicotoxinaPorIdFichaTecnica(this.IdFichaTecnica)
                    select O1).OrderBy(X => X.Nombre).ToList();
        }

        public List<CAL_GetParametroMicrobiologiaPorIdFichaTecnicaResult> GetParametroMicrobiologia()
        {
            return (from O1 in dcSoftwareCalidad.CAL_GetParametroMicrobiologiaPorIdFichaTecnica(this.IdFichaTecnica)
                    select O1).OrderBy(X => X.Nombre).ToList();
        }

        public List<CAL_GetParametroNutricionalPorIdFichaTecnicaResult> GetParametroNutricionales()
        {
            return (from O1 in dcSoftwareCalidad.CAL_GetParametroNutricionalPorIdFichaTecnica(this.IdFichaTecnica)
                    select O1).OrderBy(X => X.Nombre).ToList();
        }

        public List<CAL_GetParametroPesticidaPorIdFichaTecnicaResult> GetParametroPesticidas()
        {
            return (from O1 in dcSoftwareCalidad.CAL_GetParametroPesticidaPorIdFichaTecnica(this.IdFichaTecnica)
                    select O1).OrderBy(X => X.Nombre).ToList();
        }

        public List<CAL_GetParametroAnalisisPorFichaTecnicaResult> GetParametrosAnalisis()
        {
            return (from O1 in dcSoftwareCalidad.CAL_GetParametroAnalisisPorFichaTecnica(this.IdFichaTecnica)
                    select O1).ToList();
        }

        public CAL_PesoTipoSaco GetPesoSaco(int IdTipoSaco)
        {
            return dcSoftwareCalidad.CAL_PesoTipoSaco.SingleOrDefault(X => X.IdTipoSaco == IdTipoSaco);
        }

        public CAL_Producto GetProducto(int IdProducto)
        {
            return dcSoftwareCalidad.CAL_Producto.SingleOrDefault(X => X.IdProducto == IdProducto);
        }
        public List<CAL_GetSacoPorIdFichaTecnicaResult> GetSaco()
        {
            return (from O1 in dcSoftwareCalidad.CAL_GetSacoPorIdFichaTecnica(this.IdFichaTecnica)
                    select O1).ToList();
        }

        public CAL_Subproducto GetSubproducto(int IdSubproducto)
        {
            return dcSoftwareCalidad.CAL_Subproducto.SingleOrDefault(X => X.IdSubproducto == IdSubproducto);
        }

        public int GetVersion()
        {
            try
            {
                return dcSoftwareCalidad.CAL_FTControlVersion.Where(X => X.IdFichaTecnica == this.IdFichaTecnica).Sum(X => X.Version);
            }
            catch { }
            return 0;
        }
        public int GetControlVersion()
        {
            try { return dcSoftwareCalidad.CAL_FTControlVersion.Where(X => X.IdFichaTecnica == this.IdFichaTecnica).OrderByDescending(x => x.IdControlVersion).FirstOrDefault().Version; }
            catch { }
            return 0;
        }
        public SYS_User GetUser(string UserIns)
        {
            return dcAgroFichas.SYS_User.SingleOrDefault(X => X.UserName == UserIns);
        }
        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetClientes(int? IdCliente)
        {
            IEnumerable<SelectListItem> selectList = from X in dcAgroFichas.Cliente
                                                     join Y in dcAgroFichas.ClienteEmpresa on X.IdCliente equals Y.IdCliente
                                                     where Y.IdEmpresa == 2
                                                     orderby X.RazonSocial
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdCliente == this.IdCliente && IdCliente != null),
                                                         Text = X.RazonSocial,
                                                         Value = X.IdCliente.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetHumedadRelativa(string Key)
        {
            Dictionary<string, string> list = new Dictionary<string, string>();
            list.Add("No Aplica", "No Aplica");
            list.Add("60", "60%");

            IEnumerable<SelectListItem> selectList = from X in list
                                                     orderby X.Value
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.Key == Key && !string.IsNullOrEmpty(Key)),
                                                         Text = X.Value,
                                                         Value = X.Key
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetPaises(string PaisCodigo)
        {
            IEnumerable<SelectListItem> selectList = from X in dcAgroFichas.Pais
                                                     orderby X.PaisNombre
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.PaisCodigo == PaisCodigo && !string.IsNullOrEmpty(PaisCodigo)),
                                                         Text = X.PaisNombre,
                                                         Value = X.PaisCodigo
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetProductos(int? IdProducto)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_Producto
                                                     where X.Habilitado == true
                                                     orderby X.Nombre
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdProducto == IdProducto && IdProducto != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdProducto.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetSacoControlFechados(int? IdControlFechado)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_FTSacoControlFechado
                                                     orderby X.IdControlFechado
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdControlFechado == IdControlFechado && IdControlFechado != null),
                                                         Text = X.Descripcion,
                                                         Value = X.IdControlFechado.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetSacos(int? IdSaco)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_Saco
                                                     where X.Habilitado == true
                                                     orderby X.Nombre
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdSaco == IdSaco && IdSaco != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdSaco.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetSubproductos(int? IdSubproducto)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_Subproducto
                                                     where X.Habilitado == true
                                                     orderby X.Nombre
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdSubproducto == IdSubproducto && IdSubproducto != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdSubproducto.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetTemperatura(int? IdTemperatura)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_FTTemperatura
                                                     orderby X.IdTemperatura
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdTemperatura == IdTemperatura && IdTemperatura != null),
                                                         Text = string.Format("{0} a {1}° C", X.MinValidValue, X.MaxValidValue),
                                                         Value = X.IdTemperatura.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetVidaUtil(int? Key)
        {
            Dictionary<int, string> list = new Dictionary<int, string>();
            list.Add(09, "09 meses");
            list.Add(12, "12 meses");
            list.Add(18, "18 meses");

            IEnumerable<SelectListItem> selectList = from X in list
                                                     orderby X.Value
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.Key == Key && Key != null),
                                                         Text = X.Value,
                                                         Value = X.Key.ToString()
                                                     };
            return selectList;
        }
        #endregion
        #region 5. Validaciones
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

            #region CrearPaso1

            if (actionName == "CrearPaso1" && this.IdProducto == 0)
                yield return new RuleViolation("La familia de productos es requerido", "IdProducto");

            if (actionName == "CrearPaso1" && this.IdSubproducto == 0)
                yield return new RuleViolation("El producto es requerido", "IdSubproducto");

            #endregion

            #region CrearPaso2

            if (actionName == "CrearPaso2" && string.IsNullOrEmpty(this.Codigo))
                yield return new RuleViolation("El código es requerido", "Codigo");

            if (actionName == "CrearPaso2" && this.IdCliente == 0)
                yield return new RuleViolation("El cliente es requerido", "IdCliente");

            if (actionName == "CrearPaso2" && string.IsNullOrEmpty(PaisCodigo))
                yield return new RuleViolation("El país es requerido", "PaisCodigo");

            if (actionName == "CrearPaso2" && this.PesoTotalPickingTest == 0)
                yield return new RuleViolation("El peso total del picking test es requerido", "PesoTotalPickingTest");

            if (actionName == "CrearPaso2" && this.VidaUtil == 0)
                yield return new RuleViolation("La vida útil es requerida", "VidaUtil");

            if (actionName == "CrearPaso2" && this.IdTemperatura == 0)
                yield return new RuleViolation("La temperatura es requerida", "IdTemperatura");

            if (actionName == "CrearPaso2" && string.IsNullOrEmpty(HumedadRelativa))
                yield return new RuleViolation("La humedad relativa es requerida", "HumedadRelativa");

            #endregion

            #region Editar

            if (actionName == "Editar" && string.IsNullOrEmpty(this.Codigo))
                yield return new RuleViolation("El código es requerido", "Codigo");

            if (actionName == "Editar" && this.IdCliente == 0)
                yield return new RuleViolation("El cliente es requerido", "IdCliente");

            if (actionName == "Editar" && string.IsNullOrEmpty(PaisCodigo))
                yield return new RuleViolation("El país es requerido", "PaisCodigo");

            if (actionName == "Editar" && this.PesoTotalPickingTest == 0)
                yield return new RuleViolation("El peso total del picking test es requerido", "PesoTotalPickingTest");

            if (actionName == "Editar" && this.VidaUtil == 0)
                yield return new RuleViolation("La vida útil es requerida", "VidaUtil");

            if (actionName == "Editar" && this.IdTemperatura == 0)
                yield return new RuleViolation("La temperatura es requerida", "IdTemperatura");

            if (actionName == "Editar" && string.IsNullOrEmpty(HumedadRelativa))
                yield return new RuleViolation("La humedad relativa es requerida", "HumedadRelativa");

            #endregion

            yield break;
        }

        public bool ValidaFichaTecnica(CAL_Pale pale, out string errMsg)
        {
            errMsg = "";

            CAL_DetalleOrdenProduccion detalleOrdenProduccion = pale.CAL_DetalleOrdenProduccion;
            CAL_OrdenProduccion ordenProduccion = detalleOrdenProduccion.CAL_OrdenProduccion;

            if (ordenProduccion.IdTipoOrdenProduccion == 1 && this.Granel == false) //Envasado
            {
                List<CAL_FTSaco> sacosList = dcSoftwareCalidad.CAL_FTSaco.Where(X => X.IdFichaTecnica == this.IdFichaTecnica).ToList();
                if (sacosList.Where(X => X.IdSaco == detalleOrdenProduccion.IdSaco).ToList().Count == 0)
                {
                    errMsg = "La ficha técnica no contiene asociado un saco/maxisaco que concuerde con el del pallet a analizar";
                    return false;
                }
                else if (sacosList.Where(X => X.Peso == detalleOrdenProduccion.CAL_PesoSaco.Peso).ToList().Count == 0)
                {
                    errMsg = "La ficha técnica no contiene asociado un saco/maxisaco con el peso requerido en la orden de producción";
                    return false;
                }
                else if (pale.IdControlFechado.HasValue && sacosList.Where(X => X.IdControlFechado == pale.IdControlFechado).ToList().Count == 0)
                {
                    errMsg = "El control de fechado del pallet no calza con el de la ficha técnica";
                    return false;
                }
            }
            else if (ordenProduccion.IdTipoOrdenProduccion == 2 && this.Granel) //Granel
            {
                return true;
            }
            else
            {
                errMsg = "La ficha técnica dice granel y la orden de producción envasado o viceversa";
                return false;
            }

            return true;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
        #endregion
        #region 6. Mailing
        public bool NotificarCreacion()
        {
            try
            {
                string destinatarios = "";
                var receptores = dcAgroFichas.ReceptoresNotificacionCreacionFichaTecnica().ToList();

                if (receptores.Count == 0)
                    throw new Exception("No hay destinatarios disponibles para notificar la alerta");

                foreach (var receptor in receptores)
                {
                    var email = receptor.Email;
                    destinatarios += (destinatarios != "" ? ", " : "") + email;
                }

                string baseTemplate = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/crearfichatecnica_template.html"), Encoding.UTF8);
                Util.RepTempAngularStyle(ref baseTemplate, "CODIGO", this.Codigo);
                Util.RepTempAngularStyle(ref baseTemplate, "DATETIME", string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Now));
                Util.RepTempAngularStyle(ref baseTemplate, "CONSIGNATARIO", this.GetCliente(this.IdCliente).RazonSocial);
                Util.RepTempAngularStyle(ref baseTemplate, "PAIS", this.GetPais(this.PaisCodigo).PaisNombreLocal);
                Util.RepTempAngularStyle(ref baseTemplate, "VERSION", string.Format("{0}", this.GetControlVersion()));
                Util.RepTempAngularStyle(ref baseTemplate, "PRODUCTO", this.CAL_Subproducto.Nombre); 
                Util.RepTempAngularStyle(ref baseTemplate, "USER", this.GetUser(this.UserIns).FullName);
                Util.RepTempAngularStyle(ref baseTemplate, "YEAR", DateTime.Now.Year.ToString());

                string subject = string.Format(string.Format("Se ha creado una nueva ficha técnica - Código {0}", this.Codigo));

                Util.SendMail(destinatarios, baseTemplate, subject);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool NotificarEdicion()
        {
            try
            {
                string destinatarios = "";
                var receptores = dcAgroFichas.ReceptoresNotificacionEdicionFichaTecnica().ToList();

                if (receptores.Count == 0)
                    throw new Exception("No hay destinatarios disponibles para notificar la alerta");

                foreach (var receptor in receptores)
                {
                    var email = receptor.Email;
                    destinatarios += (destinatarios != "" ? ", " : "") + email;
                }

                string baseTemplate = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/editarfichatecnica_template.html"), Encoding.UTF8);
                Util.RepTempAngularStyle(ref baseTemplate, "CODIGO", this.Codigo);
                Util.RepTempAngularStyle(ref baseTemplate, "DATETIME", string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Now));
                Util.RepTempAngularStyle(ref baseTemplate, "CONSIGNATARIO", this.GetCliente(this.IdCliente).RazonSocial);
                Util.RepTempAngularStyle(ref baseTemplate, "PAIS", this.GetPais(this.PaisCodigo).PaisNombreLocal);
                Util.RepTempAngularStyle(ref baseTemplate, "VERSION", string.Format("{0}", this.GetControlVersion()));
                Util.RepTempAngularStyle(ref baseTemplate, "PRODUCTO", this.CAL_Subproducto.Nombre);
                Util.RepTempAngularStyle(ref baseTemplate, "USER", this.GetUser(this.UserIns).FullName);
                Util.RepTempAngularStyle(ref baseTemplate, "USERUPDATE", this.GetUser(this.UserUpd).FullName);
                Util.RepTempAngularStyle(ref baseTemplate, "YEAR", DateTime.Now.Year.ToString());

                string subject = string.Format(string.Format("Se ha editado una ficha técnica - Código {0}", this.Codigo));

                Util.SendMail(destinatarios, baseTemplate, subject);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool NotificarEliminacion()
        {
            try
            {
                string destinatarios = "";
                var receptores = dcAgroFichas.ReceptoresNotificacionEliminacionFichaTecnica().ToList();

                if (receptores.Count == 0)
                    throw new Exception("No hay destinatarios disponibles para notificar la alerta");

                foreach (var receptor in receptores)
                {
                    var email = receptor.Email;
                    destinatarios += (destinatarios != "" ? ", " : "") + email;
                }

                string baseTemplate = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/eliminarfichatecnica_template.html"), Encoding.UTF8);
                Util.RepTempAngularStyle(ref baseTemplate, "CODIGO", this.Codigo);
                Util.RepTempAngularStyle(ref baseTemplate, "DATETIME", string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Now));
                Util.RepTempAngularStyle(ref baseTemplate, "CONSIGNATARIO", this.GetCliente(this.IdCliente).RazonSocial);
                Util.RepTempAngularStyle(ref baseTemplate, "PAIS", this.GetPais(this.PaisCodigo).PaisNombreLocal);
                Util.RepTempAngularStyle(ref baseTemplate, "VERSION", string.Format("{0}", this.GetControlVersion()));
                Util.RepTempAngularStyle(ref baseTemplate, "PRODUCTO", this.CAL_Subproducto.Nombre);
                Util.RepTempAngularStyle(ref baseTemplate, "USER", this.GetUser(this.UserIns).FullName);
                Util.RepTempAngularStyle(ref baseTemplate, "USERDELETE", this.GetUser(this.UserUpd).FullName);
                Util.RepTempAngularStyle(ref baseTemplate, "YEAR", DateTime.Now.Year.ToString());

                string subject = string.Format(string.Format("Se ha eliminado una ficha técnica - Código {0}", this.Codigo));

                Util.SendMail(destinatarios, baseTemplate, subject);

                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}