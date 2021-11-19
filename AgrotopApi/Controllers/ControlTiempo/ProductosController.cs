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
    public class ProductosController : ApiController
    {
        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public List<SelectEnvase> GetEnvases(int id)
        {
            List<SelectEnvase> list = new List<SelectEnvase>();

            List<CTR_Envase> envaseList = (from X in dc.CTR_Envase
                                           join Y in dc.CTR_ProductoEnvase on X.IdEnvase equals Y.IdEnvase
                                           where Y.IdProducto == id
                                           orderby X.IdEnvase
                                           select X).ToList();

            foreach (var envase in envaseList)
            {
                list.Add(new SelectEnvase()
                {
                    IdEnvase = envase.IdEnvase,
                    Descripcion = envase.Descripcion
                });
            }

            return list;
        }

        public List<SelectProducto> GetProductos(int id, int idPlanta)
        {
            List<SelectProducto> list = new List<SelectProducto>();

            List<CTR_Producto> productoList = (from X in dc.CTR_Producto
                                               join Y in dc.CTR_ProductoEmpresa on X.IdProducto equals Y.IdProducto
                                               join Z in dc.CTR_ProductoPlanta on X.IdProducto equals Z.IdProducto
                                               where Y.IdEmpresa == id && Z.IdPlantaProduccion == idPlanta
                                               && X.Habilitado == true
                                               orderby X.Nombre
                                               select X).ToList();

            foreach (var producto in productoList)
            {
                list.Add(new SelectProducto()
                {
                    IdProducto = producto.IdProducto,
                    Nombre = producto.Nombre
                });
            }

            return list;
        }
    }
}