using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models.TrazaTop.Documentos
{
    public class Anexo : DocumentoContrato
    {
        public Anexo(int idSolicitudContrato)
        {
            this.IdSolicitudContrato = idSolicitudContrato;
        }

        [Obsolete]
        public string CrearPDF(string userIns, DateTime fechaHoraIns, string ipIns)
        {
            string html = GetPlanillaHTMLAnexo();
            return CrearDoctoContrato(html, 2, 2, userIns, fechaHoraIns, ipIns);
        }
    }
}