using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace AgroFichasApi.Models
{
    public partial class Recomendacion : ISynSerializable, IBaseEntity
    {
        public static string XmlTag = "re";

        public string[] SyncProperties
        {
            get { return new[] { "IdFicha", "IdQuimico", "FechaAplicacion", "Dosis", "IdUM", "FerN", "FerP2O5", "FerKO2", "FerMgO", "FerS","FerB","FerZn", "FerCaO", "ID" }; }
            set { }
        }

        public int ID
        {
            get { return this.IdRecomendacion; }
        }

        public System.Xml.Linq.XElement Serialize()
        {
            var element = new System.Xml.Linq.XElement(Recomendacion.XmlTag);
            element.SetAttributeValue("id", this.IdRecomendacion);
            element.SetAttributeValue("idfi", this.IdFicha);
            element.SetAttributeValue("idqu", this.IdQuimico);
            element.SetAttributeValue("d", this.Dosis);
            element.SetAttributeValue("um", this.IdUM);
            element.SetAttributeValue("fa", this.FechaAplicacion);
            
            element.SetAttributeValue("N", this.FerN);
            element.SetAttributeValue("P2O5", this.FerP2O5);
            element.SetAttributeValue("KO2", this.FerKO2);
            element.SetAttributeValue("MgO", this.FerMgO);
            element.SetAttributeValue("S", this.FerS);
            element.SetAttributeValue("B", this.FerB);
            element.SetAttributeValue("Zn2", this.FerZn);
            element.SetAttributeValue("CaO", this.FerCaO);

            return element;
        }

        public void Update(XmlNode node, string username)
        {
            this.IdQuimico = int.Parse(node.Attributes["idqu"].Value);
            this.FechaAplicacion = (node.Attributes["fa"] != null) ? (DateTime?)DateTime.Parse(node.Attributes["fa"].Value) : null;
            this.IdUM = int.Parse(node.Attributes["um"].Value);
            this.Dosis = Utils.ParseDecimal(node.Attributes["d"].Value);
            this.FerN = Utils.ParseDecimal(node.Attributes["N"].Value);
            this.FerP2O5 = Utils.ParseDecimal(node.Attributes["P2O5"].Value);
            this.FerKO2 = Utils.ParseDecimal(node.Attributes["KO2"].Value);
            this.FerMgO = Utils.ParseDecimal(node.Attributes["MgO"].Value);
            this.FerS = Utils.ParseDecimal(node.Attributes["S"].Value);
            this.FerB = Utils.ParseDecimal(node.Attributes["B"].Value);
            this.FerZn = Utils.ParseDecimal(node.Attributes["Zn"].Value);
            this.FerCaO = Utils.ParseDecimal(node.Attributes["CaO"].Value);

            this.UserUpd = username;
            this.IpUpd = Utils.RemoteAddr();
            this.FechaHoraUpd = DateTime.Now;
        }

        public static Recomendacion CreateNew(string mobileTag, int idFicha, string username)
        {
            return new Recomendacion()
            {
                IdFicha = idFicha,
                MobileTag = mobileTag,
                UserIns = username,
                IpIns = Utils.RemoteAddr(),
                FechaHoraIns = DateTime.Now
            };
        }


        public List<XElement> OnSubmitChanges(XmlNode note, AgroFichasDBDataContext dc, string username, bool isNew)
        {
            return new List<XElement>();
        }
    }
}