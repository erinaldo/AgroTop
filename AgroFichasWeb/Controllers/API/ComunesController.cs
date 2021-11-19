using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.Models.API.Comunes;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;

namespace AgroFichasWeb.Controllers.API
{
    [WebsiteAuthorize]
    public class ComunesController : ApiController
    {
        Models.AgroFichasDBDataContext context = new Models.AgroFichasDBDataContext();

        [HttpGet]
        public JsonResult<List<ResponseRegion>> GetRegiones()
        {
            List<ResponseRegion> responseList = new List<ResponseRegion>();
            List<Region> regions = context.Region.OrderBy(r => r.Orden).ToList();

            foreach (Region region in regions)
            {
                responseList.Add(new ResponseRegion()
                {
                    IdRegion = region.IdRegion,
                    Nombre = region.Nombre
                });
            }

            return Json(responseList);
        }

        [HttpGet]
        public JsonResult<List<ResponseProvincia>> GetProvincias([FromUri] RequestProvincia request)
        {
            List<ResponseProvincia> responseList = new List<ResponseProvincia>();
            if (request != null)
            {
                List<Provincia> provincias = context.Provincia.Where(p => p.IdRegion == request.IdRegion).OrderBy(p => p.Nombre).ToList();

                foreach (Provincia provincia in provincias)
                {
                    responseList.Add(new ResponseProvincia()
                    {
                        IdProvincia = provincia.IdProvincia,
                        Nombre = provincia.Nombre
                    });
                }
            }
            return Json(responseList);
        }

        [HttpGet]
        public JsonResult<List<ResponseComuna>> GetComunas([FromUri] RequestComuna request)
        {
            List<ResponseComuna> responseList = new List<ResponseComuna>();
            if (request != null)
            {
                List<Comuna> comunas = context.Comuna.Where(c => c.IdProvincia == request.IdProvincia).OrderBy(c => c.Orden).ToList();

                foreach (var comuna in comunas)
                {
                    responseList.Add(new ResponseComuna()
                    {
                        IdComuna = comuna.IdComuna,
                        Nombre = comuna.Nombre
                    });
                }
            }
            return Json(responseList);
        }

        [HttpGet]
        public JsonResult<List<ResponseTituloExplotacion>> GetTituloExplotacion()
        {
            List<ResponseTituloExplotacion> responseList = new List<ResponseTituloExplotacion>();
            List<TituloExplotacion> tituloExplotacions = context.TituloExplotacion.OrderBy(te => te.IdTituloExplotacion).ToList();

            foreach (TituloExplotacion tituloExplotacion in tituloExplotacions)
            {
                responseList.Add(new ResponseTituloExplotacion()
                {
                    IdTituloExplotacion = tituloExplotacion.IdTituloExplotacion,
                    Nombre = tituloExplotacion.Nombre
                });
            }
            return Json(responseList);
        }

        [HttpGet]
        public JsonResult<List<ResponseEmpresa>> GetEmpresas()
        {
            List<ResponseEmpresa> responseList = new List<ResponseEmpresa>();
            List<Empresa> empresas = context.Empresa.OrderBy(e => e.IdEmpresa).ToList();

            foreach (Empresa empresa in empresas)
            {
                responseList.Add(new ResponseEmpresa()
                {
                    IdEmpresa = empresa.IdEmpresa,
                    Nombre = empresa.Nombre
                });
            }
            return Json(responseList);
        }

        [HttpGet]
        public JsonResult<List<ResponseGastosTransportePara>> GetGastosTransportePara()
        {
            List<ResponseGastosTransportePara> responseList = new List<ResponseGastosTransportePara>();

            responseList.Add(new ResponseGastosTransportePara()
            {
                IdGastosTransportePara = 1,
                Nombre = "EMPRESA"
            });

            responseList.Add(new ResponseGastosTransportePara()
            {
                IdGastosTransportePara = 2,
                Nombre = "PRODUCTOR"
            });

            return Json(responseList);
        }
    }
}