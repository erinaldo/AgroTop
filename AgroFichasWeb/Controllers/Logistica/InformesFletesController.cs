using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Logistica
{
    public class InformesFletesController : BaseApplicationController
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();
        //
        // GET: /InformesFletes/

        public InformesFletesController()
        {
            SetCurrentModulo(5);
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CostoPorMes(int? año, string tramo, int? idTransportista)
        {
            CheckPermisoAndRedirect(131);

            if (año == null) { año = DateTime.Now.Year; }

            int origen = 0;
            int destino = 0;
            if (tramo != null && tramo.Contains(";"))
            {
                if (!int.TryParse(tramo.Split(new char[] { ';' })[0], out origen))
                    throw new HttpException((int)System.Net.HttpStatusCode.BadRequest, "El tramo seleccionado es inválido");

                if (!int.TryParse(tramo.Split(new char[] { ';' })[1], out destino))
                    throw new HttpException((int)System.Net.HttpStatusCode.BadRequest, "El tramo seleccionado es inválido");
            }

            List<rptl_InformeCostoPorMesResult> informe = null;
            if (origen == 0 || destino == 0)
                informe = dc.rptl_InformeCostoPorMes(año, null, null, idTransportista).ToList();
            else
                informe = dc.rptl_InformeCostoPorMes(año, origen, destino, idTransportista).ToList();

            ViewData["año"] = año;
            if (origen == 0)
                ViewData["origen"] = null;
            else
                ViewData["origen"] = origen;
            if (destino == 0)
                ViewData["destino"] = null;
            else
                ViewData["destino"] = destino;
            ViewData["idTransportista"] = idTransportista;

            SetAños(año);
            SetTramos(origen, destino);
            SetTransportistas(idTransportista);
            return View(informe);
        }

        public ActionResult CostoPorTramo(DateTime? fechaDesde, DateTime? fechaHasta, string tramo)
        {
            CheckPermisoAndRedirect(132);

            int origen = 0;
            int destino = 0;
            if (tramo != null && tramo.Contains(";"))
            {
                if (!int.TryParse(tramo.Split(new char[] { ';' })[0], out origen))
                    throw new HttpException((int)System.Net.HttpStatusCode.BadRequest, "El tramo seleccionado es inválido");

                if (!int.TryParse(tramo.Split(new char[] { ';' })[1], out destino))
                    throw new HttpException((int)System.Net.HttpStatusCode.BadRequest, "El tramo seleccionado es inválido");
            }

            List<rptl_InformeCostoPorTramoResult> informe = null;
            if (origen == 0 || destino == 0)
                informe = dc.rptl_InformeCostoPorTramo(fechaDesde, fechaHasta, null, null).ToList();
            else
                informe = dc.rptl_InformeCostoPorTramo(fechaDesde, fechaHasta, origen, destino).ToList();

            if (origen == 0)
                ViewData["origen"] = null;
            else
                ViewData["origen"] = origen;
            if (destino == 0)
                ViewData["destino"] = null;
            else
                ViewData["destino"] = destino;
            ViewData["fechaDesde"] = fechaDesde;
            ViewData["fechaHasta"] = fechaHasta;
            SetTramos(origen, destino);
            return View(informe);
        }

        public ActionResult MermasGlobal(DateTime? fechaDesde, DateTime? fechaHasta, string tramo, int? idChofer, int? idTransportista)
        {
            CheckPermisoAndRedirect(133);

            int origen = 0;
            int destino = 0;
            if (tramo != null && tramo.Contains(";"))
            {
                if (!int.TryParse(tramo.Split(new char[] { ';' })[0], out origen))
                    throw new HttpException((int)System.Net.HttpStatusCode.BadRequest, "El tramo seleccionado es inválido");

                if (!int.TryParse(tramo.Split(new char[] { ';' })[1], out destino))
                    throw new HttpException((int)System.Net.HttpStatusCode.BadRequest, "El tramo seleccionado es inválido");
            }

            List<rptl_InformeMermasGlobalResult> informe = null;
            if (origen == 0 || destino == 0)
                informe = dc.rptl_InformeMermasGlobal(fechaDesde, fechaHasta, null, null, idChofer, idTransportista).ToList();
            else
                informe = dc.rptl_InformeMermasGlobal(fechaDesde, fechaHasta, origen, destino, idChofer, idTransportista).ToList();

            if (origen == 0)
                ViewData["origen"] = null;
            else
                ViewData["origen"] = origen;
            if (destino == 0)
                ViewData["destino"] = null;
            else
                ViewData["destino"] = destino;
            ViewData["fechaDesde"] = fechaDesde;
            ViewData["fechaHasta"] = fechaHasta;
            ViewData["idChofer"] = idChofer;
            ViewData["idTransportista"] = idTransportista;
            SetChoferes(idChofer);
            SetTramos(origen, destino);
            SetTransportistas(idTransportista);
            return View(informe);
        }

        public ActionResult MermasTransportistas(DateTime? fechaDesde, DateTime? fechaHasta, string tramo, int? idChofer, int? idTransportista)
        {
            CheckPermisoAndRedirect(134);

            int origen = 0;
            int destino = 0;
            if (tramo != null && tramo.Contains(";"))
            {
                if (!int.TryParse(tramo.Split(new char[] { ';' })[0], out origen))
                    throw new HttpException((int)System.Net.HttpStatusCode.BadRequest, "El tramo seleccionado es inválido");

                if (!int.TryParse(tramo.Split(new char[] { ';' })[1], out destino))
                    throw new HttpException((int)System.Net.HttpStatusCode.BadRequest, "El tramo seleccionado es inválido");
            }

            List<rptl_InformeMermasTransportistasResult> informe = null;
            if (origen == 0 || destino == 0)
                informe = dc.rptl_InformeMermasTransportistas(fechaDesde, fechaHasta, null, null, idChofer, idTransportista).ToList();
            else
                informe = dc.rptl_InformeMermasTransportistas(fechaDesde, fechaHasta, origen, destino, idChofer, idTransportista).ToList();

            if (origen == 0)
                ViewData["origen"] = null;
            else
                ViewData["origen"] = origen;
            if (destino == 0)
                ViewData["destino"] = null;
            else
                ViewData["destino"] = destino;
            ViewData["fechaDesde"] = fechaDesde;
            ViewData["fechaHasta"] = fechaHasta;
            ViewData["idChofer"] = idChofer;
            ViewData["idTransportista"] = idTransportista;
            SetChoferes(idChofer);
            SetTramos(origen, destino);
            SetTransportistas(idTransportista);
            return View(informe);
        }

        public ActionResult MermasChoferesPorTransportista(int? idTransportista, DateTime? fechaDesde, DateTime? fechaHasta, string tramo, int? idChofer)
        {
            CheckPermisoAndRedirect(135);

            int origen = 0;
            int destino = 0;
            if (tramo != null && tramo.Contains(";"))
            {
                if (!int.TryParse(tramo.Split(new char[] { ';' })[0], out origen))
                    throw new HttpException((int)System.Net.HttpStatusCode.BadRequest, "El tramo seleccionado es inválido");

                if (!int.TryParse(tramo.Split(new char[] { ';' })[1], out destino))
                    throw new HttpException((int)System.Net.HttpStatusCode.BadRequest, "El tramo seleccionado es inválido");
            }

            List<rptl_InformeMermasChoferesPorTransportistaResult> informe = null;
            if (origen == 0 || destino == 0)
                informe = dc.rptl_InformeMermasChoferesPorTransportista(idTransportista, fechaDesde, fechaHasta, null, null, idChofer).ToList();
            else
                informe = dc.rptl_InformeMermasChoferesPorTransportista(idTransportista, fechaDesde, fechaHasta, origen, destino, idChofer).ToList();

            if (origen == 0)
                ViewData["origen"] = null;
            else
                ViewData["origen"] = origen;
            if (destino == 0)
                ViewData["destino"] = null;
            else
                ViewData["destino"] = destino;
            ViewData["fechaDesde"] = fechaDesde;
            ViewData["fechaHasta"] = fechaHasta;
            ViewData["idChofer"] = idChofer;
            ViewData["idTransportista"] = idTransportista;
            SetChoferes(idChofer);
            SetTramos(origen, destino);
            SetTransportistas(idTransportista);
            return View(informe);
        }

        public ActionResult SeguimientoInterno(int? idRequerimiento, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            CheckPermisoAndRedirect(137);

            List<rptl_InformeSeguimientoInternoResult> informe = dc.rptl_InformeSeguimientoInterno(idRequerimiento, fechaDesde, fechaHasta).ToList();
            ViewData["idRequerimiento"] = idRequerimiento;
            ViewData["fechaDesde"] = fechaDesde;
            ViewData["fechaHasta"] = fechaHasta;
            SetRequerimientos(idRequerimiento);
            return View(informe);
        }

        public ActionResult AlertasPorTransportistas(int? idTransportista, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            CheckPermisoAndRedirect(138);

            List<rptl_InformeAlertasResult> informe = dc.rptl_InformeAlertas(idTransportista, fechaDesde, fechaHasta).ToList();
            ViewData["idTransportista"] = idTransportista;
            ViewData["fechaDesde"] = fechaDesde;
            ViewData["fechaHasta"] = fechaHasta;
            SetTransportistas(idTransportista);
            return View(informe);
        }

        public ActionResult TotalFacturado(int? año, int? idCultivo, int? idTransportista, DateTime? fechaDesde, DateTime? fechaHasta, int? idTipoMovimiento)
        {
            CheckPermisoAndRedirect(136);

            if (!año.HasValue)
                año = DateTime.Now.Year;

            List<lgt_TotalFacturadoResult> informe = dc.lgt_TotalFacturado(año, idTransportista, idCultivo, fechaDesde, fechaHasta, idTipoMovimiento).ToList();
            ViewData["año"] = idTransportista;
            ViewData["idCultivo"] = idCultivo;
            ViewData["idTransportista"] = idTransportista;
            ViewData["fechaDesde"] = fechaDesde;
            ViewData["fechaHasta"] = fechaHasta;
            ViewData["idTipoMovimiento"] = idTipoMovimiento;
            SetAños(año);
            SetCultivos(idCultivo);
            SetTransportistas(idTransportista);
            SetTipoMovimientos(idTipoMovimiento);
            return View(informe);
        }

        public ActionResult MovimientosInternos(DateTime? fechaDesde, DateTime? fechaHasta, int? idCultivo, int? idEmpresa)
        {
            CheckPermisoAndRedirect(222);

            dc.CommandTimeout = 3 * 60;

            List<rpt_LOG_MovimientosInternosResult> informe = dc.rpt_LOG_MovimientosInternos(fechaDesde, fechaHasta, idCultivo, idEmpresa).ToList();

            var idCultivoSelect = idCultivo ?? 0;
            var idEmpresaSelect = idEmpresa ?? 0;

            ViewData["fechaDesde"] = fechaDesde;
            ViewData["fechaHasta"] = fechaHasta;
            ViewData["idCultivo"] = idCultivoSelect;
            ViewData["idEmpresa"] = idEmpresaSelect;
            ViewData["empresas"] = dc.Empresa.ToList();
            SetCultivos(idCultivo);

            return View(informe);
        }

        private void SetAños(int? año)
        {
            IEnumerable<SelectListItem> selectList =
                from s in dc.rptl_ObtenerAños()
                orderby s.Año
                select new SelectListItem
                {
                    Selected = (s.Año == año && año != null),
                    Text = s.Año.ToString(),
                    Value = s.Año.ToString()
                };
            ViewData["añosList"] = selectList;
        }

        private void SetChoferes(int? IdChofer = null)
        {
            IEnumerable<SelectListItem> selectList =
                from s in dc.LOG_Chofer
                where s.Habilitado == true
                orderby s.Nombre
                select new SelectListItem
                {
                    Selected = (s.IdChofer == IdChofer && IdChofer != null),
                    Text = s.Nombre,
                    Value = s.IdChofer.ToString()
                };
            ViewData["choferesList"] = selectList;
        }

        private void SetCultivos(int? IdCultivo)
        {
            IEnumerable<SelectListItem> selectList =
                from s in dc.Cultivo
                where s.Habilitado == true
                orderby s.Nombre
                select new SelectListItem
                {
                    Selected = (s.IdCultivo == IdCultivo && IdCultivo != null),
                    Text = s.Nombre,
                    Value = s.IdCultivo.ToString()
                };
            ViewData["cultivosList"] = selectList;
        }

        private void SetTramos(int? origen, int? destino)
        {
            IEnumerable<SelectListItem> selectList =
                from s in dc.rptl_ObtenerTramos()
                orderby s.strNombreSucursalOrigen, s.strNombreSucursalDestino
                select new SelectListItem
                {
                    Selected = ((s.Origen == origen && s.Destino == destino) && (origen != null && destino != null)),
                    Text = string.Format("{0} {1}-{2} {3}", s.strNombreSucursalOrigen, s.strNombreCortoBodegaOrigen, s.strNombreSucursalDestino, s.strNombreCortoBodegaDestino),
                    Value = string.Format("{0};{1}", s.Origen, s.Destino)
                };
            ViewData["tramosList"] = selectList;
        }

        private void SetTransportistas(int? IdTransportista)
        {
            IEnumerable<SelectListItem> selectList =
                from s in dc.LOG_Transportista
                where s.Habilitado == true
                orderby s.Nombre
                select new SelectListItem
                {
                    Selected = (s.IdTransportista == IdTransportista && IdTransportista != null),
                    Text = s.Nombre,
                    Value = s.IdTransportista.ToString()
                };
            ViewData["transportistasList"] = selectList;
        }

        private void SetTipoMovimientos(int? idTipoMovimiento)
        {
            IEnumerable<SelectListItem> selectList =
                from s in dc.LOG_TipoMovimiento
                orderby s.Descripcion
                select new SelectListItem
                {
                    Selected = (s.IdTipoMovimiento == idTipoMovimiento && idTipoMovimiento != null),
                    Text = s.Descripcion,
                    Value = s.IdTipoMovimiento.ToString()
                };
            ViewData["tipoMovimientosList"] = selectList;
        }

        private void SetRequerimientos(int? IdRequerimiento)
        {
            IEnumerable<SelectListItem> selectList =
                from s in dc.LOG_Requerimiento
                where s.IdEstado != 99
                orderby s.IdRequerimiento
                select new SelectListItem
                {
                    Selected = (s.IdRequerimiento == IdRequerimiento && IdRequerimiento != null),
                    Text = string.Format("{0}: {1}", s.IdRequerimiento, s.Glosa),
                    Value = s.IdRequerimiento.ToString()
                };
            ViewData["requerimientosList"] = selectList;
        }
    }
}
