using AgrotopApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.ServiceModel.Channels;
using System.Web;
using System.Web.Http;

namespace AgrotopApi.Controllers.SoftwareCalidad
{
    public class OrdenProduccionController : ApiController
    {
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        [HttpPost]
        public ResultOrdenProduccion EditarProducto(FormDataCollection formDataCollection)
        {
            ResultOrdenProduccion result = new ResultOrdenProduccion()
            {
                OK = true
            };

            string rowKey = "";
            int Producto = 0;
            int Espesor = 0;
            int Saco = 0;
            int PesoSaco = 0;
            int Contenedor = 0;
            int SacosPorContenedor = 0;
            decimal cntCont = 0;
            int cntSaco = 0;
            decimal cntProd = 0;

            if (string.IsNullOrEmpty(formDataCollection.Get("rowKey")))
            {
                result.OK = false;
                result.Error = "La clave de fila es requerida";
                return result;
            }
            else
            {
                rowKey = formDataCollection.Get("rowKey");
            }

            if (result.OK && !int.TryParse(formDataCollection.Get("Producto"), out Producto))
            {
                result.OK = false;
                result.Error = "El producto no es válido";
                return result;
            }

            if (result.OK && !int.TryParse(formDataCollection.Get("Espesor"), out Espesor)) { }

            if (result.OK && !int.TryParse(formDataCollection.Get("Saco"), out Saco))
            {
                result.OK = false;
                result.Error = "El saco no es válido";
                return result;
            }

            if (result.OK && !int.TryParse(formDataCollection.Get("PesoSaco"), out PesoSaco))
            {
                result.OK = false;
                result.Error = "El peso del saco no es válido";
                return result;
            }

            if (result.OK && !int.TryParse(formDataCollection.Get("Contenedor"), out Contenedor))
            {
                result.OK = false;
                result.Error = "El contenedor no es válido";
                return result;
            }

            if (result.OK && !int.TryParse(formDataCollection.Get("SacosPorContenedor"), out SacosPorContenedor))
            {
                result.OK = false;
                result.Error = "La cantidad de sacos por contenedor no es válida";
                return result;
            }

            if (result.OK)
            {
                try
                {
                    cntCont = Utils.ParseDecimal(formDataCollection.Get("cntCont"));
                }
                catch
                {
                    result.OK = false;
                    result.Error = "La cantidad de contenedores no es válida";
                    return result;
                }
            }

            if (result.OK && !int.TryParse(formDataCollection.Get("cntSaco"), out cntSaco))
            {
                result.OK = false;
                result.Error = "La cantidad de sacos no es válida";
                return result;
            }

            if (result.OK)
            {
                try
                {
                    cntProd = Utils.ParseDecimal(formDataCollection.Get("cntProd"));
                }
                catch
                {
                    result.OK = false;
                    result.Error = "La cantidad de producto no es válida";
                    return result;
                }
            }

            var detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.RowKey == rowKey);
            if (detalleOrdenProduccion == null)
            {
                result.OK = false;
                result.Error = "No se ha encontrado el producto";
            }

