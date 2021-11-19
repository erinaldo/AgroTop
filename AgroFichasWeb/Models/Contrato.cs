using AgroFichasWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class Contrato
    {
        public string Descripcion()
        {
            return $"{this.NumeroContrato} - {this.Empresa.Nombre} - {this.Agricultor.Nombre} - {this.Temporada.Nombre} - {this.Sucursal.Nombre}";
        }

        public string DescripcionCultivos(string delimiter)
        {
            if (this.ItemContrato == null || this.ItemContrato.Count == 0)
                return String.Empty;

            return this.ItemContrato.Select(i => i.CultivoContrato.Nombre).Aggregate((i, j) => i + delimiter + j);
        }

        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            var dc = new AgroFichasDBDataContext();

            if (this.IdAgricultor <= 0)
                yield return new RuleViolation("El Agricultor es requerido", "IdAgricultor");

            if (this.IdEmpresa <= 0)
                yield return new RuleViolation("La Empresa es requerida", "IdEmpresa");

            if (String.IsNullOrEmpty(this.NumeroContrato))
            {
                yield return new RuleViolation("El Número de Contrato es requerido", "NumeroContrato");
            }
            else
            {
                var contrato = dc.Contrato.SingleOrDefault(c => c.IdTemporada == this.IdTemporada && c.NumeroContrato == this.NumeroContrato && c.IdEmpresa == this.IdEmpresa && c.IdContrato != this.IdContrato && c.IdSucursal == this.IdSucursal);
                if (contrato != null)
                {
                    yield return new RuleViolation("El Número de Contrato ya existe", "NumeroContrato");
                }
            }
            yield break;
        }

        public IEnumerable<RuleViolation> ValidateItems(AgroFichasDBDataContext dc, List<ItemContratoViewModel> items)
        {
            var result = new List<RuleViolation>();

            //Debe existir al menos 1 item
            if (items.Count == 0)
                result.Add(new RuleViolation("El contrato debe tener al menos un cultivo", ""));

            //Sólo una vez cada cultivo
            var idsCultivosContrato = (from it in items select it.IdCultivoContrato).Distinct();
            foreach (var idCultivoContrato in idsCultivosContrato)
            {
                var cuenta = items.Where(it => it.IdCultivoContrato == idCultivoContrato).Count();
                if (cuenta > 1)
                    result.Add(new RuleViolation(String.Format("El cultivo {0} aparece más de una vez en el contrato", dc.CultivoContrato.Single(cc => cc.IdCultivoContrato == idCultivoContrato).Nombre), ""));
            }

            var cultivos = (from cc in dc.CultivoContrato
                            where idsCultivosContrato.Contains(cc.IdCultivoContrato)
                            select cc.IdCultivo).Distinct().ToList();

            if (cultivos.Count() > 1)
            {
                result.Add(new RuleViolation("El contrato no puede tener más de un cultivo básico (Trigo, Raps, Avena, etc...)", ""));
            }
            return result;
        }

        public Cultivo GetCultivo()
        {
            //Se supone que los contratos tienen solo un cultivo básico subyacente
            var firstItem = ItemContrato.FirstOrDefault();
            if (firstItem == null)
                return null;

            return firstItem.CultivoContrato.Cultivo;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }

        public void LoadControlConvenioCambioMoneda()
        {
            //Los cierres a utilizar
            var precios = new List<PrecioIngreso>();
            foreach (var itemContrato in this.ItemContrato)
                precios.AddRange(itemContrato.PrecioIngreso.Where(pi => pi.IdMoneda == 2));

            //Inicializamos
            foreach (var convenio in this.ConvenioCambioMoneda)
                convenio.Usos = new List<UsoConvenioCambioMoneda>();


            //asignamos
            foreach (var cierre in precios)
            {
                var saldoCierre = cierre.Total; //Total de dólares del cierre
                //Buscamos cierres que utilizaron la tasa de cambio convenida
                foreach (var convenio in this.ConvenioCambioMoneda.Where(ccm => ccm.PrecioUnidad == cierre.TasaCambio).OrderBy(ccm => ccm.Prioridad))
                {
                    decimal saldoConvenio = convenio.Cantidad - convenio.CantidadUtilizada;
                    if (saldoCierre > 0 && saldoConvenio > 0)
                    {
                        decimal asignable = (saldoConvenio > saldoCierre) ? saldoCierre : saldoConvenio;

                        saldoCierre -= asignable;
                        convenio.Usos.Add(new UsoConvenioCambioMoneda() { PrecioIngreso = cierre, CantidadUtilizada = asignable });
                    }
                }
            }
        }
    }


}