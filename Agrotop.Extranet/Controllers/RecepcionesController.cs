using Agrotop.Extranet.Controllers.Filters;
using Agrotop.Extranet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Agrotop.Extranet.Controllers
{
    [ExtranetAuthorize]
    public class RecepcionesController : BaseController
    {
        //
        // GET: /Recepciones/

        public ActionResult Index(int? id, int? idCultivo)//, string key = "")
        {
            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);
            
            Temporada temporada = null;
            if (id.HasValue)
                temporada = temporadas.SingleOrDefault(t => t.IdTemporada == id.Value);
            
            if (temporada == null)
                temporada = temporadas.Single(t => t.Activa);
            
            if (!idCultivo.HasValue)
                idCultivo = 0;

            var recepciones = from pi in dc.ProcesoIngreso
                              where (pi.IdAgricultor == user.IdAgricultor)// || agricultor.IdsHijos().Contains(pi.IdAgricultor))
                                 && pi.IdTemporada == temporada.IdTemporada
                                 && pi.IdEstado != 99
                                 && (idCultivo.Value == 0 || pi.CultivoContrato.IdCultivo == idCultivo.Value)
                                 //&& (key == "" || pi.Agricultor.Nombre.Contains(key))
                              orderby pi.IdProcesoIngreso descending
                              select pi;


            ViewData["idCultivo"] = idCultivo.Value;
            ViewData["cultivos"] = dc.Cultivo.Where(c => (new int[] { 1, 2, 3 }).Contains(c.IdCultivo)).ToList();
            ViewData["temporadas"] = temporadas;
            ViewData["agricultor"] = agricultor;
            ViewData["temporada"] = temporada;
            ViewData["recepciones"] = recepciones;
            
            //ViewData["key"] = key;

            return View();
        }

        public ActionResult Detalle(int id)
        {
            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);
            var recepcion = (from pi in dc.ProcesoIngreso
                             where pi.IdProcesoIngreso == id
                                && (pi.IdAgricultor == user.IdAgricultor || agricultor.IdsHijos().Contains(pi.IdAgricultor))
                                && pi.IdEstado != 99
                             orderby pi.IdProcesoIngreso descending
                             select pi).SingleOrDefault();

            return View(recepcion);
        }
    }
}
