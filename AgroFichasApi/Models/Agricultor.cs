using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace AgroFichasApi.Models
{
    public partial class Agricultor : IBaseEntity, ISynSerializable
    {
        public static string XmlTag = "ag";

        public string[] SyncProperties
        {
            get { return new[] { "Rut", "Nombre", "ID", "Fono1", "Fono2", "Email", "IdProveedor" }; }
            set { }
        }

        public int ID
        {
            get { return this.IdAgricultor; }
        }

        public System.Xml.Linq.XElement Serialize()
        {
            var element = new System.Xml.Linq.XElement(Agricultor.XmlTag);
            element.SetAttributeValue("id", this.IdAgricultor);
            element.SetAttributeValue("r",  this.Rut);
            element.SetAttributeValue("n", this.Nombre);
            element.SetAttributeValue("e", this.Email);
            element.SetAttributeValue("f1", this.Fono1);
            element.SetAttributeValue("f2", this.Fono2);
            element.SetAttributeValue("idpr", this.IdProveedor);

            return element;
        }

        public void Update(XmlNode node, string username)
        {
            this.Rut = node.Attributes["r"].Value.Trim().Replace(".", "").Replace(",", "").Replace(" ", "").ToUpper();
            this.Nombre = node.Attributes["n"].Value;
            this.IdProveedor = int.Parse(node.Attributes["idpr"].Value);
            this.Email = node.Attributes["e"].Value;
            this.Fono1 = node.Attributes["f1"].Value;
            this.Fono2 = node.Attributes["f2"].Value;
            this.UserUpd = username;
            this.IpUpd = Utils.RemoteAddr();
            this.FechaHoraUpd = DateTime.Now;
        }

        public List<XElement> OnSubmitChanges(XmlNode node, AgroFichasDBDataContext dc, string username, bool isNew)
        {
            //damos automáticamente acceso al usuario que creó al agricultor
            if (isNew)
            {
                this.GiveUserAccess(dc, username);
                dc.SubmitChanges();
            }

            return new List<XElement>();
        }

        public void GiveUserAccess(AgroFichasDBDataContext dc, string username)
        {
            var usuario = dc.SYS_User.SingleOrDefault(u => u.UserName == username);

            if (usuario != null)
            {
                if (this.UsuarioAgricultor.SingleOrDefault(ua => ua.UserID == usuario.UserID) == null)
                {
                    this.UsuarioAgricultor.Add(new UsuarioAgricultor()
                    {
                        UserID = usuario.UserID,
                        MobileTag = "",
                        UserIns = "api",
                        FechaHoraIns = DateTime.Now,
                        IpIns = "127.0.0.1"
                    });
                }
            }
        }

        public static Agricultor CreateNew(string mobileTag, string username)
        {
            return new Agricultor()
            {
                Email = "",
                Fono1 = "",
                Fono2 = "",
                Origen = 0,
                IDOleotop = "",
                IDAvenatop = "",
                IDGranotop = "",
                Habilitado = true,
                IdProveedor = 1,
                MobileTag = mobileTag,
                UserIns = username,
                IpIns = Utils.RemoteAddr(),
                FechaHoraIns = DateTime.Now
            };
        }
    }
}