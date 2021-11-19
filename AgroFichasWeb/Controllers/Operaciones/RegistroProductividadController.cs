using AgroFichasWeb.Models;
using AgroFichasWeb.ViewModels.Operaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers.Operaciones
{
    public class RegistroProductividadController : BaseApplicationController
    {
        OperacionesDBDataContext dc = new OperacionesDBDataContext();
        //
        // GET: /RegistroProductividad/

        public RegistroProductividadController()
        {
            SetCurrentModulo(7);
        }

        public ActionResult Index(int id)
        {
            CheckPermisoAndRedirect(213);
            var registroTurno = dc.OPR_RegistroTurno.SingleOrDefault(X => X.IdRegistroTurno == id && X.Habilitado == true);
            if (registroTurno == null)
                throw new HttpException(404, "No se ha encontrado el registro del turno");

            var model = new RegistroProductividadViewModel();
            model.RegistroTurno = registroTurno;
            model.ProduccionTurno = dc.rpt_OPR_ProduccionTurno(registroTurno.IdRegistroTurno, registroTurno.OPR_Turno.Horas).ToList();
            model.ProduccionTurnoEstabilizado = dc.rpt_OPR_ProduccionTurnoEstabilizado(registroTurno.IdRegistroTurno, registroTurno.OPR_Turno.Horas).ToList();
            model.RegistroEnvasados = dc.OPR_RegistroEnvasado.Where(X => X.IdRegistroTurno == registroTurno.IdRegistroTurno && X.Habilitado == true).OrderBy(X => X.OPR_Producto.OPR_TipoProducto.IdTipoProducto).ThenBy(X => X.OPR_Producto.Descripcion).ToList();
            model.RegistroGeneracionMaxisacos = dc.OPR_RegistroGeneracionMaxisacos.Where(X => X.IdRegistroTurno == registroTurno.IdRegistroTurno && X.Habilitado == true).ToList();

            model.TurnoAnteriorSiguiente = dc.OPR_GetTurnoAnteriorSiguiente(model.RegistroTurno.Correlativo).SingleOrDefault();

            return View(model);
        }
    }
}