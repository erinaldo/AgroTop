using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasApi.Models
{
    public class IndexAgricultor : ISynSerializable
    {
        public System.Xml.Linq.XElement Serialize()
        {
            throw new NotImplementedException();
        }

        public string[] SyncProperties
        {
            get { return new[] { "Rut", "ID" }; }
            set { }
        }

        public string Rut
        {
            get;
            set;
        }

        public int ID
        {
            get;
            set;
        }
    }
}