using AgroFichasLib;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace AgroFichasApi.Models
{
    public partial class FichaPreSiembra : IBaseEntity, ISynSerializable, IFichaNotificable
    {
        public static string XmlTag = "fps";

        public string[] SyncProperties
        {
            get { return new[] { "IdPredio", "IdTemporada", "IdEstadoSiembra", "IdImportanciaSeguimiento", "Fecha", "Observaciones", "ID" }; }
            set { }
        }

        public int ID
        {
            get { return this.IdFichaPreSiembra; }
        }

        public string Hash
        {
            get
            {
                return AgroFichasLib.FichaPreSiembra.Hash(this.IdFichaPreSiembra, this.IdPredio, this.UserIns);
            }
        }

        public string Hash2
        {
            get
            {
                return AgroFichasLib.FichaPreSiembra.Hash2(this.IdFichaPreSiembra, this.IdPredio);
            }
        }


        public string PdfUrl
        {
            get
            {
                return string.Format("{0}/documents/FichaPreSiembra/{1}?h={2}", Properties.Settings.Default.WebsiteUrl, this.IdFichaPreSiembra, this.Hash);
            }
        }

        public string SendUrl
        {
            get
            {
                return string.Format("{0}/agroapi/sendFichaPreSiembra/{1}?h={2}", Properties.Settings.Default.ApiUrl, this.IdFichaPreSiembra, this.Hash2);
            }
        }

        public string NombrePredio
        {
            get
            {
                return this.Predio.Nombre;
            }
        }

        public string NombreAgricultor
        {
            get
            {
                return this.Predio.Agricultor.Nombre;
            }
        }

        public string EmailAgricultor
        {
            get
            {
                return this.Predio.Agricultor.Email;
            }
        }

        public List<string> GetDestinatariosFicha()
        {
            var dc = new AgroFichasDBDataContext();
            return dc.GetDestinatariosFichaPreSiembra(this.IdFichaPreSiembra).Select(d => d.Email).ToList();
        }

        public System.Xml.Linq.XElement Serialize()
        {
            var element = new System.Xml.Linq.XElement(FichaPreSiembra.XmlTag);
            element.SetAttributeValue("id", this.IdFichaPreSiembra);
            element.SetAttributeValue("idpr", this.IdPredio);
            element.SetAttributeValue("idtm", this.IdTemporada);
            element.SetAttributeValue("f", this.Fecha);
            element.SetAttributeValue("ob", this.Observaciones);
            element.SetAttributeValue("ides", this.IdEstadoSiembra);
            element.SetAttributeValue("idis", this.IdImportanciaSeguimiento);


            return element;
        }

        public void Update(XmlNode node, string username)
        {
            this.Fecha = DateTime.Parse(node.Attributes["f"].Value);
            this.Observaciones = node.Attributes["ob"].Value ?? "";

            var estadoSiembraNode = node.Attributes["ides"];
            this.IdEstadoSiembra = estadoSiembraNode != null ? int.Parse(estadoSiembraNode.Value) : 5;

            var importanciaNode = node.Attributes["idis"];
            this.IdImportanciaSeguimiento = importanciaNode != null ? int.Parse(importanciaNode.Value) : 20;

            this.UserUpd = username;
            this.IpUpd = Utils.RemoteAddr();
            this.FechaHoraUpd = DateTime.Now;
        }

        public List<XElement> OnSubmitChanges(XmlNode node, AgroFichasDBDataContext dc, string username, bool isNew)
        {
            //Retornamos lista de equivalencias de ids
            var result = new List<XElement>();

            //Eliminamos los Potreros
            var currentPotreros = dc.FichaPreSiembraPotrero.Where(fp => fp.IdFichaPreSiembra == this.IdFichaPreSiembra);
            dc.FichaPreSiembraPotrero.DeleteAllOnSubmit(currentPotreros);
            dc.SubmitChanges(); //Must submit to delete before inserting to avoid duplicate keys

            //Creamos los nuevos potreros
            string pots = node.Attributes["pots"].Value;
            if (pots == "")
                return result;

            string[] pairs = pots.Split(',');
            foreach (var pair in pairs)
            {
                string[] ids = pair.Split('@');
                int idPotrero = int.Parse(ids[0]);
                string lidPotrero = ids[1];
                int id = int.Parse(ids[2]);

                if (idPotrero < 0)
                    idPotrero = dc.GetTable<Potrero>().FromMobileTag(lidPotrero).IdPotrero;

                var fp = new FichaPreSiembraPotrero()
                {
                    IdPotrero = idPotrero,
                    MobileTag = "",
                    UserIns = username,
                    IpIns = Utils.RemoteAddr(),
                    FechaHoraIns = DateTime.Now
                };

                this.FichaPreSiembraPotrero.Add(fp);
                dc.SubmitChanges();

                result.Add(Repository.GetDifferentialResponseElement(Models.FichaPreSiembraPotrero.XmlTag, fp.IdFichaPreSiembraPotrero, id));
            }

            //Notificamos
            if (isNew)
            {
                this.NotifyCreator();
            }

            return result;
        }

        public static FichaPreSiembra CreateNew(string mobileTag, int idPredio, int idTemporada, string username)
        {
            return new FichaPreSiembra()
            {
                IdPredio = idPredio,
                IdTemporada = idTemporada,
                MobileTag = mobileTag,
                UserIns = username,
                IpIns = Utils.RemoteAddr(),
                FechaHoraIns = DateTime.Now
            };
        }


        /* EMAIL NOTIFICATION
         * *****************************************************/
        public void NotifyCreator()
        {
            try
            {
                var mailer = new FichaMailer();
                mailer.NotifyCreator(this);
            }
            catch (Exception ex)
            {
                var s = "";
                s += "DateTime: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + "\r\n";
                s += "IdFichaPreSiembra: " + this.IdFichaPreSiembra + "\r\n";
                s += ex.ToString();
                System.IO.File.WriteAllText(HttpContext.Current.Server.MapPath("~/App_Data/notifications/errors/") + System.Guid.NewGuid() + ".txt", s);
            }
        }

        public OperationResult Notify()
        {
            try
            {
                if (String.IsNullOrEmpty(this.Predio.Agricultor.Email))
                {
                    return new OperationResult(false, "No es posible enviar el correo por que el agricultor no tiene email registrado.", "501", null);
                }

                var mailer = new FichaMailer();
                mailer.NotifyAgricultor(this);

                return new OperationResult(true, "", "200", null);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message, "501", ex);
            }
        }
    }
}