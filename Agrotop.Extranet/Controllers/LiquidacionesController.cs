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
    public class LiquidacionesController : BaseController
    {
        //
        // GET: /Liquidaciones/

        public ActionResult Index(int? id, int? idEmpresa)
        {
            var temporadas = dc.Temporada.OrderByDescending(t => t.IdTemporada);
            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);

            Temporada temporada = null;
            if (id.HasValue)
                temporada = temporadas.SingleOrDefault(t => t.IdTemporada == id.Value);

            if (temporada == null)
                temporada = temporadas.Single(t => t.Activa);

            if (!idEmpresa.HasValue)
                idEmpresa = 0;

            var liquidaciones = from li in dc.Liquidacion
                              where (li.IdAgricultor == user.IdAgricultor || agricultor.IdsHijos().Contains(li.IdAgricultor))
                                 && li.IdTemporada == temporada.IdTemporada
                                 && li.IdEstado == 3 //Autorizada
                                 && (idEmpresa.Value == 0 || li.IdEmpresa == idEmpresa)
                              orderby li.IdLiquidacion descending
                              select li;

            ViewData["idEmpresa"] = idEmpresa.Value;
            ViewData["empresas"] = dc.Empresa.ToList();
            ViewData["temporadas"] = temporadas;
            ViewData["agricultor"] = agricultor;
            ViewData["temporada"] = temporada;
            ViewData["liquidaciones"] = liquidaciones;

            return View();
        }

        public ActionResult Detalle(int id)
        {
            var agricultor = dc.Agricultor.Single(a => a.IdAgricultor == user.IdAgricultor);

            var liquidacion   = (from li in dc.Liquidacion
                                where li.IdLiquidacion == id
                                   && (li.IdAgricultor == user.IdAgricultor || agricultor.IdsHijos().Contains(li.IdAgricultor))
                                   && li.IdEstado == 3 //Autorizada
                                orderby li.IdLiquidacion descending
                                select li).SingleOrDefault();

            return View(liquidacion);
        }

    }
}
