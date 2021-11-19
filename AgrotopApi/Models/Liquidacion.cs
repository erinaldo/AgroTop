using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgrotopApi.Models
{
    public class Liquidacion
    {
        #region Propiedades

        public Proyecto Proyecto { get; set; }
        public EmpresaLiquidacion Empresa { get; set; }
        public Proveedor Proveedor { get; set; }
        public int IdLiquidacion { get; set; }
        public Estado Estado { get; set; }
        public DateTime FechaDocumento { get; set; }
        public List<Glosa> Glosas { get; set; }

        #endregion

        public static List<Liquidacion> GetLiquidaciones()
        {
            var dc = new AgroFichasDBDataContext();
            var liquidaciones = new List<Liquidacion>();

            var glosasAgrupadas = dc.OC_LiquidacionesAgrupadas().ToList();
            var glosas = dc.OC_Liquidaciones().ToList();

            if (glosasAgrupadas.Count > 0)
            {
                foreach (var glosasAgrupada in glosasAgrupadas)
                {
                    var liquidacion = new Liquidacion();
                    liquidacion.Proyecto = new Proyecto()
                    {
                        Id = glosasAgrupada.IdProyecto,
                        Descripcion = glosasAgrupada.DescripcionProyecto
                    };
                    liquidacion.IdLiquidacion = glosasAgrupada.IdLiquidacion;
                    var glosaAux = glosas.FirstOrDefault(X => X.IdProyecto == glosasAgrupada.IdProyecto && X.IdLiquidacion == glosasAgrupada.IdLiquidacion);
                    liquidacion.Empresa = new EmpresaLiquidacion()
                    {
                        Id = glosaAux.IdEmpresa,
                        Nombre = glosaAux.NombreEmpresa
                    };
                    liquidacion.Estado = new Estado()
                    {
                        Id = glosaAux.IdEstado,
                        Descripcion = glosaAux.DescripcionEstado
                    };
                    liquidacion.FechaDocumento = glosaAux.FechaDocumento;
                    liquidacion.Proveedor = new Proveedor()
                    {
                        Id = glosaAux.IdProveedor,
                        Nombre = glosaAux.NombreProveedor
                    };
                    liquidacion.Glosas = new List<Glosa>();
                    foreach (var glosa in glosas.Where(X => X.IdProyecto == glosasAgrupada.IdProyecto && X.IdLiquidacion == glosasAgrupada.IdLiquidacion))
                    {
                        liquidacion.Glosas.Add(new Glosa()
                        {
                            CentroCosto = new CentroCosto()
                            {
                                Id = glosa.IdCentroCosto,
                                Nombre = glosa.NombreCentroCosto
                            },
                            Material = new Material()
                            {
                                Id = glosa.IdMaterial,
                                Nombre = glosa.NombreMaterial
                            },
                            Moneda = glosa.Moneda,
                            Cantidad = glosa.Cantidad,
                            PrecioUnitario = glosa.PrecioUnitario,
                            PrecioTotal = glosa.PrecioTotal,
                            CondicionPago = glosa.CondicionPago
                        });
                    }
                    liquidaciones.Add(liquidacion);
                }
            }

            return liquidaciones;
        }
    }

    public class Glosa
    {
        #region Propiedades

        public CentroCosto CentroCosto { get; set; }
        public Material Material { get; set; }
        public string Moneda { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioTotal { get; set; }
        public string CondicionPago { get; set; }

        #endregion
    }

    public class EmpresaLiquidacion
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }

    public class Proyecto
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
    }

    public class Estado
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
    }

    public class Proveedor
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
    }

    public class CentroCosto
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
    }

    public class Material
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
    }
}