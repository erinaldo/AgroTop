using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace AgroFichasApi.Models
{
    public partial class Potrero : IBaseEntity, ISynSerializable
    {
        public static string XmlTag = "po";

        public string[] SyncProperties
        {
            get { return new[] { "IdPredio", "Nombre", "Superficie", "ID" }; }
            set { }
        }

        public int ID
        {
            get { return this.IdPotrero; }
        }

        public System.Xml.Linq.XElement Serialize()
        {
            var element = new System.Xml.Linq.XElement(Potrero.XmlTag);
            element.SetAttributeValue("id", this.IdPotrero);
            element.SetAttributeValue("idpr", this.IdPredio);
            element.SetAttributeValue("n",this.Nombre);
            element.SetAttributeValue("s", this.Superficie);
            
            return element;
        }

        public void Update(XmlNode node, string username)
        {
            this.Nombre = node.Attributes["n"].Value;
            this.Superficie = int.Parse(node.Attributes["s"].Value);
            this.UserUpd = username;
            this.IpUpd = Utils.RemoteAddr();
            this.FechaHoraUpd = DateTime.Now;
        }

        public List<XElement> OnSubmitChanges(XmlNode node, AgroFichasDBDataContext dc, string username, bool isNew)
        {
            return new List<XElement>();
        }

        public static Potrero CreateNew(string mobileTag, int idPredio, string username)
        {
            return new Potrero()
            {
                IdPredio = idPredio,
                Habilitado = true,
                MobileTag = mobileTag,
                UserIns = username,
                IpIns = Utils.RemoteAddr(),
                FechaHoraIns = DateTime.Now
            };
        }
    }
}