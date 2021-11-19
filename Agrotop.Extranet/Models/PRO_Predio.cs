using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agrotop.Extranet.Models
{
    [MetadataType(typeof(PRO_Predio.Metadata))]
    public partial class PRO_Predio
    {
        private sealed class Metadata
        {
            [Required(ErrorMessage = "Debe escribir / seleccionar una comuna")]
            [DisplayName("Ubicación")]
            public int IdComuna { get; set; }

            [Required(ErrorMessage = "Debe escribir / seleccionar una temporada")]
            [DisplayName("Id Temporada")]
            public int IdTemporada { get; set; }

            [Required(ErrorMessage = "Debe escribir un nombre para el predio")]
            [DisplayName("Nombre")]
            public string Nombre { get; set; }
        }
    }
}
