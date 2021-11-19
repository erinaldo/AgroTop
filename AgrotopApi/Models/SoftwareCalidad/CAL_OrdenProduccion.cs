using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace AgrotopApi.Models
{
    public partial class CAL_OrdenProduccion
    {
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();

        #region *** Getters ***

        public Cliente GetCliente(int IdCliente)
        {
            return dcAgroFichas.Cliente.SingleOrDefault(X => X.IdCliente == IdCliente);
        }

        public List<CAL_DetalleOrdenProduccion> GetDetalleOrdenProduccion()
        {
            return dcSoftwareCalidad.CAL_DetalleOrdenProduccion.Where(X => X.IdOrdenProduccion == this.IdOrdenProduccion).ToList();
        }

        public Pais GetPais(string PaisCodigo)
        {
            return dcAgroFichas.Pais.SingleOrDefault(X => X.PaisCodigo == PaisCodigo);
        }

        public SYS_User GetUser(string UserIns)
        {
            return dcAgroFichas.SYS_User.SingleOrDefault(X => X.UserName == UserIns);
        }

        #endregion

        #region *** Mailing ***

        public bool NotificarEdicion()
        {
            try
            {
                string destinatarios = "";
                var receptores = dcAgroFichas.ReceptoresNotificacionEdicionOrdenProduccion().ToList();

                if (receptores.Count == 0)
                    throw new Exception("No hay destinatarios disponibles para notificar la alerta");

                foreach (var receptor in receptores)
                {
                    var email = receptor.Email;
                    destinatarios += (destinatarios != "" ? ", " : "") + email;
                }

                string baseTemplate = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/editarordenproduccion_template.html"), Encoding.UTF8);
                Util.RepTempAngularStyle(ref baseTemplate, "LOTE", this.LoteComercial);
                Util.RepTempAngularStyle(ref baseTemplate, "DATETIME", string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Now));
                Util.RepTempAngularStyle(ref baseTemplate, "TIPO", this.CAL_TipoOrdenProduccion.Descripcion);
                Util.RepTempAngularStyle(ref baseTemplate, "FECHA", string.Format("{0:dd/MM/yyyy}", this.Fecha));
                Util.RepTempAngularStyle(ref baseTemplate, "EMBARCADOR", this.CAL_Exportador.Nombre);
                Util.RepTempAngularStyle(ref baseTemplate, "CONSIGNATARIO", this.GetCliente(this.IdCliente).RazonSocial);
                Util.RepTempAngularStyle(ref baseTemplate, "PAIS", this.GetPais(this.PaisCodigo).PaisNombreLocal);
                Util.RepTempAngularStyle(ref baseTemplate, "USER", this.GetUser(this.UserIns).FullName);
                Util.RepTempAngularStyle(ref baseTemplate, "USERUPDATE", this.GetUser(this.UserUpd).FullName);
                Util.RepTempAngularStyle(ref baseTemplate, "YEAR", DateTime.Now.Year.ToString());

                string subject = string.Format(string.Format("Se ha editado una orden de producción - N° {0}", this.LoteComercial));

                Util.SendMail(destinatarios, baseTemplate, subject);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}