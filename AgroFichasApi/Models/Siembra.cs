using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace AgroFichasApi.Models
{
    public partial class Siembra : IBaseEntity, ISynSerializable
    {
        public static string XmlTag = "si";

        public string[] SyncProperties
        {
            get { return new[] { "IdPredio", "IdVariedad", "FechaSiembra", "Dosis", "IdCultivoAnterior", "IdTemporada", "RendimientoEstimado", "FechaCosechaEstimada", "ID" }; }
            set { }
        }

        public int ID
        {
            get { return this.IdSiembra; }
        }

        public System.Xml.Linq.XElement Serialize()
        {
            var element = new System.Xml.Linq.XElement(Siembra.XmlTag);
            element.SetAttributeValue("id", this.IdSiembra);
            element.SetAttributeValue("idpr", this.IdPredio);
            element.SetAttributeValue("idva", this.IdVariedad);
            element.SetAttributeValue("f", this.FechaSiembra);
            element.SetAttributeValue("d", this.Dosis);
            element.SetAttributeValue("idca", this.IdCultivoAnterior);
            element.SetAttributeValue("idts", this.IdTipoSiembra);
            element.SetAttributeValue("idtm", this.IdTemporada);
            element.SetAttributeValue("fc", this.FechaCosechaEstimada);
            element.SetAttributeValue("re", this.RendimientoEstimado);
            return element;
        }

        public void Update(XmlNode node, string username)
        {
            this.IdVariedad = int.Parse(node.Attributes["idva"].Value);
            this.FechaSiembra = DateTime.Parse(node.Attributes["f"].Value).Date;
            this.Dosis = Utils.ParseDecimal(node.Attributes["d"].Value);
            this.IdCultivoAnterior = int.Parse(node.Attributes["idca"].Value);
            this.IdTipoSiembra = int.Parse(node.Attributes["idts"].Value);

            if (node.Attributes["re"] != null)
                this.RendimientoEstimado = Utils.ParseDecimal(node.Attributes["re"].Value);

            if (node.Attributes["fc"] != null) //El atributo viene, es decir el cliente sabe de "fc"
            {
                if (node.Attributes["fc"].Value != "") //si viene una cadena vacía, entonces es null
                    this.FechaCosechaEstimada = DateTime.Parse(node.Attributes["fc"].Value).Date;
                else
                    this.FechaCosechaEstimada = null;
            }

            this.UserUpd = username;
            this.IpUpd = Utils.RemoteAddr();
            this.FechaHoraUpd = DateTime.Now;
        }

        public List<XElement> OnSubmitChanges(XmlNode node, AgroFichasDBDataContext dc, string username, bool isNew)
        {
            //Retornamos lista de equivalencias de ids
            var result = new List<XElement>();
            
            //Eliminamos los Potreros
            var currentPotreros = dc.SiembraPotrero.Where(sp => sp.IdSiembra == this.IdSiembra);
            dc.SiembraPotrero.DeleteAllOnSubmit(currentPotreros);
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

                var sp = new SiembraPotrero()
                {
                    IdPotrero = idPotrero,
                    IdTemporada = this.IdTemporada,
                    MobileTag = "",
                    UserIns = username,
                    IpIns = Utils.RemoteAddr(),
                    FechaHoraIns = DateTime.Now
                };

                this.SiembraPotrero.Add(sp);
                dc.SubmitChanges();
                
                result.Add(Repository.GetDifferentialResponseElement(Models.SiembraPotrero.XmlTag, sp.IdSiembraPotrero, id));
            }

            return result;
        }

        public static Siembra CreateNew(string mobileTag, int idPredio, int idTemporada, string username)
        {
            return new Siembra()
            {
                IdPredio = idPredio,
                IdTemporada = idTemporada,
                MobileTag = mobileTag,
                UserIns = username,
                IpIns = Utils.RemoteAddr(),
                FechaHoraIns = DateTime.Now
            };
        }
    }
}