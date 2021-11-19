using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgrotopApi.Models
{
    public class SelectPlanificacionSemanal
    {
        public int IdPlanificacionSemanal { get; set; }

        public int Año { get; set; }

        public int Semana { get; set; }

        public int IdEmpresa { get; set; }

        public string Empresa { get; set; }

        public int IdCliente { get; set; }

        public string IdClienteSAPAT { get; set; }

        public string IdClienteSAPOT { get; set; }

        public string IdClienteSAPICI { get; set; }

        public string Cliente { get; set; }

        public int IdProducto { get; set; }

        public string Producto { get; set; }

        public string Destino { get; set; }

        public decimal OC { get; set; }

        public decimal LC { get; set; }

        public string Pais { get; set; }

        public string Lote { get; set; }

        public string DUS { get; set; }

        public string Reserva { get; set; }

        public int Lunes { get; set; }
        public string FechaLunes      { get; set; }

        public int Martes          { get; set; }
        public string FechaMartes     { get; set; }

        public int Miercoles       { get; set; }
        public string FechaMiercoles  { get; set; }

        public int Jueves          { get; set; }
        public string FechaJueves     { get; set; }

        public int Viernes         { get; set; }
        public string FechaViernes    { get; set; }

        public int Sabado          { get; set; }
        public string FechaSabado     { get; set; }

        public int Domingo         { get; set; }
        public string FechaDomingo    { get; set; }
    }
}