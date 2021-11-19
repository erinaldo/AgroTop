using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace AgroFichasWeb.ViewModels.OrdenesCompra
{
    public class OrdenCompraViewModel
    {
        public string IdProveedor { get; set; }

        public int IdMaterial { get; set; }

        public string CondicionPago { get; set; }

        public int IdLiquidacion { get; set; }

        public int IdProyecto { get; set; }

        public string ErrorMessage { get; set; }

        public int? OC { get; set; }

        public IEnumerable<SelectListItem> MaterialesSelectList { get; set; }

        public void SetMaterialesSelectList(int IdEmpresa, int? IdMaterial, AgroFichasDBDataContext dc)
        {
            string[] ignorados = { "FIPRPA01", "FIDEPE01" };
            this.MaterialesSelectList = (from X in dc.OC_Material
                                         join Y in dc.Empresa on X.IdEmpresa equals Y.IdEmpresa
                                         where X.IdEmpresa == IdEmpresa
                                         && X.IdProyecto == this.IdProyecto
                                         && ignorados.Contains(X.CodigoMaterial) == false
                                         orderby X.Descripcion
                                         select new SelectListItem
                                         {
                                             Selected = (X.IdMaterial == IdMaterial && IdMaterial != null),
                                             Text = X.Descripcion,
                                             Value = X.IdMaterial.ToString()
                                         });
        }

        public bool ValidateFirstLoadEditar(OrdenCompraViewModel ordenCompraViewModel)
        {
            var isValid = true;
            if (string.IsNullOrWhiteSpace(ordenCompraViewModel.IdProveedor))
            {
                isValid = false;
                this.ErrorMessage = "Debe ingresar el código del proveedor";
            }
            if (ordenCompraViewModel.IdMaterial == 0)
            {
                isValid = false;
                this.ErrorMessage = "Debe seleccionar un material";
            }
            if (isValid && string.IsNullOrWhiteSpace(ordenCompraViewModel.CondicionPago))
            {
                isValid = false;
                this.ErrorMessage = "Debe ingresar la condición de pago";
            }
            if (OC.HasValue && OC.Value <= 0)
            {
                isValid = false;
                this.ErrorMessage = "El número de O/C no es válido";
            }
            return isValid;
        }

        public void CrearXmlOrdenDeCompra(List<OC_OrdenCompra> ordenDeCompra, AgroFichasDBDataContext dc)
        {
            var PrimeraGlosa = ordenDeCompra.First();
            var Firma = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            var Filename = string.Format("OC_P2_L{0}_F{1}.xml", PrimeraGlosa.IdLiquidacion, Firma);
            var FullPath = string.Format(@"{0}\{1}", ConfigurationManager.AppSettings["OcsPendientes"], Filename);

            XmlTextWriter writer = new XmlTextWriter(FullPath, System.Text.Encoding.UTF8);
            writer.WriteStartDocument(true);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 4;
            writer.WriteStartElement("BOM");
            writer.WriteStartElement("Documents");
            writer.WriteStartElement("row");
            writer.WriteStartElement("CardCode");
            writer.WriteString(PrimeraGlosa.IdProveedor);
            writer.WriteEndElement();///CardCode
            writer.WriteStartElement("DocDate");
            writer.WriteString(string.Format("{0:yyyyMMdd}", PrimeraGlosa.FechaDocumento));
            writer.WriteEndElement();///DocDate
            writer.WriteStartElement("DocDueDate");
            writer.WriteString(string.Format("{0:yyyyMMdd}", PrimeraGlosa.FechaDocumento));
            writer.WriteEndElement();///DocDueDate
            writer.WriteStartElement("TaxDate");
            writer.WriteString(string.Format("{0:yyyyMMdd}", PrimeraGlosa.FechaDocumento));
            writer.WriteEndElement();///TaxDate
            writer.WriteStartElement("Comments");
            writer.WriteString(string.Format(@"LIQ.{0}\N{1}", PrimeraGlosa.IdLiquidacion, PrimeraGlosa.CondicionPago));
            writer.WriteEndElement();///Comments
            writer.WriteEndElement();///row
            writer.WriteEndElement();///Documents
            writer.WriteStartElement("Document_Lines");

            foreach (var glosa in ordenDeCompra)
            {
                writer.WriteStartElement("row");
                writer.WriteStartElement("CostingCode");
                writer.WriteString(dc.GetCentroCosto(glosa.IdProyecto, glosa.IdEmpresa, glosa.IdCentroCosto).SingleOrDefault().Id);
                writer.WriteEndElement();
                writer.WriteStartElement("ItemCode");
                writer.WriteString(dc.GetMaterial(glosa.IdMaterial).SingleOrDefault().Id);
                writer.WriteEndElement();
                writer.WriteStartElement("Quantity");
                writer.WriteValue(glosa.Cantidad);
                writer.WriteEndElement();
                writer.WriteStartElement("Price");
                writer.WriteValue(glosa.PrecioUnitario);
                writer.WriteEndElement();
                writer.WriteStartElement("LineTotal");
                writer.WriteValue(glosa.PrecioTotal);
                writer.WriteEndElement();
                writer.WriteEndElement();///row
            }

            writer.WriteEndElement();///Document_Lines
            writer.WriteEndElement();///BOM
            writer.WriteEndDocument();
            writer.Close();

            // Firma
            var ordenDeCompraDB = dc.OC_OrdenCompra.Where(X => X.IdProyecto == PrimeraGlosa.IdProyecto && X.IdLiquidacion == PrimeraGlosa.IdLiquidacion);
            foreach (var glosa in ordenDeCompraDB)
            {
                glosa.Firma = Firma;
                dc.SubmitChanges();
            }
        }
    }
}