using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels
{
    public class TransportistaSelectorViewModel
    {
        public TransportistaSelectorViewModel(int IdTransportista, string RUT, string Nombre)
        {
            this.IdTransportista = IdTransportista;
            this.RUT = RUT;
            this.Nombre = Nombre;
        }

        public TransportistaSelectorViewModel()
        {
            this.IdTransportista = 0;
            this.RUT = "";
            this.Nombre = "";
        }

        public int IdTransportista { get; set; }
        public string RUT { get; set; }
        public string Nombre { get; set; }
    }

    public class TransportistaFinderViewModel
    {
        public int IdTransportistaArgument { get; set; }
    }
}