            if (result.OK)
            {
                try
                {
                    CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.Single(X => X.IdOrdenProduccion == detalleOrdenProduccion.IdOrdenProduccion);
                    ordenProduccion.Autorizado = false;
                    ordenProduccion.AutorizadoAuto = false;
                    ordenProduccion.UserAutoriza = User.Identity.Name;
                    ordenProduccion.FechaHoraAutoriza = DateTime.Now;
                    ordenProduccion.IpAutoriza = GetIp();
                    ordenProduccion.UserUpd = User.Identity.Name;
                    ordenProduccion.FechaHoraUpd = DateTime.Now;
                    ordenProduccion.IpUpd = GetIp();
                    dcSoftwareCalidad.SubmitChanges();

                    detalleOrdenProduccion.IdSubproducto = Producto;
                    if (Espesor != 0)
                        detalleOrdenProduccion.IdEspesorProducto = Espesor;
                    detalleOrdenProduccion.IdSaco = Saco;
                    detalleOrdenProduccion.IdPesoSaco = PesoSaco;
                    detalleOrdenProduccion.IdContenedor = Contenedor;
                    detalleOrdenProduccion.SacosPorContenedor = SacosPorContenedor;
                    detalleOrdenProduccion.CantidadContenedores = cntCont;
                    detalleOrdenProduccion.CantidadSacos = cntSaco;
                    detalleOrdenProduccion.CantidadProducto = cntProd;
                    dcSoftwareCalidad.SubmitChanges();

                    result.OK = true;
                    result.Error = "";
                    result.rowKey = rowKey;
                    result.FamiliaProductos = detalleOrdenProduccion.CAL_Producto.Nombre;
                    result.Producto = detalleOrdenProduccion.CAL_Subproducto.Nombre;

                    if (detalleOrdenProduccion.IdEspesorProducto.HasValue)
                        result.Espesor = string.Format("{0:N2}-{1:N2} mm", detalleOrdenProduccion.CAL_EspesorProducto.Min, detalleOrdenProduccion.CAL_EspesorProducto.Max);
                    else
                        result.Espesor = "(No tiene Espesor)";

                    result.Saco = detalleOrdenProduccion.CAL_Saco.Nombre;
                    result.PesoSaco = (detalleOrdenProduccion.IdPesoSaco.HasValue ? detalleOrdenProduccion.CAL_PesoSaco.Peso : 0) + " kgs.";
                    result.Contenedor = string.Format("{0} ft", detalleOrdenProduccion.CAL_Contenedor.Tamaño);
                    result.SacosPorContenedor = detalleOrdenProduccion.SacosPorContenedor.Value.ToString("N0");
                    result.cntCont = detalleOrdenProduccion.CantidadContenedores.ToString("N0");
                    result.cntSaco = detalleOrdenProduccion.CantidadSacos.Value.ToString("N0");
                    result.cntProd = string.Format("{0:N2} tns.", detalleOrdenProduccion.CantidadProducto);
                }
                catch (Exception ex)
                {
                    result.OK = false;
                    result.Error = ex.Message;
                }
            }

            detalleOrdenProduccion.CAL_OrdenProduccion.NotificarEdicion();

            return result;
        }

        [HttpPost]
        public ResultOrdenProduccion EditarProductoGranel(FormDataCollection formDataCollection)
        {
            ResultOrdenProduccion result = new ResultOrdenProduccion()
            {
                OK = true
            };

            string rowKey = "";
            int Producto = 0;
            int Contenedor = 0;
            decimal cntProd = 0;
            int cntPorCont = 0;
            decimal cntCont = 0;

            if (string.IsNullOrEmpty(formDataCollection.Get("rowKey")))
            {
                result.OK = false;
                result.Error = "La clave de fila es requerida";
                return result;
            }
            else
            {
                rowKey = formDataCollection.Get("rowKey");
            }

            if (result.OK && !int.TryParse(formDataCollection.Get("Producto"), out Producto))
            {
                result.OK = false;
                result.Error = "El producto no es válido";
                return result;
            }

            if (result.OK && !int.TryParse(formDataCollection.Get("Contenedor"), out Contenedor))
            {
                result.OK = false;
                result.Error = "El contenedor no es válido";
                return result;
            }

            if (result.OK)
            {
                try
                {
                    cntProd = Utils.ParseDecimal(formDataCollection.Get("cntProd"));
                }
                catch
                {
                    result.OK = false;
                    result.Error = "La cantidad de producto no es válida";
                    return result;
                }
            }

            if (result.OK && !int.TryParse(formDataCollection.Get("cntPorCont"), out cntPorCont))
            {
                result.OK = false;
                result.Error = "La cantidad por contenedor no es válida";
                return result;
            }

            if (result.OK)
            {
                try
                {
                    cntCont = Utils.ParseDecimal(formDataCollection.Get("cntCont"));
                }
                catch
                {
                    result.OK = false;
                    result.Error = "La cantidad de contenedores no es válida";
                    return result;
                }
            }

            var detalleOrdenProduccionGranel = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.RowKey == rowKey);
            if (detalleOrdenProduccionGranel == null)
            {
                result.OK = false;
                result.Error = "No se ha encontrado el producto";
            }

