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
    public partial class Ficha : IBaseEntity, ISynSerializable, IFichaNotificable
    {
        public static string XmlTag = "fi";

        public string[] SyncProperties
        {
            get { return new[] { "IdPredio", "IdTipoFicha", "IdEstadoSiembra", "IdImportanciaSeguimiento", "IdTemporada", "Fecha", "Observaciones", "ID" }; }
            set { }
        }

        public int ID
        {
            get { return this.IdFicha; }
        }

        public string Hash
        {
            get
            {
                return AgroFichasLib.Ficha.Hash(this.IdFicha, this.IdPredio, this.UserIns);
            }
        }

        public string Hash2
        {
            get
            {
                return AgroFichasLib.Ficha.Hash2(this.IdFicha, this.IdPredio);
            }
        }

        public string PdfUrl
        {
            get
            {
                return string.Format("{0}/documents/ficha/{1}?h={2}", Properties.Settings.Default.WebsiteUrl, this.IdFicha, this.Hash);
            }
        }

        public string SendUrl
        {
            get
            {

                return string.Format("{0}/agroapi/sendficha/{1}?h={2}", Properties.Settings.Default.ApiUrl, this.IdFicha, this.Hash2);
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
            return dc.GetDestinatariosFicha(this.IdFicha).Select(d => d.Email).ToList();
        }

        public System.Xml.Linq.XElement Serialize()
        {
            var element = new System.Xml.Linq.XElement(Ficha.XmlTag);
            element.SetAttributeValue("id", this.IdFicha);
            element.SetAttributeValue("idpr", this.IdPredio);
            element.SetAttributeValue("idtf", this.IdTipoFicha);
            element.SetAttributeValue("idtm", this.IdTemporada);
            element.SetAttributeValue("f", this.Fecha);
            element.SetAttributeValue("ob", this.Observaciones);
            element.SetAttributeValue("ides", this.IdEstadoSiembra);
            element.SetAttributeValue("idis", this.IdImportanciaSeguimiento);

            return element;
        }

        public void Update(XmlNode node, string username)
        {
            this.IdTipoFicha = int.Parse(node.Attributes["idtf"].Value);
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
            var currentPotreros = dc.FichaPotrero.Where(fp => fp.IdFicha == this.IdFicha);
            dc.FichaPotrero.DeleteAllOnSubmit(currentPotreros);
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

                var fp = new FichaPotrero()
                {
                    IdPotrero = idPotrero,
                    MobileTag = "",
                    UserIns = username,
                    IpIns = Utils.RemoteAddr(),
                    FechaHoraIns = DateTime.Now
                };

                this.FichaPotrero.Add(fp);
                dc.SubmitChanges();

                result.Add(Repository.GetDifferentialResponseElement(Models.FichaPotrero.XmlTag, fp.IdFichaPotrero, id));
            }

            //Notificamos
            if (isNew)
            {
                try
                {
                    this.NotifyCreator(dc);
                }
                catch (Exception ex)
                {
                    var s = "";
                    s += "DateTime: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + "\r\n";
                    s += "IdFicha: " + this.IdFicha + "\r\n";
                    s += ex.ToString();
                    System.IO.File.WriteAllText(HttpContext.Current.Server.MapPath("~/App_Data/notifications/errors/") + System.Guid.NewGuid() + ".txt", s);
                }
            }

            return result;
        }

        public static Ficha CreateNew(string mobileTag, int idPredio, int idTemporada, string username)
        {
            return new Ficha()
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
        public void NotifyCreator(AgroFichasDBDataContext dc)
        {
            var mailer = new FichaMailer();
            mailer.NotifyCreator(this);
        }

        public OperationResult Notify(AgroFichasDBDataContext dc)
        {
            try
            {
                if (String.IsNullOrEmpty(this.Predio.Agricultor.Email))
                {
                    return new OperationResult(false, "No es posible enviar el correo por que nel agricultor no tiene email registrado.", "501", null);
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

        private void RepTemp(ref string Template, string Key, string Value)
        {
            Template = Template.Replace("***" + Key + "***", Value);
        }

        private void WriteCData(ref XmlTextWriter w, string Name, string Value)
        {
            w.WriteStartElement(Name);
            w.WriteCData(Value);
            w.WriteEndElement();
        }
    }
}