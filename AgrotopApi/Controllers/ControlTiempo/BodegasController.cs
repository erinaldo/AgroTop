using AgrotopApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace AgrotopApi.Controllers.ControlTiempo
{
    public class BodegasController : ApiController
    {
        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public List<SelectBodega> GetBodegas(int id)
        {
            List<SelectBodega> list = new List<SelectBodega>();
            Empresa objEmpresa = dc.Empresa.Where(c => c.IdEmpresa == id).First();
            List<Bodega> bodegaList = new List<Bodega>();
            switch (objEmpresa.IdEmpresa)
            {
                case 1:
                    bodegaList = (from X in dc.Bodega where X.IDOleotop != "" select X).OrderBy(X => X.IdBodega).ToList();
                    break;
                case 2:
                    bodegaList = (from X in dc.Bodega where X.IDAvenatop != "" select X).OrderBy(X => X.IdBodega).ToList();
                    break;
                case 3:
                    bodegaList = (from X in dc.Bodega where X.IDGranotop != "" select X).OrderBy(X => X.IdBodega).ToList();
                    break;
                case 4:
                    bodegaList = (from X in dc.Bodega where X.IDSaprosem != "" select X).OrderBy(X => X.IdBodega).ToList();
                    break;
            }

            foreach (var bodega in bodegaList)
            {
                list.Add(new SelectBodega()
                {
                    IdBodega = bodega.IdBodega,
                    Nombre = bodega.Nombre
                });
            }

            return list;
        }

    }
}
