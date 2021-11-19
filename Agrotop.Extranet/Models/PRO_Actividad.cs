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
    [MetadataType(typeof(PRO_Actividad.Metadata))]
    public partial class PRO_Actividad
    {
        private sealed class Metadata
        {
            [Required(ErrorMessage = "Debe escribir / seleccionar una descripción")]
            [DisplayName("Descripción")]
            public string Descripcion { get; set; }

            [Required(ErrorMessage = "Debe escribir / seleccionar una descripción del agricultor")]
            [DisplayName("Descripción Agricultor")]
            public string DescripcionAgricultor { get; set; }

            [Required(ErrorMessage = "Debe escribir / seleccionar un mes")]
            [DisplayName("Mes")]
            public string Mes { get; set; }

            [Required(ErrorMessage = "Debe escribir / seleccionar una cantidad")]
            [DisplayName("Cantidad")]
            public decimal Cantidad { get; set; }

            [Required(ErrorMessage = "Debe escribir / seleccionar una unidad")]
            [DisplayName("Unidad")]
            public string Unidad { get; set; }

            [Required(ErrorMessage = "Debe escribir / seleccionar un valor unitario")]
            [DisplayName("Valor Unitario")]
            public decimal ValorUnitario { get; set; }

            [Required(ErrorMessage = "Debe escribir / seleccionar un valor ítem")]
            [DisplayName("Valor Ítem")]
            public decimal ValorItem { get; set; }
        }
    }
}
