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
    public class CALIdentificacionPale
    {
        #region 1. DataContexts

        #endregion
        #region 2. Propiedades
        public string QRCode { get; set; }
        #endregion
        #region 3. Funciones
        public string CreateQRCode(string QRCodeData)
        {
            QrCodeEncodingOptions options = new QrCodeEncodingOptions()
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                Width = 250,
                Height = 250,
            };

            BarcodeWriter writer = new BarcodeWriter()
            {
                Format = BarcodeFormat.QR_CODE,
                Options = options
            };

            Bitmap QRCode = new Bitmap(writer.Write(QRCodeData));

            Models.ImageConverter imageConverter = new Models.ImageConverter();
            byte[] byteArrayQRCode = imageConverter.imageToByteArray(QRCode);

            return GetImgSrc(byteArrayQRCode);
        }

        public string CreateQRCodeData(CAL_Pale cAL_Pale, int nroSaco)
        {
            return string.Format("SCO_PLE{0}_TPLE_{1}_{2}", cAL_Pale.IdPale, Util.RemoveDiacritics(cAL_Pale.CAL_TipoPale.Descripcion.Replace(" ", "").ToUpper()), nroSaco);
        }

        public string GetImgSrc(byte[] QR_CODE)
        {
            string base64 = Convert.ToBase64String(QR_CODE);
            return string.Format("data:image/gif;base64,{0}", base64);
        }
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