using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class SolicitudPrecio
    {
        public string EstadoProceso
        {
            get
            {
                if (this.Procesado)
                {
                    if (this.IdConvenioPrecio.HasValue || this.IdConvenioPrecioAutorizacion.HasValue)
                        return "PROCESADA";
                    else
                        return "DESCARTADA";
                }
                else
                {
                    return "EN REVISION";
                }
            }
        }

        public string ColorProceso
        {
            get
            {
                if (this.Procesado)
                {
                    return this.IdConvenioPrecio.HasValue || this.IdConvenioPrecioAutorizacion.HasValue ? "#83f03c" : "#ff8e73";
                }
                else
                {
                    return "#FFE640";
                }
            }
        }
    }
}