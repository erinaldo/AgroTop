using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.ViewModels
{
    public class AsignarCamionViewModel
    {
        public IEnumerable<SelectListItem> CamionesList { get; set; }
        public LOG_Chofer Chofer { get; set; }
        public IEnumerable<SelectListItem> ChoferesList { get; set; }
        public string Destino { get; set; }
        public int IdCamion { get; set; }
        public int IdChofer { get; set; }
        public int IdPedido { get; set; }
        public int IdRequerimiento { get; set; }
        public int IdTransportista { get; set; }
        public string MsgErr { get; set; }
        public string MsgOk { get; set; }
        public string Origen { get; set; }
        public LOG_Transportista Transportista { get; set; }
        public IEnumerable<SelectListItem> TransportistasList { get; set; }
        public decimal ValorFletePorKgTransportado { get; set; }
        public bool Reasignar { get; set; }
    }
}