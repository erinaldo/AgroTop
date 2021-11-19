using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class Cliente
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public void SetDefaults()
        {
            if (this.IDOleotop == null)
                this.IDOleotop = "";
            if (this.IDAvenatop == null)
                this.IDAvenatop = "";
            if (this.IDGranotop == null)
                this.IDGranotop = "";
            if (this.IDSaprosem == null)
                this.IDSaprosem = "";
            if (this.IDICI == null)
                this.IDICI = "";
        }

        public bool ClienteValido { get; set; }

        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public List<ClienteEmpresa> GetEmpresas(int IdCliente)
        {
            List<ClienteEmpresa> list = new List<Models.ClienteEmpresa>();

            var empresas = dc.GetEmpresasPorIdCliente(IdCliente);

            foreach (var empresa in empresas)
            {
                list.Add(new Models.ClienteEmpresa()
                {
                    IdCliente = IdCliente,
                    IdEmpresa = empresa.IdEmpresa,
                    Empresa = dc.Empresa.Single(X => X.IdEmpresa == empresa.IdEmpresa),
                    Tiene = empresa.Tiene.Value
                });
            }

            return list;
        }

        public IEnumerable<SelectListItem> GetPaises(string PaisCodigo)
        {
            IEnumerable<SelectListItem> selectList = from X in dc.Pais
                                                     orderby X.PaisNombre
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.PaisCodigo == PaisCodigo && PaisCodigo != null),
                                                         Text = X.PaisNombre.ToString(),
                                                         Value = X.PaisCodigo
                                                     };
            return selectList;
        }

        public bool ValidacionEmpresas(ModelStateDictionary modelState, HttpContext httpContext)
        {
            string errMsg = "";
            if (string.IsNullOrEmpty(httpContext.Request["chkEmpresa"]))
            {
                errMsg = "Debe seleccionar al menos una empresa";
                modelState.AddModelError("ClienteValido", errMsg);
                return false;
            }
            else
            {
                return true;
            }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (string.IsNullOrEmpty(this.RazonSocial))
                yield return new RuleViolation("La razón social es requerida", "RazonSocial");

            //if (string.IsNullOrEmpty(this.Telefono))
            //    yield return new RuleViolation("El teléfono es requerido", "Telefono");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
    }
}