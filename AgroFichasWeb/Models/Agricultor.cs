using AgroFichasWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class Agricultor
    {
        #region 1. DataContexts
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        #endregion

        public bool IsNew { get; set; }
        public bool PasswordDefined { get; set; }
        public string SubmitedPassword { get; set; }

        public void SetDefaults()
        {
            if (this.Email == null)
                this.Email = "";
            if (this.Fono1 == null)
                this.Fono1 = "";
            if (this.Fono2 == null)
                this.Fono2 = "";
            if (this.IDOleotop == null)
                this.IDOleotop = "";
            if (this.IDAvenatop == null)
                this.IDAvenatop = "";
            if (this.IDGranotop == null)
                this.IDGranotop = "";
            if (this.IDSaprosem == null)
                this.IDSaprosem = "";

        }

        public List<AgricultorRelacionadoViewModel> RelacionadosFull(AgroFichasDBDataContext dc, bool includeSelf = false)
        {
            var relacionados = this.RelacionadosHijos(includeSelf);

            foreach (var hermano in this.RelacionadosHermanos(dc, includeSelf, true)) //incluye al padre
                if (relacionados.Where(r => r.IdAgricultor == hermano.IdAgricultor).Count() == 0)
                    relacionados.Add(hermano);

            if (includeSelf && relacionados.Where(r => r.IdAgricultor == this.IdAgricultor).Count() == 0)
            {
                relacionados.Add(new AgricultorRelacionadoViewModel()
                {
                    IdAgricultor = this.IdAgricultor,
                    Nombre = this.Nombre,
                    Rut = this.Rut
                });
            }
            return relacionados;
        }

        public List<AgricultorRelacionadoViewModel> RelacionadosHijos(bool includeSelf = false)
        {
            return RelacionadosHijos(new AgroFichasDBDataContext(), includeSelf);
        }

        public List<AgricultorRelacionadoViewModel> RelacionadosHijos(AgroFichasDBDataContext dc, bool includeSelf = false)
        {
            return (from rel in dc.AgricultoresRelacionadosHijos(this.IdAgricultor, includeSelf)
                    select new AgricultorRelacionadoViewModel()
                    {
                        IdAgricultor = rel.IdAgricultor,
                        Rut = rel.Rut,
                        Nombre = rel.Nombre
                    }).ToList();
        }

        public AgricultorRelacionadoViewModel RelacionadoPadre()
        {
            return RelacionadoPadre(new AgroFichasDBDataContext());
        }

        public AgricultorRelacionadoViewModel RelacionadoPadre(AgroFichasDBDataContext dc)
        {
            return (from rel in dc.AgricultorRelacionadoPadre(this.IdAgricultor)
                    select new AgricultorRelacionadoViewModel()
                    {
                        IdAgricultor = rel.IdAgricultor,
                        Rut = rel.Rut,
                        Nombre = rel.Nombre
                    }).FirstOrDefault();
        }

        public List<AgricultorRelacionadoViewModel> RelacionadosHermanos(AgroFichasDBDataContext dc, bool includeSelf = false, bool includePadre = false)
        {
            return (from rel in dc.AgricultoresRelacionadosHermanos(this.IdAgricultor, includeSelf, includePadre)
                    select new AgricultorRelacionadoViewModel()
                    {
                        IdAgricultor = rel.IdAgricultor,
                        Rut = rel.Rut,
                        Nombre = rel.Nombre
                    }).ToList(); 
        }

        public List<int> IdsRelacionadosHermanos(AgroFichasDBDataContext dc, bool includeSelf = false, bool includePadre = false)
        {
            return (from ar in this.RelacionadosHermanos(dc, includeSelf, includePadre) select ar.IdAgricultor).ToList();
        }

        public IEnumerable<SelectListItem> GetRegiones(int? IdRegion)
        {
            IEnumerable<SelectListItem> selectList = from X in dcAgroFichas.Region
                                                     orderby X.IdRegion
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdRegion == IdRegion && IdRegion != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdRegion.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetProvincia(int? IdProvincia)
        {
            IEnumerable<SelectListItem> selectList = from X in dcAgroFichas.Provincia
                                                     orderby X.IdRegion
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdProvincia == IdProvincia && IdProvincia != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdProvincia.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetComuna(int? IdComuna)
        {
            IEnumerable<SelectListItem> selectList = from X in dcAgroFichas.Comuna
                                                     orderby X.IdProvincia
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdComuna == IdComuna && IdComuna != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdComuna.ToString()
                                                     };
            return selectList;
        }
        public IEnumerable<SelectListItem> GetTituloExplotacion(int? IdTituloExplotacion)
        {
            IEnumerable<SelectListItem> selectList = from X in dcAgroFichas.TituloExplotacion
                                                     orderby X.IdTituloExplotacion
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdTituloExplotacion == IdTituloExplotacion && IdTituloExplotacion != null),
                                                         Text = X.Nombre,
                                                         Value = X.IdTituloExplotacion.ToString()
                                                     };
            return selectList;
        }
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (String.IsNullOrEmpty(this.Rut))
                yield return new RuleViolation("El Rut es requerido", "Rut");

            if (String.IsNullOrEmpty(this.Nombre))
                yield return new RuleViolation("El Nombre es requerido", "Nombre");

            if (!String.IsNullOrEmpty(this.Rut))
            {
                var dc = new AgroFichasDBDataContext();
                var rut = AgroFichasWeb.Models.Rut.NomarlizarRut(this.Rut);
                var existente = dc.Agricultor.SingleOrDefault(a => a.Rut == rut && a.IdAgricultor != this.IdAgricultor);
                if (existente != null)
                    yield return new RuleViolation(String.Format("Ya existe un agricultor con el Rut {0} ({2} - ID:{1})", rut, existente.IdAgricultor, existente.Nombre ), "Rut");
            }
            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            
            if (!IsValid)
            {
                var msg = "";
                foreach (var rv in GetRuleViolations())
                    msg += rv.ErrorMessage + " \n";

                throw new ApplicationException("Rule violations prevent saving: " + msg);
            }
        }

        public bool PermitirRecepcion(int idTemporada)
        {
            return this.Contrato.Where(c => c.Habilitado && c.IdTemporada == idTemporada).Count() > 0;
        }

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

        public static string HashPassword(string password)
        {
            if (password == "")
                return "";

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] data = md5.ComputeHash(Encoding.Default.GetBytes("ask(sdMARCO)anbuw&((#=)" + password));

            var sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
                sb.Append(data[i].ToString("x2"));

            return sb.ToString();
        }

        public List<IngresoValorizado> IngresosValorizados(AgroFichasDBDataContext dc, int idTemporada)
        {
            var result = new List<IngresoValorizado>();

            var items = from pi in this.ProcesoIngreso
                        where pi.Nulo == 0
                           && pi.IdTemporada == idTemporada
                        select pi;

            foreach (var item in items)
            {
                bool usingSpot;
                decimal? valor = item.Valorizar(dc, out usingSpot);

                result.Add(new IngresoValorizado()
                {
                    ProcesoIngreso = item,
                    Valor = valor,
                    UsaSpot = usingSpot
                });
            }

            return result;
        }

        public class IngresoValorizado
        {
            public ProcesoIngreso ProcesoIngreso { get; set; }
            public decimal? Valor { get; set; }
            public bool UsaSpot { get; set; }
        }
    }

    


}