using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels.Recepciones
{
    public class ColaViewModel
    {
        private int length = 40; // Debe ser par

        public string Title { get; private set; }
        public List<ColaItem> Items { get; private set; }

        public ColaViewModel(AgroFichasDBDataContext dc, int idEstado, int idSucursal, int idTemporada)
        {
            var cola = (from pi in dc.ProcesoIngreso
                        where pi.IdTemporada == idTemporada
                           && pi.IdSucursal == idSucursal
                           && pi.IdEstado == idEstado
                        orderby pi.FechaHoraLlegada ascending
                        select pi).Take(length).ToList();

            Items = new List<ColaItem>();

            int halfLength = length / 2;
            for (int i = 0; i < halfLength; i++ )
            {
                if (cola.Count > i)
                    Items.Add(new ColaItem(i + 1, cola[i]));
                else
                    Items.Add(new ColaItem(i + 1));

                if (cola.Count > i + halfLength)
                    Items.Add(new ColaItem(i + halfLength + 1, cola[i + halfLength]));
                else
                    Items.Add(new ColaItem(i + halfLength + 1));
            }

            string estado = "";
            if (idEstado == 1)
                estado = "Toma de Muestra";
            else if (idEstado == 4)
                estado = "Primer Pesaje";

            this.Title = String.Format("{0} {1} {2}", estado, dc.Sucursal.Single(s => s.IdSucursal == idSucursal).Nombre, dc.Temporada.Single(t => t.IdTemporada == idTemporada).Nombre);
        }
    }

    public class ColaItem
    {
        public int Orden { get; set; }
        public string Patente { get; set; }
        public string Nombre { get; set; }

        public ColaItem(int orden)
        {
            this.Orden = orden;
            this.Patente = "";
            this.Nombre = "";
        }

        public ColaItem(int orden, ProcesoIngreso pi)
        {
            this.Orden = orden;
            this.Patente = pi.Patente;
            this.Nombre = pi.Agricultor.Nombre;
        }
    }
}