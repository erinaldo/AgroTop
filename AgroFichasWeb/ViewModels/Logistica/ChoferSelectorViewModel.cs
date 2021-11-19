using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels
{
    public class ChoferSelectorViewModel
    {
        public ChoferSelectorViewModel(int IdTransportista, int IdChofer, string RUT, string Nombre)
        {
            this.IdTransportista = IdTransportista;
            this.IdChofer = IdChofer;
            this.RUT = RUT;
            this.Nombre = Nombre;
        }

        public ChoferSelectorViewModel()
        {
            this.IdTransportista = IdTransportista;
            this.IdChofer = 0;
            this.RUT = "";
            this.Nombre = "";
        }

        public int IdTransportista { get; set; }
        public int IdChofer { get; set; }
        public string RUT { get; set; }
        public string Nombre { get; set; }
    }

    public class ChoferFinderViewModel
    {
        public int IdTransportistaArgument { get; set; }
    }
}