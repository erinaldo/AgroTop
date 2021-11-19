using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels
{
    public class SelectorPotreroViewModel
    {
        public int IdPotrero { get; set; }
        public string NombrePotrero { get; set; }
        public bool Seleccionado { get; set; }

        public static List<SelectorPotreroViewModel> ForSiembraPotrero(AgroFichasDBDataContext dc, Siembra siembra)
        {
            return ForSiembraPotrero(dc, siembra.IdPredio, siembra.IdTemporada, siembra.IdSiembra);
        }

        public static List<SelectorPotreroViewModel> ForSiembraPotrero(AgroFichasDBDataContext dc, int idPredio, int idTemporada, int idSiembra)
        {
            var result = new List<SelectorPotreroViewModel>();

            var predio = dc.Predio.SingleOrDefault(p => p.IdPredio == idPredio);

            if (predio != null)
            {
                foreach (var potrero in predio.Potrero)
                {
                    var siembrasPotrero = potrero.SiembraPotrero.Where(sp => sp.IdTemporada == idTemporada && sp.IdPotrero == potrero.IdPotrero);
                    if (siembrasPotrero.Count() == 0)
                    {
                        result.Add(new SelectorPotreroViewModel()
                        {
                            IdPotrero = potrero.IdPotrero,
                            NombrePotrero = potrero.Nombre,
                            Seleccionado = false
                        });
                    }
                    else if (siembrasPotrero.Select(sp => sp.IdSiembra).Contains(idSiembra))
                    {
                        result.Add(new SelectorPotreroViewModel()
                        {
                            IdPotrero = potrero.IdPotrero,
                            NombrePotrero = potrero.Nombre,
                            Seleccionado = true
                        });
                    }
                }
            }

            return result;
        }

        public static List<SelectorPotreroViewModel> ForPreSiembraPotrero(AgroFichasDBDataContext dc, FichaPreSiembra ficha)
        {
            return ForPreSiembraPotrero(dc, ficha.IdPredio, ficha.IdTemporada, ficha.IdFichaPreSiembra);
        }

        public static List<SelectorPotreroViewModel> ForPreSiembraPotrero(AgroFichasDBDataContext dc, int idPredio, int idTemporada, int idFicha)
        {
            var result = new List<SelectorPotreroViewModel>();

            var predio = dc.Predio.SingleOrDefault(p => p.IdPredio == idPredio);

            if (predio != null)
            {
                foreach (var potrero in predio.Potrero)
                {
                    //Si el potrero ya está asignado a una ficha, lo incluimos
                    var fichaPotrero = potrero.FichaPreSiembraPotrero.Where(fp => fp.IdFichaPreSiembra == idFicha && fp.FichaPreSiembra.IdTemporada == idTemporada);

                    //Si no está en la ficha, tiene datos de siembra?
                    var tieneDatosSiembra = false;
                    if (fichaPotrero.Count() == 0)
                    {
                        var siembrasPotrero = potrero.SiembraPotrero.Where(sp => sp.IdTemporada == idTemporada);
                        tieneDatosSiembra = siembrasPotrero.Count() > 0;
                    }

                    if (fichaPotrero != null || tieneDatosSiembra)
                    {
                        result.Add(new SelectorPotreroViewModel()
                        {
                            IdPotrero = potrero.IdPotrero,
                            NombrePotrero = potrero.Nombre,
                            Seleccionado = fichaPotrero.Count() > 0
                        });
                    }
                }
            }

            return result;
        }

        public static List<SelectorPotreroViewModel> ForFichaPotrero(AgroFichasDBDataContext dc, Ficha ficha)
        {
            return ForFichaPotrero(dc, ficha.IdPredio, ficha.IdTemporada, ficha.IdFicha);
        }

        public static List<SelectorPotreroViewModel> ForFichaPotrero(AgroFichasDBDataContext dc, int idPredio, int idTemporada, int idFicha)
        {
            var result = new List<SelectorPotreroViewModel>();

            var predio = dc.Predio.SingleOrDefault(p => p.IdPredio == idPredio);

            if (predio != null)
            {
                foreach (var potrero in predio.Potrero)
                {
                    //Si el potrero ya está asignado a la ficha, lo incluimos
                    var fichaPotrero = potrero.FichaPotrero.Where(fp => fp.IdFicha == idFicha && fp.Ficha.IdTemporada == idTemporada);

                    //Si no está en la ficha, tiene datos de siembra?
                    var tieneDatosSiembra = false;
                    if (fichaPotrero.Count() == 0)
                    {
                        var siembrasPotrero = potrero.SiembraPotrero.Where(sp => sp.IdTemporada == idTemporada);
                        tieneDatosSiembra = siembrasPotrero.Count() > 0;
                    }

                    if (fichaPotrero.Count() > 0 || tieneDatosSiembra)
                    {
                        result.Add(new SelectorPotreroViewModel()
                        {
                            IdPotrero = potrero.IdPotrero,
                            NombrePotrero = potrero.Nombre,
                            Seleccionado = fichaPotrero.Count() > 0
                        });
                    }
                }
            }

            return result;
        }

    }
}