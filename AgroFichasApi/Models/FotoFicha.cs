using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace AgroFichasApi.Models
{
    public partial class FotoFicha : IBaseEntity, ISynSerializable
    {
        public static string XmlTag = "ff";

        public string[] SyncProperties
        {
            get { return new[] { "IdFicha", "FileName", "Observaciones", "ID" }; }
            set { }
        }

        public int ID
        {
            get { return this.IdFotoFicha; }
        }

        public System.Xml.Linq.XElement Serialize()
        {
            var element = new System.Xml.Linq.XElement(FotoFicha.XmlTag);
            element.SetAttributeValue("id", this.IdFotoFicha);
            element.SetAttributeValue("idfi", this.IdFicha);
            element.SetAttributeValue("fn", this.FileName);
            element.SetAttributeValue("ob", this.Observaciones);

            return element;
        }

        public void Update(XmlNode node, string username)
        {
            //this.IdFicha = int.Parse(node.Attributes["idfi"].Value);
            this.FileName = node.Attributes["fn"].Value;
            this.Observaciones = node.Attributes["ob"].Value;
            this.UserUpd = username;
            this.IpUpd = Utils.RemoteAddr();
            this.FechaHoraUpd = DateTime.Now;

            this.SaveFoto(this.FileName, node.Attributes["bin"].Value);
        }

        private void SaveFoto(string fileName, string encodedFile)
        {
            if (String.IsNullOrEmpty(encodedFile))
                return;

            var folder = "default";
            if (fileName.Length>=3)
            {
                folder = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Fotos"), fileName.Substring(0, 1), fileName.Substring(0, 2));
            }

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var bytes = Convert.FromBase64String(encodedFile);
            File.WriteAllBytes(Path.Combine(folder, fileName), bytes);
        }

        public List<XElement> OnSubmitChanges(XmlNode node, AgroFichasDBDataContext dc, string username, bool isNew)
        {
            return new List<XElement>();
        }

        public static FotoFicha CreateNew(string mobileTag, int idFicha, string username)
        {
            return new FotoFicha()
            {
                IdFicha = idFicha,
                Observaciones = "",
                MobileTag = mobileTag,
                UserIns = username,
                IpIns = Utils.RemoteAddr(),
                FechaHoraIns = DateTime.Now
            };
        }

    }
}