using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace AgroFichasApi.Models
{
    public class FullSync
    {
        public XDocument Response;
        public void FillResponse(AgroFichasDBDataContext dc)
        {

            throw new Exception("No usar");

            //Create the empty doc
            Response = new XDocument();
            var root = new XElement("data");
            Response.Add(root);

            
            //Proveedores
            root.Add(from pr in dc.Proveedor orderby pr.Nombre select pr.Serialize());

            //Agricultores
            root.Add(from ag in dc.Agricultor where ag.Habilitado orderby ag.Nombre select ag.Serialize());

            //Predios
            root.Add(from ag in dc.Agricultor
                     join pr in dc.Predio on ag.IdAgricultor equals pr.IdAgricultor
                     where ag.Habilitado
                        && pr.Habilitado
                     select pr.Serialize());

            //Potreros
            root.Add(from ag in dc.Agricultor
                      join pr in dc.Predio on ag.IdAgricultor equals pr.IdAgricultor
                      join po in dc.Potrero on pr.IdPredio equals po.IdPredio
                     where ag.Habilitado
                        && pr.Habilitado
                     select po.Serialize());

            //Comunas
            root.Add(from re in dc.Region
                     join pr in dc.Provincia on re.IdRegion equals pr.IdRegion
                     join co in dc.Comuna on pr.IdProvincia equals co.IdProvincia
                     where re.DoSync
                     orderby co.Orden
                     select co.Serialize());

            //Temporadas
            var temporadaActiva = dc.Temporada.Where(t => t.ActivaFichas).First();
            root.Add(temporadaActiva.Serialize());

            //Cultivos
            root.Add(from cu in dc.Cultivo select cu.Serialize());

            //Variedades
            root.Add(from va in dc.Variedad select va.Serialize());

            //Tipos de siembra
            root.Add(from ts in dc.TipoSiembra select ts.Serialize());

            //Siembras
            root.Add(from s in dc.Siembra where s.IdTemporada == temporadaActiva.IdTemporada select s.Serialize());

            //SiembrasPotrero
            root.Add(from sp in dc.SiembraPotrero where sp.IdTemporada == temporadaActiva.IdTemporada select sp.Serialize());

            //TiposRecomendacion
            root.Add(from tr in dc.TipoRecomendacion select tr.Serialize());

            //UM
            root.Add(from um in dc.UM select um.Serialize());

            //Quimico
            root.Add(from qu in dc.Quimico select qu.Serialize());

            //TiposFicha
            root.Add(from tf in dc.TipoFicha select tf.Serialize());
            
            //Ficha
            root.Add(from fi in dc.Ficha where fi.IdTemporada == temporadaActiva.IdTemporada select fi.Serialize());

            //Recomendacion
            root.Add(from re in dc.Recomendacion
                     join fi in dc.Ficha on re.IdFicha equals fi.IdFicha
                     where fi.IdTemporada == temporadaActiva.IdTemporada
                     select re.Serialize());

            //FichaPotrero
            root.Add(from fp in dc.FichaPotrero
                     join fi in dc.Ficha on fp.IdFicha equals fi.IdFicha
                     where fi.IdTemporada == temporadaActiva.IdTemporada
                     select fp.Serialize());
         }
    }
}