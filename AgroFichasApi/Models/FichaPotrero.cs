using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasApi.Models
{
    public partial class FichaPotrero : ISynSerializable
    {
        public static string XmlTag = "fp";

        public string[] SyncProperties
        {
            get { return new[] { "IdPotrero", "IdFicha", "ID" }; }
            set { }
        }

        public int ID
        {
            get { return this.IdFichaPotrero; }
        }


        public System.Xml.Linq.XElement Serialize()
        {
            var element = new System.Xml.Linq.XElement(FichaPotrero.XmlTag);
            element.SetAttributeValue("id", this.IdFichaPotrero);
            element.SetAttributeValue("idpo", this.IdPotrero);
            element.SetAttributeValue("idfi", this.IdFicha);
            return element;
        }
    }
}