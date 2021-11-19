using AgrotopApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgrotopApi.ViewModels
{
    public class LiquidacionViewModel
    {
        public bool OK { get; set; }

        public string Message { get; set; }

        public List<Liquidacion> Liquidaciones { get; set; }
    }
}