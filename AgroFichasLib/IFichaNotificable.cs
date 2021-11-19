using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroFichasLib
{
    public interface IFichaNotificable
    {
        int ID { get; }
        string NombrePredio { get; }
        string NombreAgricultor { get; }
        string EmailAgricultor { get; }
        string UserIns { get; set; }
        string PdfUrl { get; }
        string SendUrl { get; }

        List<string> GetDestinatariosFicha();
    }
}
