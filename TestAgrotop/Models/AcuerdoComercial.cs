using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestAgrotop.Models
{
    public class AcuerdoComercial
    {
        public int AñoContrato { get; set; }
        public string Comuna { get; set; }
        public string Correo { get; set; }
        public string Cultivo { get; set; }
        public string Domicilio { get; set; }
        public string Empresa { get; set; }
        public string EmpresaRazonSocial { get; set; }
        public string EmpresaRut { get; set; }
        public DateTime FechaContrato { get; set; }
        public string GastosTransportes { get; set; }
        public string NumeroContrato { get; set; }
        public string Predio { get; set; }
        public string Proveedor { get; set; }
        public string Representante { get; set; }
        public string Rut { get; set; }
        public string RutRepresentante { get; set; }
        public int Superficie { get; set; }
        public string Temporada { get; set; }
        public int Toneladas { get; set; }
        public string Ubicacion { get; set; }
        public string Variedad { get; set; }
    }
}