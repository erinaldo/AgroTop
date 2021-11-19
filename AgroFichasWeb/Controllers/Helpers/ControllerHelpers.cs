using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Controllers
{
    public static class ControllerHelpers
    {
        public static void AddRuleViolations(this ModelStateDictionary modelState, IEnumerable<RuleViolation> errors)
        {
            foreach (RuleViolation issue in errors)
            {
                modelState.AddModelError(issue.PropertyName, issue.ErrorMessage);
            }
        }

        public static int LastWeek(int year)
        {
            return (new CultureInfo("es-CL")).Calendar.GetWeekOfYear(new DateTime(year, 12, 31), CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }

        public static int GetMonth(int year, int week)
        {
            DateTime tDt = new DateTime(year, 1, 1);

            tDt.AddDays((week - 1) * 7);

            for (int i = 0; i <= 365; ++i)
            {
                int tWeek = (new CultureInfo("es-CL")).Calendar.GetWeekOfYear(
                    tDt,
                    CalendarWeekRule.FirstDay,
                    DayOfWeek.Monday);
                if (tWeek == week)
                    return tDt.Month;

                tDt = tDt.AddDays(1);
            }
            return 0;
        }

    }
}