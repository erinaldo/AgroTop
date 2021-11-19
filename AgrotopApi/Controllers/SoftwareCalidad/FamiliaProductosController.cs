using AgrotopApi.Models;
using MoreLinq;
using System.Collections.Generic;
using System.Data.Linq.SqlClient;
using System.Linq;
using System.Web.Http;

namespace AgrotopApi.Controllers.SoftwareCalidad
{
    public class FamiliaProductosController : ApiController
    {
        private SoftwareCalidadDBDataContext dc = new SoftwareCalidadDBDataContext();

        public List<SelectFamiliaProducto> Get()
        {
            List<SelectFamiliaProducto> list = (from X in dc.vw_CAL_Producto.DistinctBy(X => X.Familia)
                                                orderby X.Familia
                                                select new SelectFamiliaProducto
                                                {
                                                    IdFamiliaProducto = X.IdProducto,
                                                    Nombre = X.Familia
                                                }).ToList();

            return list;
        }

        public List<SelectProducto> GetProductos()
        {
            List<SelectProducto> list = (from X in dc.vw_CAL_Producto
                                         where !X.Producto.ToUpper().Contains("GRANEL")
                                         orderby X.Producto
                                         select new SelectProducto
                                         {
                                             IdProducto = X.IdSubproducto,
                                             Nombre = X.Producto,
                                             FamiliaProducto = new SelectFamiliaProducto()
                                             {
                                                 IdFamiliaProducto = X.IdProducto,
                                                 Nombre = X.Familia
                                             }
                                         }).Distinct().ToList();

            return list;
        }

        public List<SelectProducto> GetProductosGranel()
        {
            List<SelectProducto> list = (from X in dc.vw_CAL_Producto
                                         where X.Producto.ToUpper().Contains("GRANEL")
                                         orderby X.Producto
                                         select new SelectProducto
                                         {
                                             IdProducto = X.IdSubproducto,
                                             Nombre = X.Producto,
                                             FamiliaProducto = new SelectFamiliaProducto()
                                             {
                                                 IdFamiliaProducto = X.IdProducto,
                                                 Nombre = X.Familia
                                             }
                                         }).Distinct().ToList();

            return list;
        }

        public List<SelectProducto> GetProductosCascada(int id)
        {
            List<SelectProducto> list = (from X in dc.vw_CAL_Producto
                                         where X.IdProducto == id
                                         orderby X.Producto
                                         select new SelectProducto
                                         {
                                             IdProducto = X.IdSubproducto,
                                             Nombre = X.Producto
                                         }).ToList();

            return list;
        }
    }
}
