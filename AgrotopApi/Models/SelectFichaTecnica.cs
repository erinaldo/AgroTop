using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgrotopApi.Models
{
    public class SelectFichaTecnica
    {
        public int IdFichaTecnica { get; set; }

        public string Codigo { get; set; }

        public int Version { get; set; }

        public int IdCliente { get; set; }

        public string IdClienteSap { get; set; }

        public string Cliente { get; set; }

        public string FamiliaProducto { get; set; }

        public string Producto { get; set; }

        public string Pais { get; set; }

        public string Sag { get; set; }

        public string Fumigacion { get; set; }

        public int PesoTotalPickingTest { get; set; }

        public string Granel { get; set; }

        public List<SelectFichaTecnicaSacos> Sacos { get; set; }

        public string Temperatura { get; set; }

        public string HumedadRelativa { get; set; }

        public int VidaUtil { get; set; }

        public List<SelectFichaTecnicaParametroAnalisis> ParametroAnalisis { get; set; }

        public List<SelectFichaTecnicaParametroPesticida> ParametroAnalisisPesticida { get; set; }

        public List<SelectFichaTecnicaParametroMetalesPesados> ParametroAnalisisMetalesPesados { get; set; }

        public List<SelectFichaTecnicaParametroMicotoxinas> ParametroAnalisisMicotoxinas { get; set; }

        public List<SelectFichaTecnicaParametroMicrobiologia> ParametroAnalisisMicrobiologia { get; set; }

        public List<SelectFichaTecnicaParametroNutricionales> ParametroAnalisisNutricionales { get; set; }

        public List<SelectFichaTecnicaFrecuenciaAnalisis> FrecuenciaAnalisis { get; set; }

        public List<SelectFichaTecnicaControlVersion> ControlVersion { get; set; }

        public string Observacion { get; set; }

       public string VerificacionCliente { get; set; }
    }
}