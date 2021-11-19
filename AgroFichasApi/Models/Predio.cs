using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace AgroFichasApi.Models
{
    public partial class Predio : IBaseEntity, ISynSerializable
    {
        public static string XmlTag = "pr";

        public string[] SyncProperties
        {
            get { return new[] { "IdAgricultor", "Nombre", "IdComuna", "ID", "Lat", "Lon" }; }
            set { }
        }

        public int ID
        {
            get { return this.IdPredio; }
        }

        public System.Xml.Linq.XElement Serialize()
        {
            var element = new System.Xml.Linq.XElement(Predio.XmlTag);
            element.SetAttributeValue("id", this.IdPredio);
            element.SetAttributeValue("idag", this.IdAgricultor);
            element.SetAttributeValue("n", this.Nombre);
            element.SetAttributeValue("idco", this.IdComuna);
            element.SetAttributeValue("lat", this.Lat);
            element.SetAttributeValue("lon", this.Lon);
            return element;
        }

        public void Update(XmlNode node, string username)
        {
            this.Nombre = node.Attributes["n"].Value;
            this.IdComuna = int.Parse(node.Attributes["idco"].Value);

            if (node.Attributes["lat"] != null)
                this.Lat = Utils.ParseDecimal(node.Attributes["lat"].Value);

            if (node.Attributes["lon"] != null)
                this.Lon = Utils.ParseDecimal(node.Attributes["lon"].Value);

            this.UserUpd = username;
            this.IpUpd = Utils.RemoteAddr();
            this.FechaHoraUpd = DateTime.Now;
        }

        public List<XElement> OnSubmitChanges(XmlNode node, AgroFichasDBDataContext dc, string username, bool isNew)
        {
            return new List<XElement>();
        }

        public static Predio CreateNew(string mobileTag, int idAgricultor, string username)
        {
            return new Predio()
            {
                IdAgricultor = idAgricultor,
                Origen = 0,
                IDOleotop = "",
                IDAvenatop = "",
                IDGranotop = "",
                Habilitado = true,
                MobileTag = mobileTag,
                UserIns = username,
                IpIns = Utils.RemoteAddr(),
                FechaHoraIns = DateTime.Now
            };
        }
    }
}