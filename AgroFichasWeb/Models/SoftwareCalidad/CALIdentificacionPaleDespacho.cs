using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZXing;
using ZXing.QrCode;

namespace AgroFichasWeb.Models.SoftwareCalidad
{
    public class CALIdentificacionPaleDespacho
    {
        #region 1. DataContexts

        #endregion
        #region 2. Propiedades
        public string QRCode { get; set; }
        #endregion
        #region 3. Funciones

        #endregion
        #region 4. SelectLists

        #endregion
        #region 5. Validaciones
        public bool ValidacionIdentificacionPale(ModelStateDictionary modelState, HttpContext httpContext)
        {
            string errMsg = "";
            if (string.IsNullOrEmpty(httpContext.Request["QRCode"]))
            {
                errMsg = "El QR-Code es requerido";
                modelState.AddModelError("QRCode", errMsg);
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion
    }
}