using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class Bodega
    {
        public string SAPID(int idEmpresa)
        {
            var sapid = "";

            if (idEmpresa == 1)
                sapid = this.IDOleotop;
            else if (idEmpresa == 2)
                sapid = this.IDAvenatop;
            else if (idEmpresa == 3)
                sapid = this.IDGranotop;
            else if (idEmpresa == 4)
                sapid = this.IDSaprosem;

            return sapid ?? "";
        }

        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (String.IsNullOrEmpty(this.Nombre))
                yield return new RuleViolation("El nombre es requerido", "Nombre");
            if (this.IdSucursal == 0)
                yield return new RuleViolation("La sucursal es requerida", "IdSucursal");
            if (String.IsNullOrEmpty(this.NombreCorto))
                yield return new RuleViolation("El nombre corto es requerido", "NombreCorto");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
    }
}