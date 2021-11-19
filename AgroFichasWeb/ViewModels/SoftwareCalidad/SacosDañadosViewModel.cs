using AgroFichasWeb.Models;
using AgroFichasWeb.Models.SoftwareCalidad;
using System.Collections.Generic;
using System.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.ViewModels.SoftwareCalidad
{
    public class SacosDañadosViewModel
    {
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();

        public List<CAL_GetLoteSacosDañadosResult> SacosDañados { get; set; }
        public List<CAL_GetDetalleLoteSacosDañadosResult> DetalleSacosDañados { get; set; }

        #region *** Funciones ***
        #endregion

    }
}