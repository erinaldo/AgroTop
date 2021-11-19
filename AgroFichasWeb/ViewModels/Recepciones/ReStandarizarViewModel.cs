using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels.Recepciones
{
    public class ReStandarizarViewModel
    {
        public List<ReStandarizarItem> Items { get; set; }

        public void Load(string filepath, AgroFichasDBDataContext dc)
        {
            this.Items = new List<ReStandarizarItem>();

            var file = System.IO.File.ReadAllLines(filepath);
            foreach (var line in file)
            {
                if (line != "")
                {
                    var pi = dc.ProcesoIngreso.Single(p => p.IdProcesoIngreso == int.Parse(line));
                    var item = new ReStandarizarItem()
                    {
                        IdProcesoIngreso = pi.IdProcesoIngreso,
                        ProcesoIngreso = pi,
                        Ori_PesoNormal = pi.PesoNormal.Value,
                        Ori_TotalNeto = pi.TotalNetoRecepcion
                    };

                    item.ProcesoIngreso.ReStandarizar(dc);
                    this.Items.Add(item);
                }
            }
        }
    }

    public class ReStandarizarItem
    {
        public int IdProcesoIngreso { get; set; }
        public ProcesoIngreso ProcesoIngreso { get; set; }

        public int Ori_PesoNormal { get; set; }
        public decimal? Ori_TotalNeto { get; set; }
    }
}