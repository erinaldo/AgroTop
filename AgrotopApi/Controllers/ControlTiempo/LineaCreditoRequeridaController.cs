using AgrotopApi.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.ServiceModel.Channels;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace AgrotopApi.Controllers
{
    public class LineaCreditoRequeridaController : ApiController
    {
        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        // GET: LineaCreditoRequerida

        [HttpPost]
        public ResultLineaCreditoRequerida Aprobar(FormDataCollection formDataCollection)
        {
            ResultLineaCreditoRequerida result = new ResultLineaCreditoRequerida()
            {
                OK = true
            };

            if (!int.TryParse(formDataCollection.Get("id"), out int id))
            {
                result.OK = false;
                result.Error = "La planificación semanal no es válida";
                return result;
            }

            var planificacionSemanal = dc.CTR_PlanificacionSemanal.SingleOrDefault(X => X.IdPlanificacionSemanal == id);
            if (planificacionSemanal == null)
            {
                result.OK = false;
                result.Error = "No se ha encontrado la planificación semanal";
                result.Observacion = "";
            }

            if (result.OK)
            {
                try
                {
                    planificacionSemanal.LineaCreditoRechazada = false;
                    planificacionSemanal.UserUpd = User.Identity.Name;
                    planificacionSemanal.FechaHoraUpd = DateTime.Now;
                    planificacionSemanal.IpUpd = GetIp();
                    dc.SubmitChanges();

                    result.OK = true;
                    result.Error = "";
                    result.Observacion = "";
                }
                catch (Exception ex)
                {
                    result.OK = false;
                    result.Error = ex.Message;
                    result.Observacion = "";
                }
            }

            return result;
        }

        [HttpPost]
        public ResultLineaCreditoRequerida Rechazar(FormDataCollection formDataCollection)
        {
            ResultLineaCreditoRequerida result = new ResultLineaCreditoRequerida()
            {
                OK = true
            };

            if (!int.TryParse(formDataCollection.Get("id"), out int id))
            {
                result.OK = false;
                result.Error = "La planificación semanal no es válida";
                return result;
            }

            string observacion = "";
            if (string.IsNullOrEmpty(formDataCollection.Get("observacion")))
            {
                result.OK = false;
                result.Error = "La observación no debe estar vacía";
                return result;
            }
            else
            {
                observacion = formDataCollection.Get("observacion");
            }

            var planificacionSemanal = dc.CTR_PlanificacionSemanal.SingleOrDefault(X => X.IdPlanificacionSemanal == id);
            if (planificacionSemanal == null)
            {
                result.OK = false;
                result.Error = "No se ha encontrado la planificación semanal";
                result.Observacion = observacion;
            }

            if (result.OK)
            {
                try
                {
                    planificacionSemanal.LineaCreditoRechazada = true;
                    planificacionSemanal.ObservacionLineaCreditoRechazada = observacion;
                    planificacionSemanal.UserUpd = User.Identity.Name;
                    planificacionSemanal.FechaHoraUpd = DateTime.Now;
                    planificacionSemanal.IpUpd = GetIp();
                    dc.SubmitChanges();

                    result.OK = true;
                    result.Error = "";
                    result.Observacion = observacion;
                }
                catch (Exception ex)
                {
                    result.OK = false;
                    result.Error = ex.Message;
                    result.Observacion = observacion;
                }
            }

            return result;
        }

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
    }
}