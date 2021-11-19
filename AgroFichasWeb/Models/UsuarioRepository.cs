using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public class UsuarioRepository
    {
        public AgroFichasDBDataContext db = new AgroFichasDBDataContext();

        public IQueryable<SYS_User> GetAll()
        {
            return db.SYS_User;
        }

        public IQueryable<SYS_User> GetAdminList(string key)
        {
            return (from u in db.SYS_User
                    where u.Disabled == false &&
                    (
                        (key == "") ||
                        (key != "" && u.Email.Contains(key)) ||
                        (key != "" && u.FullName.Contains(key)) ||
                        (key != "" && u.UserName.Contains(key))
                    )
                    orderby u.FullName
                    select u);
        }

        public IQueryable<Seccion> GetSecciones()
        {
            return from x in db.Seccion
                   where x.Habilitado == true
                   orderby x.Descripcion
                   select x;
        }

        public SYS_User GetByUserName(string userName)
        {
            return db.SYS_User.SingleOrDefault(u => u.UserName == userName);
        }

        public SYS_User GetByPassword(string cleanPassword)
        {
            return db.SYS_User.SingleOrDefault(u => u.Password == SYS_User.HashPassword(cleanPassword));
        }

        public SYS_User GetByUserNameAndPassword(string userName, string cleanPassword)
        {
            return db.SYS_User.SingleOrDefault(u => u.UserName == userName && u.Password == SYS_User.HashPassword(cleanPassword));
        }

        public void Add(SYS_User usuarioAdmin)
        {
            db.SYS_User.InsertOnSubmit(usuarioAdmin);
        }

        public void Delete(SYS_User usuarioAdmin)
        {
            db.SYS_User.DeleteOnSubmit(usuarioAdmin);
        }

        public void Save()
        {
            db.SubmitChanges();
        }
    }
}