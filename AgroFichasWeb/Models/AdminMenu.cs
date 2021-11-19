using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    //public class AdminMenu
    //{
    //    private List<AdminMenuItem> items;

    //    public List<AdminMenuItem> Items
    //    {
    //        get
    //        {
    //            return items;
    //        }
    //    }

    //    public AdminMenu(string userName)
    //    {
    //        var dc = new AgroFichasDBDataContext();
            
    //        items = new List<AdminMenuItem>();
           
    //        //items = (from modulo in dc.SYS_ModulosUsuario(userName)
    //        //        select new AdminMenuItem(modulo.Nombre, "~/modulo/index/" + modulo.IdModulo, false, 0)).ToList();

    //        items.Add(new AdminMenuItem("Agricultores", "~/agricultores", false, 3));
    //        items.Add(new AdminMenuItem("Informes", "~/informes", false, 10));
    //        items.Add(new AdminMenuItem("Proveedores", "~/proveedores", false, 6));
    //        items.Add(new AdminMenuItem("Químicos", "~/quimicos?tipo=4", false, 4));
    //        items.Add(new AdminMenuItem("Cultivos", "~/cultivos?tipo=1", false, 7));
    //        items.Add(new AdminMenuItem("Usuarios", "~/usuarios", false, 2));
    //    }

    //    public void SelectByNombre(string nombre)
    //    {
    //        foreach (var i in items)
    //            i.Selected = i.Nombre == nombre;
    //    }
    //}

    public class AdminMenuItem
    {
        public int ID { get; set; }
        public string Nombre { get; set; }
        public string Url { get; set; }
        public bool Selected { get; set; }
        public int IdPermiso { get; set; }
        public bool ContieneSubMenu { get; set; }

    }
}