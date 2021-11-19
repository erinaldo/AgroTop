using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models.API.TrazaTop
{
    public class ResponsePlanillaContrato
    {
        public int IdPlanillaContrato { get; set; }

        public string Documento { get; set; }

        public string Observacion { get; set; }

        public int IdTemporada { get; set; }

        public int? IdTipoContrato { get; set; }

        public int? IdCultivo { get; set; }

        public string UserIns { get; set; }

        public DateTime FechaHoraIns { get; set; }

        public string IpIns { get; set; }

        public string UserUpd { get; set; }

        public DateTime? FechaHoraUpd { get; set; }

        public string IpUpd { get; set; }
    }
}