            if (result.OK)
            {
                try
                {
                    CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.Single(X => X.IdOrdenProduccion == detalleOrdenProduccionGranel.IdOrdenProduccion);
                    ordenProduccion.Autorizado = false;
                    ordenProduccion.AutorizadoAuto = false;
                    ordenProduccion.UserAutoriza = User.Identity.Name;
                    ordenProduccion.FechaHoraAutoriza = DateTime.Now;
                    ordenProduccion.IpAutoriza = GetIp();
                    ordenProduccion.UserUpd = User.Identity.Name;
                    ordenProduccion.FechaHoraUpd = DateTime.Now;
                    ordenProduccion.IpUpd = GetIp();
                    dcSoftwareCalidad.SubmitChanges();

                    detalleOrdenProduccionGranel.IdSubproducto = Producto;
                    detalleOrdenProduccionGranel.IdContenedor = Contenedor;
                    detalleOrdenProduccionGranel.CantidadProducto = cntProd;
                    detalleOrdenProduccionGranel.CantidadPorContenedor = cntPorCont;
                    detalleOrdenProduccionGranel.CantidadContenedores = cntCont;
                    dcSoftwareCalidad.SubmitChanges();

                    result.OK = true;
                    result.Error = "";
                    result.rowKey = rowKey;
                    result.FamiliaProductos = detalleOrdenProduccionGranel.CAL_Producto.Nombre;
                    result.Producto = detalleOrdenProduccionGranel.CAL_Subproducto.Nombre;
                    result.Contenedor = string.Format("{0}", detalleOrdenProduccionGranel.CAL_Contenedor.Tamaño);
                    result.cntProd = string.Format("{0:N2}", detalleOrdenProduccionGranel.CantidadProducto);
                    result.cntPorCont = string.Format("{0:N2}", detalleOrdenProduccionGranel.CantidadPorContenedor);
                    result.cntCont = detalleOrdenProduccionGranel.CantidadContenedores.ToString("N0");

                }
                catch (Exception ex)
                {
                    result.OK = false;
                    result.Error = ex.Message;
                }
            }

            detalleOrdenProduccionGranel.CAL_OrdenProduccion.NotificarEdicion();

