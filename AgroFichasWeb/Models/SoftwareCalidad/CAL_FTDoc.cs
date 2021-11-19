using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class CAL_FTDoc
    {
        #region 1. DataContexts

        #endregion
        #region 2. Propiedades
        public bool EditarArchivo { get; set; }
        public HttpPostedFileBase PostedFile { get; set; }
        #endregion
        #region 3. Funciones
        public string GetArchivo()
        {
            if (this.NombreArchivo.HasImageExtension())
            {
                return string.Format("<a href=\"{0}\"><img src=\"{0}\" alt=\"{1}\" width=\"{2}\"></a>", string.Format("{0}/uploads/softwarecalidad/{1}", Util.HttpBasePath(), this.NombreArchivo), "", 350);
            }
            else
            {
                return string.Format("<a href=\"{0}\">{1}</a>", string.Format("{0}/CALFTDoc/Descargar/{1}", Util.HttpBasePath(), this.IdDoc), "Descargar Archivo");
            }
        }

        public string GetArchivoMini()
        {
            if (this.NombreArchivo.HasImageExtension())
            {
                return string.Format("<a href=\"{0}\"><img src=\"{0}\" alt=\"{1}\" width=\"{2}\"></a>", string.Format("{0}/uploads/softwarecalidad/{1}", Util.HttpBasePath(), this.NombreArchivo), "", 200);
            }
            else
            {
                return string.Format("<a href=\"{0}\">{1}</a>", string.Format("{0}/CALFTDoc/Descargar/{1}", Util.HttpBasePath(), this.IdDoc), "Descargar Archivo");
            }
        }
        #endregion
        #region 4. SelectLists

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

            #region Crear

            if (actionName == "Crear" && this.PostedFile == null)
                yield return new RuleViolation("Por favor seleccione el archivo", "PostedFile");

            #endregion

            #region Crear

            if (actionName == "Editar" && this.EditarArchivo && this.PostedFile == null)
                yield return new RuleViolation("Por favor seleccione el archivo a editar, por mientras se mantendrá el original", "PostedFile");

            #endregion

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
        #endregion
    }
}