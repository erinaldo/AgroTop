using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_Pale
    {
        #region 1. DataContexts
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        #endregion
        #region 2. Propiedades
        public int CntCargada { get; set; }
        public int CntMax { get; set; }
        //public int HiddenSegundaCarga              { get; set; }
        public int IdReprocesoSacosDañadosPallets { get; set; }
        public int UltimoPallet_IdTipoPale { get; set; }
        public int UltimoPallet_CantidadPaletizada { get; set; }
        public int UltimoPallet_IdControlFechado { get; set; }
        public int UltimoPallet_CntMax { get; set; }
        public int UltimoPallet_CntPallet { get; set; }
        public int CntPallets { get; set; }
        #endregion
        #region 3. Funciones
        public CAL_DespachoPale GetDespachoPale(int IdPale)
        {
            return dcSoftwareCalidad.CAL_DespachoPale.Single(X => X.IdPale == IdPale && X.Habilitado == true);
        }
        public int GetCantidadDisponible()
        {
            List<CAL_ReprocesoSacosDañadosPalletsDetalle> detalles = dcSoftwareCalidad.CAL_ReprocesoSacosDañadosPalletsDetalle.Where(X => X.IdPale == this.IdPale).ToList();
            if (detalles.Count > 0)
                return detalles.Last().CntDisponible;
            else
                return this.CAL_TipoPale.CntMax;
        }

        public int GetCantidadDisponibleUsarEstePallet()
        {
            List<CAL_ReprocesoSacosDañadosPalletsDetalle> detalles = dcSoftwareCalidad.CAL_ReprocesoSacosDañadosPalletsDetalle.Where(X => X.IdPale == this.IdPale).ToList();
            if (detalles.Count > 0)
                return detalles.Last().CntDisponible;
            else
                return this.CntMax;
        }

        public string GetImgSrc()
        {
            if (this.QR_CODE == null)
                return string.Empty;
            byte[] byteArray = QR_CODE.ToArray();
            string base64 = Convert.ToBase64String(byteArray);
            string imgSrc = string.Format("data:image/gif;base64,{0}", base64);
            return imgSrc;
        }

        public CAL_Turno2 GetTurno()
        {
            return dcSoftwareCalidad.CAL_Turno2.Single(X => X.IdTurno == this.IdTurno2);
        }

        public bool NoEstaAnalizado(int IdPale)
        {
            return (dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true) == null);
        }

        public bool NoEstaCargado(int IdPale)
        {
            return (dcSoftwareCalidad.CAL_DespachoPale.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true) == null);
        }

        public bool NoEstaLiberado(int IdPale)
        {
            return (dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true && X.Liberado == true) == null);
        }

        public bool NoEstaReprocesado(int IdPale)
        {
            return (dcSoftwareCalidad.CAL_ReprocesoPallets.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true) == null);
        }

        public bool NoEstaRetenido(int IdPale)
        {
            return (dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true && X.Retenido == true) == null);
        }

        public bool NoEstaRetenidoAut(int IdPale)
        {
            return (dcSoftwareCalidad.CAL_AnalisisPale.SingleOrDefault(X => X.IdPale == IdPale && X.Habilitado == true && X.RetenidoAut == true) == null);
        }
        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetControlFechado(int? IdControlFechado)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_FTSacoControlFechado
                                                     orderby X.IdControlFechado
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdControlFechado == IdControlFechado && IdControlFechado != null),
                                                         Text = X.Descripcion,
                                                         Value = X.IdControlFechado.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetTipoPales(int? IdTipoPale)
        {
            IEnumerable<SelectListItem> selectList = from X in dcSoftwareCalidad.CAL_TipoPale
                                                     orderby X.Descripcion
                                                     select new SelectListItem
                                                     {
                                                         Selected = (X.IdTipoPale == IdTipoPale && IdTipoPale != null),
                                                         Text = X.Descripcion,
                                                         Value = X.IdTipoPale.ToString()
                                                     };
            return selectList;
        }

        public IEnumerable<SelectListItem> GetCntPallets()
        {
            IEnumerable<SelectListItem> selectListItems = new List<SelectListItem>()
            {
                new SelectListItem()
                {
                    Text = "1",
                    Selected = true,
                    Value = "1"
                },
                new SelectListItem()
                {
                    Text = "5",
                    Selected = false,
                    Value = "5"
                },
                new SelectListItem()
                {
                    Text = "10",
                    Selected = false,
                    Value = "10"
                },
            };

            return selectListItems;
        }
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

            #region CALPale

            if ((controllerName == "CALPale" || controllerName == "calpale") && this.IdTipoPale == 0)
                yield return new RuleViolation("El tipo es requerido", "IdTipoPale");

            if ((controllerName == "CALPale" || controllerName == "calpale") && this.CantidadPaletizada == 0)
                yield return new RuleViolation("La cantidad paletizada es requerida", "CantidadPaletizada");

            if ((controllerName == "CALPale" || controllerName == "calpale") && !this.IdControlFechado.HasValue)
                yield return new RuleViolation("El control de fechado es requerido", "IdControlFechado");

            #endregion

            #region CALPale->CrearPale

            if ((controllerName == "CALPale" || controllerName == "calpale") && (actionName == "CrearPale" || actionName == "crearpale") && this.CntMax == 0)
                yield return new RuleViolation("El tipo es requerido", "IdTipoPale");

            if ((controllerName == "CALPale" || controllerName == "calpale") && (actionName == "CrearPale" || actionName == "crearpale") && (this.CantidadPaletizada > this.CntMax))
                yield return new RuleViolation(string.Format("La cantidad paletizada no puede ser mayor al tamaño del pallet (soporta {0} sacos)", this.CntMax), "CantidadPaletizada");

            if ((controllerName == "CALPale" || controllerName == "calpale") && (actionName == "CrearPale" || actionName == "crearpale") && this.CntPallets == 0)
                yield return new RuleViolation("La cantidad tiene que ser entre 1, 5 o 10", "CntPallets");
            
            #endregion

            #region CALPale->EditarPallet

            if ((controllerName == "CALPale" || controllerName == "calpale") && (actionName == "EditarPallet" || actionName == "editarpallet") && this.CntMax == 0)
                yield return new RuleViolation("El tipo es requerido", "IdTipoPale");

            if ((controllerName == "CALPale" || controllerName == "calpale") && (actionName == "EditarPallet" || actionName == "editarpallet") && (this.CantidadPaletizada > this.CntMax))
                yield return new RuleViolation(string.Format("La cantidad paletizada no puede ser mayor al tamaño del pallet (soporta {0} sacos)", this.CntMax), "CantidadPaletizada");

            #endregion

            #region CALSacosDañados->CrearPallet

            if ((controllerName == "CALSacosDañados" || controllerName == "calsacosdañados") && (actionName == "CrearPallet" || actionName == "crearpallet") && this.IdTipoPale == 0)
                yield return new RuleViolation("El tipo es requerido", "IdTipoPale");

            if ((controllerName == "CALSacosDañados" || controllerName == "calsacosdañados") && (actionName == "CrearPallet" || actionName == "crearpallet") && this.CntCargada == 0)
                yield return new RuleViolation("La cantidad cargada es requerida", "CntCargada");

            #endregion

            #region CALSacosDañados->UsarEstePallet

            if ((controllerName == "CALSacosDañados" || controllerName == "calsacosdañados") && (actionName == "UsarEstePallet" || actionName == "usarestepallet") && this.CntCargada == 0)
                yield return new RuleViolation("La cantidad cargada es requerida", "CntCargada");

            #endregion

            yield break;
        }

        public void ValidateUsarEstePallet(ModelStateDictionary modelState, FormCollection formCollection)
        {
            if (this.CntCargada == 0)
                modelState.AddModelError("CntCargada", "La cantidad cargada es requerida");
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
        #endregion
    }
}