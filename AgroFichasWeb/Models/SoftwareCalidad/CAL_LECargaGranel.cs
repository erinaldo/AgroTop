using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CAL_LECargaGranel
    {
        #region 1. DataContexts
        private SoftwareCalidadDBDataContext dcSoftwareCalidad = new SoftwareCalidadDBDataContext();
        private AgroFichasDBDataContext dcAgroFichas = new AgroFichasDBDataContext();
        #endregion
        #region 2. Propiedades

        #endregion
        #region 3. Funciones
        public List<CAL_Metodologia> GetMetodologias()
        {
            return dcSoftwareCalidad.CAL_Metodologia.Where(X => X.Habilitado == true).OrderBy(X => X.Analisis).ToList();
        }

        public SYS_User GetUser(string UserIns)
        {
            return dcAgroFichas.SYS_User.SingleOrDefault(X => X.UserName == UserIns);
        }
        public List<CAL_GetTamañoContenedoresParaListaEmpaqueResult> GetTamañoContenedores()
        {
            return dcSoftwareCalidad.CAL_GetTamañoContenedoresParaListaEmpaque(this.IdOrdenProduccion).ToList();
        }

        public List<CAL_CertificadoCargaGranel> GetCertificados(int id)
        {
            List<CAL_CertificadoCargaGranel> selectList = (from X in dcSoftwareCalidad.CAL_LECargaGranel
                                                      join Y in dcSoftwareCalidad.CAL_CertificadoCargaGranel on X.IdLECargaGranel equals Y.IdLECargaGranel
                                                      where X.Habilitado
                                                      && Y.Habilitado
                                                      && X.IdLECargaGranel == id
                                                      orderby Y.IdCertificadoCargaGranel
                                                      select Y).ToList();
            return selectList;
        }
        #endregion
        #region 4. SelectLists
        public IEnumerable<SelectListItem> GetCarriers(int? IdCarrier)
        {
            IEnumerable<SelectListItem> selectList = (from X in dcSoftwareCalidad.Carrier
                                                      orderby X.Nombre
                                                      select new SelectListItem
                                                      {
                                                          Selected = (X.IdCarrier == IdCarrier && IdCarrier != null),
                                                          Text = X.Nombre,
                                                          Value = X.IdCarrier.ToString()
                                                      });
            return selectList;
        }



        public IEnumerable<SelectListItem> GetLotes(int? IdOrdenProduccion)
        {
            IEnumerable<SelectListItem> selectList = (from X in dcSoftwareCalidad.CAL_DespachoCargaGranel
                                                      join Y in dcSoftwareCalidad.CAL_OrdenProduccion on X.IdOrdenProduccion equals Y.IdOrdenProduccion
                                                      where X.Habilitado
                                                      && Y.Habilitado
                                                      && (Y.Autorizado.HasValue && Y.Autorizado.Value)
                                                      && !Y.Terminada
                                                      orderby Y.LoteComercial
                                                      select new SelectListItem
                                                      {
                                                          Selected = (Y.IdOrdenProduccion == IdOrdenProduccion && IdOrdenProduccion != null),
                                                          Text = Y.LoteComercial,
                                                          Value = Y.IdOrdenProduccion.ToString()
                                                      }).DistinctBy(X => X.Value);
            return selectList;
        }
        public IEnumerable<SelectListItem> GetPlantaProduccion()
        {
            IEnumerable<SelectListItem> selectList = from pp in dcAgroFichas.PlantaProduccion
                                                     select new SelectListItem
                                                     {
                                                         Value = pp.IdPlantaProduccion.ToString(),
                                                         Text = pp.Nombre
                                                     };
            return selectList;
        }
        #endregion
        #region 5. Validaciones
        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (this.IdOrdenProduccion == 0)
                yield return new RuleViolation("El lote comercial es requerido", "IdOrdenProduccion");

            if (this.IdCarrier == 0)
                yield return new RuleViolation("El carrier es requerido", "IdCarrier");

            if (this.IdBarco == 0)
                yield return new RuleViolation("El barco es requerido", "IdBarco");

            if (string.IsNullOrEmpty(this.NReserva))
                yield return new RuleViolation("El reserva es requerido", "NReserva");

            if (string.IsNullOrEmpty(this.PuertoEmbarque))
                yield return new RuleViolation("El puerto de embarque es requerido", "PuertoEmbarque");

            if (string.IsNullOrEmpty(this.PuertoDestino))
                yield return new RuleViolation("El puerto de destino es requerido", "PuertoDestino");

            if (string.IsNullOrEmpty(this.DUS))
                yield return new RuleViolation("El declaración única de salida es requerido", "DUS");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }
        #endregion
        #region 6. Mailing
        public bool NotificarCreacion()
        {
            try
            {
                string destinatarios = "";
                var receptores = dcAgroFichas.ReceptoresNotificacionCreacionListaEmpaque().ToList();

                if (receptores.Count == 0)
                    throw new Exception("No hay destinatarios disponibles para notificar la alerta");

                foreach (var receptor in receptores)
                {
                    var email = receptor.Email;
                    destinatarios += (destinatarios != "" ? ", " : "") + email;
                }

                string baseTemplate = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/crearlistaempaque_template.html"), Encoding.UTF8);
                Util.RepTempAngularStyle(ref baseTemplate, "CODIGO", string.Format("{0}", this.IdLECargaGranel));
                Util.RepTempAngularStyle(ref baseTemplate, "DATETIME", string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Now));
                Util.RepTempAngularStyle(ref baseTemplate, "LOTE", this.CAL_OrdenProduccion.LoteComercial);
                Util.RepTempAngularStyle(ref baseTemplate, "TRANSPORTE", string.Format("{0}, {1}", this.Carrier.Nombre, this.Barco.Nombre));
                Util.RepTempAngularStyle(ref baseTemplate, "DUS", this.DUS);
                Util.RepTempAngularStyle(ref baseTemplate, "NRESERVA", this.NReserva);
                Util.RepTempAngularStyle(ref baseTemplate, "USER", this.GetUser(this.UserIns).FullName);
                Util.RepTempAngularStyle(ref baseTemplate, "YEAR", DateTime.Now.Year.ToString());
                Util.RepTempAngularStyle(ref baseTemplate, "TIPO", string.Format("Granel"));

                string subject = string.Format(string.Format("Se ha creado una nueva lista de empaque - Lote {0}", this.CAL_OrdenProduccion.LoteComercial));

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