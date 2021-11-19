using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace AgroFichasApi.Models
{
    public partial class IntencionSiembra : IBaseEntity, ISynSerializable
    {
        public static string XmlTag = "is";

        public string[] SyncProperties
        {
            get { return new[] { "IdAgricultor", "IdTemporada", "IdCultivo", "IdComuna", "PuntoEntrega", "Cantidad", "Superficie", "Observaciones", "ID" }; }
            set { }
        }

        public int ID
        {
            get { return this.IdIntencionSiembra; }
        }

        public System.Xml.Linq.XElement Serialize()
        {
            var element = new System.Xml.Linq.XElement(Ficha.XmlTag);
            element.SetAttributeValue("id", this.IdIntencionSiembra);
            element.SetAttributeValue("idag", this.IdAgricultor);
            element.SetAttributeValue("idtm", this.IdTemporada);
            element.SetAttributeValue("idcu", this.IdCultivo);
            element.SetAttributeValue("idco", this.IdComuna);
            element.SetAttributeValue("ca", this.Cantidad);
            element.SetAttributeValue("su", this.Superficie);
            element.SetAttributeValue("pe", this.PuntoEntrega);
            element.SetAttributeValue("ob", this.Observaciones);

            return element;
        }

        public void Update(XmlNode node, string username)
        {
            this.IdCultivo = int.Parse(node.Attributes["idcu"].Value);
            this.IdComuna = int.Parse(node.Attributes["idco"].Value);
            this.Cantidad = int.Parse(node.Attributes["ca"].Value);
            this.Superficie = int.Parse(node.Attributes["su"].Value);
            this.PuntoEntrega = node.Attributes["pe"].Value ?? "";
            this.Observaciones = node.Attributes["ob"].Value ?? "";
            this.UserUpd = username;
            this.IpUpd = Utils.RemoteAddr();
            this.FechaHoraUpd = DateTime.Now;
        }

        public List<XElement> OnSubmitChanges(XmlNode node, AgroFichasDBDataContext dc, string username, bool isNew)
        {
            //Retornamos lista de equivalencias de ids
            var result = new List<XElement>();

            return result;
        }

        public static IntencionSiembra CreateNew(string mobileTag, int idAgricultor, int idTemporada, string username)
        {
            return new IntencionSiembra()
            {
                IdAgricultor = idAgricultor,
                IdTemporada = idTemporada,
                MobileTag = mobileTag,
                UserIns = username,
                IpIns = Utils.RemoteAddr(),
                FechaHoraIns = DateTime.Now
            };
        }
    }
}