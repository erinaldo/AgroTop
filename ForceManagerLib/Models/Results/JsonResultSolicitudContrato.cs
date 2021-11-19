using ForceManagerLib.Models.Genericos;
using System.Collections.Generic;

namespace ForceManagerLib.Models.Results
{
    public class JsonResultSolicitudContrato
    {
        public int id { get; set; }

        public accountId accountId { get; set; }

        public Z_idCultivo Z_idCultivo { get; set; }

        public int? Z_PrecioCierre { get; set; }

        public int? Z_ToneladasCierre { get; set; }

        public Z_idTipoContrato Z_idTipoContrato { get; set; }

        public Z_idComunaOrigen Z_idComunaOrigen { get; set; }

        public Z_idSucursalEntrega Z_idSucursalEntrega { get; set; }

        public int? Z_HectareasContrato { get; set; }

        public int? Z_Toneladas { get; set; }

        public salesRepId salesRepId { get; set; }

        public string Z_RUT { get; set; }

        public string Z_Predio { get; set; }

        public Z_idVerificado Z_idVerificado { get; set; }

        public bool? Z_Contrato_Creado { get; set; }

        public bool? Z_Cierre_Creado { get; set; }

        public List<Z_idVariedadSolicitudContrato> Z_idVariedadSolicitudContrato { get; set; }

        public Z_Temporadas Z_Temporadas { get; set; }
    }
}