            return result;
        }

        [HttpPost]
        public ResultOrdenProduccion EliminarProducto(FormDataCollection formDataCollection)
        {
            ResultOrdenProduccion result = new ResultOrdenProduccion()
            {
                OK = true
            };

            if (!int.TryParse(formDataCollection.Get("id"), out int id))
            {
                result.OK = false;
                result.Error = "El producto no es válido";
                return result;
            }

            var detalleOrdenProduccion = dcSoftwareCalidad.CAL_DetalleOrdenProduccion.SingleOrDefault(X => X.IdDetalleOrdenProduccion == id);
            if (detalleOrdenProduccion == null)
            {
                result.OK = false;
                result.Error = "No se ha encontrado el producto";
            }

            if (result.OK)
            {
                try
                {
                    CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.Single(X => X.IdOrdenProduccion == detalleOrdenProduccion.IdOrdenProduccion);
                    ordenProduccion.Autorizado = false;
                    ordenProduccion.AutorizadoAuto = false;
                    ordenProduccion.UserUpd = User.Identity.Name;
                    ordenProduccion.FechaHoraUpd = DateTime.Now;
                    ordenProduccion.IpUpd = GetIp();
                    dcSoftwareCalidad.SubmitChanges();

                    dcSoftwareCalidad.CAL_DetalleOrdenProduccion.DeleteOnSubmit(detalleOrdenProduccion);
                    dcSoftwareCalidad.SubmitChanges();

                    result.OK = true;
                    result.Error = "";
                }
                catch (Exception ex)
                {
                    result.OK = false;
                    result.Error = ex.Message;
                }
            }

            return result;
        }

        public ResultBarco GetBarco(int id)
        {
            ResultBarco result = new ResultBarco();
            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion != null)
            {
                result.IdBarco = ordenProduccion.Barco.IdBarco;
                result.Nombre = ordenProduccion.Barco.Nombre;
            }

            return result;
        }

        public ResultCarrier GetCarrier(int id)
        {
            ResultCarrier result = new ResultCarrier();
            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion != null)
            {
                result.IdCarrier = ordenProduccion.Carrier.IdCarrier;
                result.Nombre = ordenProduccion.Carrier.Nombre;
            }

            return result;
        }

        public List<SelectCliente> GetCliente(int id)
        {
            List<SelectCliente> list = new List<SelectCliente>();

            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id);
            if (ordenProduccion == null)
            {
                return list;
            }

            List<Cliente> clienteList = (from X in dcAgroFichas.Cliente
                                         where X.IdCliente == ordenProduccion.IdCliente
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

        public List<SelectListaEmpaque> GetListaEmpaqueCargaGranel(int id)
        {
            List<SelectListaEmpaque> list = (from X in dcSoftwareCalidad.CAL_LECargaGranel
                                             where X.IdOrdenProduccion == id
                                             && X.Habilitado == true
                                             orderby X.IdLECargaGranel
                                             select new SelectListaEmpaque
                                             {
                                                 IdLECargaGranel = X.IdLECargaGranel,
                                                 Descripcion = string.Format("Lista de Empaque Nº {0} - {1} - Nº Factura {2}", X.IdLECargaGranel, X.CAL_OrdenProduccion.LoteComercial, X.NFactura)
                                             }).ToList();
            return list;
        }

        public List<SelectListaEmpaque> GetListaEmpaqueEnvasado(int id)
        {
            List<SelectListaEmpaque> list = (from X in dcSoftwareCalidad.CAL_LEPallets
                                             where X.IdOrdenProduccion == id
                                             && X.Habilitado == true
                                             orderby X.IdLEPallets
                                             select new SelectListaEmpaque
                                             {
                                                 IdLEPallets = X.IdLEPallets,
                                                 Descripcion = string.Format("Lista de Empaque Nº {0} - {1} - Nº Factura {2}", X.IdLEPallets, X.CAL_OrdenProduccion.LoteComercial, X.NFactura)
                                             }).ToList();
            return list;
        }

        public List<SelectLote> GetLotes(int id)
        {
            List<SelectLote> list = (from X in dcSoftwareCalidad.CAL_OrdenProduccion
                                     where X.IdCliente == id
                                     && X.Habilitado == true
                                     orderby X.LoteComercial
                                     select new SelectLote
                                     {
                                         IdOrdenProduccion = X.IdOrdenProduccion,
                                         LoteComercial = X.LoteComercial
                                     }).ToList();

            return list;
        }

        public List<SelectLote> GetLotesByPlanta(int id)
        {
            List<SelectLote> list = (from X in dcSoftwareCalidad.CAL_OrdenProduccion
                                     where X.IdPlanta == id
                                     && X.Habilitado == true
                                     orderby X.LoteComercial
                                     select new SelectLote
                                     {
                                         IdOrdenProduccion = X.IdOrdenProduccion,
                                         LoteComercial = X.LoteComercial
                                     }).ToList();
            return list;
        }

        public List<SelectLote> GetLotesByPlantaGranel(int id)
        {
            List<SelectLote> selectList = (from X in dcSoftwareCalidad.CAL_DespachoCargaGranel
                                           join Y in dcSoftwareCalidad.CAL_OrdenProduccion on X.IdOrdenProduccion equals Y.IdOrdenProduccion
                                           where X.Habilitado
                                           && Y.Habilitado
                                           && (Y.Autorizado.HasValue && Y.Autorizado.Value)
                                           && !Y.Terminada
                                           && Y.IdPlanta == id
                                           orderby Y.LoteComercial
                                           select new SelectLote
                                           {
                                               LoteComercial = Y.LoteComercial,
                                               IdOrdenProduccion = Y.IdOrdenProduccion
                                           }).ToList();
            return selectList;

        }

        public List<SelectLote> GetLotesBySacosDanados(int id)
        {
            List<SelectLote> selectList = (from X in dcSoftwareCalidad.CAL_OrdenProduccion
                                                     where X.Habilitado
                                                     && X.Habilitado
                                                     && (X.Autorizado.HasValue && X.Autorizado.Value)
                                                     && !X.Terminada
                                                     && X.IdPlanta == id
                                                     orderby X.LoteComercial
                                                     select new SelectLote
                                                     {
                                                         LoteComercial = X.LoteComercial,
                                                         IdOrdenProduccion = X.IdOrdenProduccion
                                                     }).ToList();
            return selectList;
        }
    

        public List<SelectDetalleOrdenProduccion> GetProductos(int id)
        {
            List<SelectDetalleOrdenProduccion> list = (from X in dcSoftwareCalidad.CAL_DetalleOrdenProduccion
                                                       where X.IdOrdenProduccion == id
                                                       && X.CAL_OrdenProduccion.Habilitado == true
                                                       orderby X.CAL_Subproducto.Nombre
                                                       select new SelectDetalleOrdenProduccion
                                                       {
                                                           IdDetalleOrdenProduccion = X.IdDetalleOrdenProduccion,
                                                           ProductoNombre = X.CAL_Subproducto.Nombre
                                                       }).ToList();
            return list;
        }

        public List<SelectProducto> GetProductosConDetalle(int id)
        {
            List<SelectProducto> list = (from X in dcSoftwareCalidad.CAL_Subproducto
                                         join Y in dcSoftwareCalidad.CAL_DetalleOrdenProduccion on X.IdSubproducto equals Y.IdSubproducto
                                         join Z in dcSoftwareCalidad.CAL_Producto on X.IdProducto equals Z.IdProducto
                                         where Y.IdOrdenProduccion == id
                                         orderby X.IdSubproducto
                                         select new SelectProducto
                                         {
                                             IdProducto = X.IdSubproducto,
                                             Nombre = X.Nombre,
                                             IdDetalleOrdenProduccion = Y.IdDetalleOrdenProduccion,
                                             FamiliaProducto = new SelectFamiliaProducto()
                                             {
                                                 IdFamiliaProducto = X.IdProducto,
                                                 Nombre = Z.Nombre,
                                             }
                                         }).ToList();

            return list;
        }

        public string GetReserva(int id)
        {
            CAL_OrdenProduccion ordenProduccion = dcSoftwareCalidad.CAL_OrdenProduccion.SingleOrDefault(X => X.IdOrdenProduccion == id && X.Habilitado == true);
            if (ordenProduccion != null)
            {
                return ordenProduccion.NumeroViaje;
            }

            return string.Empty;
        }


        #region --- FUNCIONES PRIVADAS ---

        public string GetIp()
        {
            return GetClientIp();
        }

        private string GetClientIp(HttpRequestMessage request = null)
        {
            request = request ?? Request;

            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return ((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress;
            }
            else if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                RemoteEndpointMessageProperty prop = (RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name];
                return prop.Address;
            }
            else if (HttpContext.Current != null)
            {
                return HttpContext.Current.Request.UserHostAddress;
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}
