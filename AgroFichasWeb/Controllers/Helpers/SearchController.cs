using AgroFichasWeb.Controllers.Filters;
using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers
{
    [WebsiteAuthorize]
    public class SearchController : BaseApplicationController
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        [HttpPost]
        public JsonResult AgricultorFinderSearch(string keyword, int type, int idAgricultorArgument)
        {
            var result = new List<Object>();

            if (type == (int)AgriculorSearchType.ParaRelacionado)
            {
                result.AddRange(from ag in dc.AgricultoresRelacionadosDisponibles(idAgricultorArgument, keyword)
                                select new
                                {
                                    IdAgricultor = ag.IdAgricultor,
                                    Rut = ag.Rut,
                                    Nombre = ag.Nombre
                                });
            }

            return Json(result);
        }

        [HttpPost]
        public JsonResult AgricultorSelectorSearch(string keyword)
        {
            var result = new List<Object>();
            var rut = keyword.Trim().Replace(".", "").Replace(",", "").Replace(" ", "").ToUpper();

            var agricultores = from ag in dc.Agricultor
                               where (ag.Nombre.Contains(keyword) ||
                                      ag.Rut.ToUpper().Contains(rut))
                                  && ag.Habilitado
                               orderby ag.Nombre
                               select ag;

            foreach (var agricultor in agricultores)
            {
                result.Add(new
                {
                    IdAgricultor = agricultor.IdAgricultor,
                    Rut = agricultor.Rut,
                    Nombre = agricultor.Nombre
                });
            }

            return Json(result);
        }

        [HttpPost]
        public JsonResult ChoferSelectorSearch(int IdTransportista, string keyword)
        {
            var result = new List<Object>();
            var rut = keyword.Trim().Replace(".", "").Replace(",", "").Replace(" ", "").ToUpper();

            var choferes = from ch in dc.LOG_Chofer
                           where (ch.Nombre.Contains(keyword) ||
                                  ch.RUT.ToUpper().Contains(rut))
                              && ch.IdTransportista == IdTransportista
                              && ch.Habilitado
                           orderby ch.Nombre
                           select ch;

            foreach (var chofer in choferes)
            {
                result.Add(new
                {
                    IdTransportista = chofer.IdTransportista,
                    IdChofer = chofer.IdChofer,
                    RUT = chofer.RUT,
                    Nombre = chofer.Nombre
                });
            }

            return Json(result);
        }

        [HttpPost]
        public JsonResult TransportistaSelectorSearch(string keyword)
        {
            var result = new List<Object>();
            var rut = keyword.Trim().Replace(".", "").Replace(",", "").Replace(" ", "").ToUpper();

            var transportistas = from tr in dc.LOG_Transportista
                               where (tr.Nombre.Contains(keyword) ||
                                      tr.RUT.ToUpper().Contains(rut))
                                  && tr.Habilitado
                               orderby tr.Nombre
                               select tr;

            foreach (var transportista in transportistas)
            {
                result.Add(new
                {
                    IdTransportista = transportista.IdTransportista,
                    RUT = transportista.RUT,
                    Nombre = transportista.Nombre
                });
            }

            return Json(result);
        }
    }
}
