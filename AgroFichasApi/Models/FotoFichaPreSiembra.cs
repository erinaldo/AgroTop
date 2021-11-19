﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace AgroFichasApi.Models
{
    public partial class FotoFichaPreSiembra : IBaseEntity, ISynSerializable
    {
        public static string XmlTag = "ffps";

        public string[] SyncProperties
        {
            get { return new[] { "IdFichaPreSiembra", "FileName", "Observaciones", "ID" }; }
            set { }
        }

        public int ID
        {
            get { return this.IdFotoFichaPreSiembra; }
        }

        public System.Xml.Linq.XElement Serialize()
        {
            var element = new System.Xml.Linq.XElement(FotoFichaPreSiembra.XmlTag);
            element.SetAttributeValue("id", this.IdFotoFichaPreSiembra);
            element.SetAttributeValue("idfi", this.IdFichaPreSiembra);
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
            if (fileName.Length >= 3)
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

        public static FotoFichaPreSiembra CreateNew(string mobileTag, int idFichaPreSiembra, string username)
        {
            return new FotoFichaPreSiembra()
            {
                IdFichaPreSiembra = idFichaPreSiembra,
                Observaciones = "",
                MobileTag = mobileTag,
                UserIns = username,
                IpIns = Utils.RemoteAddr(),
                FechaHoraIns = DateTime.Now
            };
        }

    }
}