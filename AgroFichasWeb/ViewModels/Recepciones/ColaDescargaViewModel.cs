using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels.Recepciones
{
    public class ColaDescargaViewModel
    {
        private int length = 40; // Debe ser par

        public string Title { get; private set; }
        public List<ColaDescargaItem> Items { get; private set; }

        public ColaDescargaViewModel(AgroFichasDBDataContext dc, int idSucursal, int idTemporada)
        {
            int[] estados = { 2, 3, 4, 5 };

            var cola = (from pi in dc.ProcesoIngreso
                        where pi.IdTemporada == idTemporada
                           && pi.IdSucursal == idSucursal
                           && estados.Contains(pi.IdEstado)
                           && (
                                !pi.IdPuntoDescarga.HasValue || //No ha definido el punto de descarga
                                (pi.IdPuntoDescarga.HasValue && !pi.PuntoDescarga.EsExternoSucursal) || //el punto de descargar es local
                                (pi.IdPuntoDescarga.HasValue && pi.PuntoDescarga.EsExternoSucursal && pi.FechaHoraPuntoDescarga.Value.AddMinutes(13) > DateTime.Now) //el pd es externo y no ha pasado 10 minutos
                           )
                        orderby pi.IdPuntoDescarga.HasValue descending,
                                pi.FechaHoraLlegada ascending
                        select pi).Take(length).ToList();

            Items = new List<ColaDescargaItem>();

            int halfLength = length / 2;
            for (int i = 0; i < halfLength; i++ )
            {
                if (cola.Count > i)
                    Items.Add(new ColaDescargaItem(i + 1, cola[i]));
                else
                    Items.Add(new ColaDescargaItem(i + 1));

                if (cola.Count > i + halfLength)
                    Items.Add(new ColaDescargaItem(i + halfLength + 1, cola[i + halfLength]));
                else
                    Items.Add(new ColaDescargaItem(i + halfLength + 1));
            }

            this.Title = String.Format("Espera de Descarga {0} {1}", dc.Sucursal.Single(s => s.IdSucursal == idSucursal).Nombre, dc.Temporada.Single(t => t.IdTemporada == idTemporada).Nombre);
        }
    }

    public class ColaDescargaItem
    {
        public int Orden { get; set; }
        public string Patente { get; set; }
        public string Nombre { get; set; }
        public string EstadoAnalisis { get; set; }
        public string ColorAnalisis { get; set; }
        public string EstadoDescarga { get; set; }
        public string ColorDescarga { get; set; }

        public ColaDescargaItem(int orden)
        {
            this.Orden = orden;
            this.Patente = "";
            this.Nombre = "";
            this.EstadoAnalisis = "";
            this.ColorAnalisis = "";
            this.EstadoDescarga = "";
            this.ColorDescarga = "";
        }

        public ColaDescargaItem(int orden, ProcesoIngreso pi)
        {
            this.Orden = orden;
            this.Patente = pi.Patente;
            this.Nombre = pi.Agricultor.Nombre;

            if (pi.Autorizado.HasValue)
            {
                this.EstadoAnalisis = pi.EstadoProcesoIngreso.Nombre;
                this.ColorAnalisis = pi.EstadoProcesoIngreso.Color;
            }
            else
            {
                this.EstadoAnalisis = "En proceso";
                this.ColorAnalisis = "";
            }

            if (pi.IdPuntoDescarga.HasValue)
            {
                this.EstadoDescarga = pi.PuntoDescarga.Nombre;
                this.ColorDescarga = "#83f03c";
            }
            else
            {
                this.EstadoDescarga = "En espera";
                this.ColorDescarga = "";
            }
        }
    }
}