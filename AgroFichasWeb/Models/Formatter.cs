namespace AgroFichasWeb.Models
{
    public class Formatter
    {
        public static string Format(decimal ValidValue, string UM, string FormatString)
        {
            if (ValidValue == 0 && UM.ToLower() == "ausencia")
                return "Ausencia";
            else if (ValidValue != 0 && ValidValue != 999999)
            {
                if (UM == "%" || (FormatString == "P2" || FormatString == "p2"))
                    return (ValidValue / 100m).ToString(FormatString);
                else
                    return (ValidValue.ToString(FormatString));
            }
            else if (ValidValue == 999999)
                return "--";
            else
                return "0";
        }

        public static string Format(decimal ValidValue, string UM, string FormatString, int? IdParametroAnalisis = null)
        {
            if (ValidValue == 0 && UM.ToLower() == "ausencia")
                return "Ausencia";
            else if (ValidValue != 0 && ValidValue != 999999)
            {
                if (UM == "%" || (FormatString == "P2" || FormatString == "p2"))
                    return (ValidValue / 100m).ToString(FormatString);
                else
                    return (ValidValue.ToString(FormatString));
            }
            else if (IdParametroAnalisis.HasValue && IdParametroAnalisis.Value == 92 && ValidValue == 0)
                return "Negativa";
            else if (ValidValue == 999999)
                return "--";
            else
                return "0";
        }

        public static string Format_en(decimal ValidValue, string UM, string FormatString)
        {
            if ((ValidValue == 0 && UM.ToLower() == "absentia") || ValidValue == 0 && UM.ToLower() == "absence" || ValidValue == 0 && UM.ToLower() == "absent")
                return UM.ToTitleCase();
            else if (ValidValue != 0 && ValidValue != 999999)
            {
                if (UM == "%" || (FormatString == "P2" || FormatString == "p2"))
                    return (ValidValue / 100m).ToString(FormatString);
                else
                    return (ValidValue.ToString(FormatString));
            }
            else if (ValidValue == 999999)
                return "--";
            else
                return "0";
        }

        public static string Format_en(decimal ValidValue, string UM, string FormatString, int? IdParametroAnalisis = null)
        {
            if ((ValidValue == 0 && UM.ToLower() == "absentia") || ValidValue == 0 && UM.ToLower() == "absence" || ValidValue == 0 && UM.ToLower() == "absent")
                return UM.ToTitleCase();
            else if (ValidValue != 0 && ValidValue != 999999)
            {
                if (UM == "%" || (FormatString == "P2" || FormatString == "p2"))
                    return (ValidValue / 100m).ToString(FormatString);
                else
                    return (ValidValue.ToString(FormatString));
            }
            else if (IdParametroAnalisis.HasValue && IdParametroAnalisis.Value == 92 && ValidValue == 0)
                return "Negative";
            else if (ValidValue == 999999)
                return "--";
            else
                return "0";
        }
    }
}