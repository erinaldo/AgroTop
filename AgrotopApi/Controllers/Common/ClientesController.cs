using AgrotopApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace AgrotopApi.Controllers
{
    public class ClientesController : ApiController
    {
        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public List<SelectCliente> GetClientes(int id)
        {
            List<SelectCliente> list = new List<SelectCliente>();

            List<Cliente> clienteList = (from X in dc.Cliente
                                         join Y in dc.ClienteEmpresa on X.IdCliente equals Y.IdCliente
                                         where Y.IdEmpresa == id
                                         orderby X.RazonSocial
                                         select X).ToList();

            foreach (var cliente in clienteList)
            {
                list.Add(new SelectCliente()
                {
                    IdCliente = cliente.IdCliente,
                    RazonSocial = cliente.RazonSocial
                });
            }

            return list;
        }
    }
}
