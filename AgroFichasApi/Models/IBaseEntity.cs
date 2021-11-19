using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace AgroFichasApi.Models
{
    public interface IBaseEntity
    {
        int ID { get; }
        string MobileTag { get; set; }

        void Update(XmlNode node, string username);
        List<XElement> OnSubmitChanges(XmlNode note, AgroFichasDBDataContext dc, string username, bool isNew);
    }
}