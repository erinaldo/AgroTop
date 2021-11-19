using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace AgroFichasApi.Models
{
    public class Repository
    {
        public static List<System.Xml.Linq.XElement> SyncTable<T>(AgroFichasDBDataContext dc, XmlElement root, string xmlTag, string username) where T : class, IBaseEntity 
        {
            var list = root.SelectNodes(xmlTag);
            var result = new List<System.Xml.Linq.XElement>();

            foreach (XmlNode item in list)
            {
                var isNew = false;
                int id = int.Parse(item.Attributes["id"].Value); //el ID según el móvil
                string lid = item.Attributes["lid"].Value;

                T record = GetItem<T>(dc, id, lid, item, username);

                if (record == null)
                {
                    record = Repository.Factory<T>(dc, lid, item, username);
                    dc.GetTable<T>().InsertOnSubmit(record);
                    isNew = true;
                }
                record.Update(item, username);
                dc.SubmitChanges();
                
                result.AddRange(record.OnSubmitChanges(item, dc, username, isNew));
                result.Add(GetDifferentialResponseElement(xmlTag, record.ID, id));
            }

            return result;
        }

        public static XElement GetDifferentialResponseElement(string xmlTag, int finalID, int provisoryID)
        {
            var element = new XElement(xmlTag);
            element.SetAttributeValue("fin", finalID);
            element.SetAttributeValue("prv", provisoryID);
            return element;
        }

        public static T GetItem<T>(AgroFichasDBDataContext dc, int id, string mobileTag, XmlNode xmlItem, string username) where T : class, IBaseEntity
        {
            T item;

            if (id > 0)
            {
                item = dc.GetTable<T>().FromId(id);
                if (item == null)
                    throw new Exception("Item not found by id: " + typeof(T).ToString() + ": " + id);
            }
            else
            {
                item = dc.GetTable<T>().FromMobileTag(mobileTag);

                if (item == null && typeof(T) == typeof(Agricultor))
                {
                    string rut = xmlItem.Attributes["r"].Value.Trim().Replace(".", "").Replace(",", "").ToUpper();
                    var agricultor = dc.Agricultor.SingleOrDefault(a => a.Rut == rut);
                    if (agricultor != null)
                    {
                        agricultor.MobileTag = mobileTag;
                        agricultor.GiveUserAccess(dc, username);
                        item = (T)(object)agricultor;
                    }
                }
            }
            return item;
        }

        public static T Factory<T>(AgroFichasDBDataContext dc, string mobileTag, XmlNode node, string username)
        {
            if (typeof(T) == typeof(Agricultor))
            {
                return (T)(object)Agricultor.CreateNew(mobileTag, username);
            }
            else if (typeof(T) == typeof(Predio))
            {
                //Existía el agricultor cuando creé el predio?
                var id = int.Parse(node.Attributes["idag"].Value);
                if (id < 0)
                    id = dc.GetTable<Agricultor>().FromMobileTag(node.Attributes["lip1"].Value).IdAgricultor;

                return (T)(object)Predio.CreateNew(mobileTag, id, username);
            }
            else if (typeof(T) == typeof(Potrero))
            {
                //Existía el predio cuando creé el potrero?
                var id = int.Parse(node.Attributes["idpr"].Value);
                if (id < 0)
                    id = dc.GetTable<Predio>().FromMobileTag(node.Attributes["lip1"].Value).IdPredio;

                return (T)(object)Potrero.CreateNew(mobileTag, id, username);
            }
            else if (typeof(T) == typeof(Siembra))
            {
                var id = int.Parse(node.Attributes["idpr"].Value);
                if (id < 0)
                    id = dc.GetTable<Predio>().FromMobileTag(node.Attributes["lip1"].Value).IdPredio;

                var idTemporada = int.Parse(node.Attributes["idtm"].Value);

                return (T)(object)Siembra.CreateNew(mobileTag, id, idTemporada, username);
            }
            else if (typeof(T) == typeof(Ficha))
            {
                var id = int.Parse(node.Attributes["idpr"].Value);
                if (id < 0)
                    id = dc.GetTable<Predio>().FromMobileTag(node.Attributes["lip1"].Value).IdPredio;

                var idTemporada = int.Parse(node.Attributes["idtm"].Value);

                return (T)(object)Ficha.CreateNew(mobileTag, id, idTemporada, username);
            }
            else if (typeof(T) == typeof(Recomendacion))
            {
                var id = int.Parse(node.Attributes["idfi"].Value);
                if (id < 0)
                    id = dc.GetTable<Ficha>().FromMobileTag(node.Attributes["lip1"].Value).IdFicha;

                return (T)(object)Recomendacion.CreateNew(mobileTag, id, username);
            }
            else if (typeof(T) == typeof(FotoFicha))
            {
                var id = int.Parse(node.Attributes["idfi"].Value);
                if (id < 0)
                    id = dc.GetTable<Ficha>().FromMobileTag(node.Attributes["lip1"].Value).IdFicha;

                return (T)(object)FotoFicha.CreateNew(mobileTag, id, username);
            }
            else if (typeof(T) == typeof(FichaPreSiembra))
            {
                var id = int.Parse(node.Attributes["idpr"].Value);
                if (id < 0)
                    id = dc.GetTable<Predio>().FromMobileTag(node.Attributes["lip1"].Value).IdPredio;

                var idTemporada = int.Parse(node.Attributes["idtm"].Value);

                return (T)(object)FichaPreSiembra.CreateNew(mobileTag, id, idTemporada, username);
            }
            else if (typeof(T) == typeof(RecomendacionPreSiembra))
            {
                var id = int.Parse(node.Attributes["idfi"].Value);
                if (id < 0)
                    id = dc.GetTable<FichaPreSiembra>().FromMobileTag(node.Attributes["lip1"].Value).IdFichaPreSiembra;

                return (T)(object)RecomendacionPreSiembra.CreateNew(mobileTag, id, username);
            }
            else if (typeof(T) == typeof(FotoFichaPreSiembra))
            {
                var id = int.Parse(node.Attributes["idfi"].Value);
                if (id < 0)
                    id = dc.GetTable<FichaPreSiembra>().FromMobileTag(node.Attributes["lip1"].Value).IdFichaPreSiembra;

                return (T)(object)FotoFichaPreSiembra.CreateNew(mobileTag, id, username);
            }
            else if (typeof(T) == typeof(IntencionSiembra))
            {
                //Existía el agricultor cuando creé el predio?
                var id = int.Parse(node.Attributes["idag"].Value);
                if (id < 0)
                    id = dc.GetTable<Agricultor>().FromMobileTag(node.Attributes["lip1"].Value).IdAgricultor;

                var idTemporada = int.Parse(node.Attributes["idtm"].Value);

                return (T)(object)IntencionSiembra.CreateNew(mobileTag, id, idTemporada, username);
            }

            throw new Exception("Type no implementado");
        }
    }

    public static class TableExtensions
    {
        public static T FromId<T>(this Table<T> table, object id) where T : class
        {
            // Get type from Mapping
            MetaType type = table.Context.Mapping.GetTable(typeof(T)).RowType;

            // Only supports 1 ID member
            MetaDataMember idMember = type.IdentityMembers[0];

            // Build linq query and return result
            ParameterExpression parameter = Expression.Parameter(typeof(T), "item");
            return table.FirstOrDefault(Expression.Lambda<Func<T, bool>>(Expression.Equal(Expression.Property(parameter, idMember.Member.Name), Expression.Constant(id)), parameter));
        }

        public static T FromMobileTag<T>(this Table<T> table, string mobileTag) where T : class, IBaseEntity
        {
            // Get type from Mapping
            MetaType type = table.Context.Mapping.GetTable(typeof(T)).RowType;

            // Only supports 1 ID member
            MetaDataMember idMember = type.IdentityMembers[0];

            // Build linq query and return result
            ParameterExpression parameter = Expression.Parameter(typeof(T), "item");
            return table.FirstOrDefault(Expression.Lambda<Func<T, bool>>(Expression.Equal(Expression.Property(parameter, "MobileTag"), Expression.Constant(mobileTag)), parameter));
        }
    }
}