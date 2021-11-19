using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace AgroFichasApi.Models
{
    public interface ISynSerializable
    {
        System.Xml.Linq.XElement Serialize();
        string[] SyncProperties { get; set; }
    }

    public interface ISynSerializableWithAlias
    {
        System.Xml.Linq.XElement Serialize();
        Dictionary<string, string> SyncProperties { get; set; }
    }
}