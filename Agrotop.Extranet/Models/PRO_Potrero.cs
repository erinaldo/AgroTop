using Agrotop.Extranet.Models.PlataformaProductiva;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Agrotop.Extranet.Models
{
    [MetadataType(typeof(PRO_Potrero.Metadata))]
    public partial class PRO_Potrero
    {
        private sealed class Metadata
        {
            [Required(ErrorMessage = "Debe escribir / seleccionar un predio")]
            [DisplayName("Id Predio")]
            public int IdPredio { get; set; }

            [Required(ErrorMessage = "Debe escribir un nombre para el potrero")]
            [DisplayName("Nombre")]
            public string Nombre { get; set; }

            [Required(ErrorMessage = "Debe escribir una superficie para el potrero")]
            [DisplayName("Superficie")]
            public int Superficie { get; set; }

            [Required(ErrorMessage = "Debe escribir / seleccionar una variedad")]
            [DisplayName("Variedad")]
            public int IdCultivo { get; set; }

            [Required(ErrorMessage = "Debe escribir / seleccionar un costo financiero")]
            [DisplayName("Costos Financieros (Máximo 12% Anual)")]
            public int CostoFinanciero { get; set; }

            [Required(ErrorMessage = "Debe escribir / seleccionar un costo arriendo")]
            [DisplayName("Costo Arriendo")]
            public int CostoArriendo { get; set; }
        }

        public Actividad ParseLaborRequest(HttpContextBase httpContext)
        {
            return Actividad.ParseLaborRequest(httpContext);
        }

        public Actividad ParseInsumoRequest(HttpContextBase httpContext)
        {
            return Actividad.ParseInsumoRequest(httpContext);
        }

        public Actividad ParseFleteRequest(HttpContextBase httpContext)
        {
            return Actividad.ParseFleteRequest(httpContext);
        }

        public Actividad ParseManoObraRequest(HttpContextBase httpContext)
        {
            return Actividad.ParseManoObraRequest(httpContext);
        }
    }
}
