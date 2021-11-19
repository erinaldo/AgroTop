using AgrotopApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;

namespace AgrotopApi.Controllers
{
    public class FechasController : ApiController
    {
        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        /// <summary>
        /// Obtiene una lista de años desde el año actual + 2 hacia adelante
        /// </summary>
        /// <returns>Año</returns>
        public List<SelectAño> GetAños()
        {
            List<SelectAño> list = new List<SelectAño>();
            for (int I = DateTime.Now.Year - 1; I < (DateTime.Now.Year + ((DateTime.Now.Year - DateTime.Now.Year) + 3)); I++)
            {
                list.Add(new Models.SelectAño()
                {
                    Año = I,
                });
            }

            return list;
        }

        /// <summary>
        /// Obtiene una lista de los días de la semana N en base a la fecha de inicio de la semana N en cuestión
        /// </summary>
        /// <param name="fInicio"></param>
        /// <returns>Fecha, DiaDeLaSemana</returns>
        public List<SelectDiasSemana> GetDiasSemana(DateTime fInicio)
        {
            List<SelectDiasSemana> list = new List<SelectDiasSemana>();
            List<CTR_GetDiasDeLaSemanaPorDiaResult> diaList = dc.CTR_GetDiasDeLaSemanaPorDia(fInicio).ToList();
            foreach (var dia in diaList)
            {
                list.Add(new SelectDiasSemana()
                {
                    Fecha = dia.dates.Value,
                    DiaDeLaSemana = dia.theday
                });
            }

            return list;
        }

        /// <summary>
        /// Obtiene una lista de los días de la semana en base a un año X y una semana Y
        /// </summary>
        /// <param name="año"></param>
        /// <param name="nroSemana"></param>
        /// <returns>NumeroDeLaSemana, FechaDeInicio, FechaFinal</returns>
        public SelectSemana GetSemana(int año, int nroSemana)
        {
            DateTime dateTime = new DateTime(año, 1, 1);

            List<SelectSemana> list = new List<SelectSemana>();
            List<CTR_GetSemanasPorAñoResult> semanaList = dc.CTR_GetSemanasPorAño(dateTime).ToList();

            CTR_GetSemanasPorAñoResult result = semanaList.Single(X => X.WeekNumber == nroSemana);
            return new SelectSemana()
            {
                NumeroDeLaSemana = result.WeekNumber.Value,
                FechaDeInicio = result.StartDate.Value,
                FechaFinal = result.EndDate.Value
            };
        }

        /// <summary>
        /// Obtiene la lista completa de las semanas que tiene una año N
        /// </summary>
        /// <param name="año"></param>
        /// <returns>NumeroDeLaSemana, FechaDeInicio, FechaFinal</returns>
        public List<SelectSemana> GetSemanas(int año)
        {
            DateTime dateTime = new DateTime(año, 1, 1);

            List<SelectSemana> list = new List<SelectSemana>();
            List<CTR_GetSemanasPorAñoResult> semanaList = dc.CTR_GetSemanasPorAño(dateTime).ToList();
            foreach (var semana in semanaList)
            {
                list.Add(new SelectSemana()
                {
                    NumeroDeLaSemana = semana.WeekNumber.Value,
                    FechaDeInicio = semana.StartDate.Value,
                    FechaFinal = semana.EndDate.Value
                });
            }

            return list;
        }
    }
}
