using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasApi.Models
{
    public partial class Temporada : ISynSerializableWithAlias
    {
        public Dictionary<string, string> SyncProperties
        {
            get { 
                return new Dictionary<string, string> { 
                    { "Nombre", "Nombre" },
                    { "Activa", "ActivaFichas" },
                    { "ID", "ID" } 
                }; 
            }
            
            set { }
        }

        public int ID
        {
            get { return this.IdTemporada; }
        }

        public System.Xml.Linq.XElement Serialize()
        {
            var element = new System.Xml.Linq.XElement("tm");
            element.SetAttributeValue("id", this.IdTemporada);
            element.SetAttributeValue("n", this.Nombre);
            element.SetAttributeValue("a", this.ActivaFichas ? "1" : "0");

            return element;
        }
    }
}