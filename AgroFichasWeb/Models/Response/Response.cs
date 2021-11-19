using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models.Response
{
    public class Response
    {
        public bool IsSuccess { get; set; }
        public bool NIngreso { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }

        public int Id { get; set; }


        #region PlanificacionSemanal

        public string CodigoSAPCliente { get; set; }

        public string CodigoEmpresa { get; set; }

        public int IdEmpresa { get; set; }

        #endregion

    }
}