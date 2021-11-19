using AgrotopApiSap.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Mvc;

namespace AgrotopApiSap.Controllers
{
    public class ImportSapController : ApiController
    {
        public JsonResult<ResultImport> AddBatch([FromBody]Recepcion recepcion)
        {
            SapObject sap = new SapObject();
            // Modo prueba para Avenatop
            sap.ResolveCredentials(2, true);

            ResultImport result = new ResultImport();

            result.code = 13;
            result.message = ">TEST";
            result.OK = true;
            return Json<ResultImport>(result);

            //try
            //{
            //    SAPbobsCOM.Documents oPurchaseDeliveryNotes = (SAPbobsCOM.Documents)sap.oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseDeliveryNotes);
            //    oPurchaseDeliveryNotes.CardCode                                      = recepcion.CardCode.ToString();
            //    oPurchaseDeliveryNotes.Comments                                      = recepcion.Comments;
            //    oPurchaseDeliveryNotes.DocCurrency                                   = recepcion.DocCurrency;
            //    oPurchaseDeliveryNotes.DocDate                                       = recepcion.DocDate;
            //    oPurchaseDeliveryNotes.TaxDate                                       = recepcion.TaxDate;
            //    oPurchaseDeliveryNotes.DocDueDate                                    = recepcion.DocDueDate;
            //    oPurchaseDeliveryNotes.FolioPrefixString                             = recepcion.FolioPrefixString;
            //    oPurchaseDeliveryNotes.FolioNumber                                   = recepcion.FolioNumber;
            //    oPurchaseDeliveryNotes.NumAtCard                                     = recepcion.NumAtCard;
            //    oPurchaseDeliveryNotes.UserFields.Fields.Item("U_liquidacion").Value = recepcion.U_liquidacion;
            //    oPurchaseDeliveryNotes.Lines.ItemCode                                = recepcion.ItemCode;
            //    oPurchaseDeliveryNotes.Lines.Quantity                                = (double)recepcion.Quantity;
            //    oPurchaseDeliveryNotes.Lines.Price                                   = (double)recepcion.Price;
            //    oPurchaseDeliveryNotes.Lines.TaxCode                                 = recepcion.TaxCode;
            //    oPurchaseDeliveryNotes.Lines.Add();

            //    if (result.code != 0)
            //    {
            //        result.OK = false;
            //        result.message = sap.oCompany.GetLastErrorDescription();
            //    }
            //    else
            //    {
            //        result.OK = true;
            //        result.code = oPurchaseDeliveryNotes.Add();
            //        result.message = sap.oCompany.GetNewObjectKey();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    result.OK = false;
            //    result.message = ex.ToString();
            //}

            //return Json<ResultImport>(result);
        }
    }
}