using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models.API.TrazaTop
{
    public class RequestPlanillaContrato
    {
        public int IdPlanillaContrato { get; set; }

        public int IdTemporada { get; set; }

        public int? IdTipoContrato { get; set; }

        public int? IdCultivo { get; set; }
    }
}