using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasWeb.Models
{
    public partial class CTR_Producto
    {
        AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public bool EmpresasValidas { get; set; }

        public bool EnvasesValidos { get; set; }

        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (String.IsNullOrEmpty(this.Nombre))
                yield return new RuleViolation("El nombre es requerido", "Nombre");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }

        public List<CTR_ProductoEmpresa> GetEmpresas(int IdProducto)
        {
            List<CTR_ProductoEmpresa> list = new List<Models.CTR_ProductoEmpresa>();

            var empresas = dc.CTR_GetEmpresasPorIdProducto(IdProducto);

            foreach (var empresa in empresas)
            {
                list.Add(new Models.CTR_ProductoEmpresa()
                {
                    IdProducto = IdProducto,
                    IdEmpresa = empresa.IdEmpresa,
                    Empresa = dc.Empresa.Single(X => X.IdEmpresa == empresa.IdEmpresa),
                    Tiene = empresa.Tiene.Value
                });
            }

            return list;
        }
        public List<CTR_ProductoPlanta> GetPlantas(int UserID, int IdProducto)
        {
            List<CTR_ProductoPlanta> list = new List<Models.CTR_ProductoPlanta>();

            if (IdProducto == 0)
            {
                var plantas = (from p in dc.PlantaProduccion
                               join pu in dc.PlantaUsuario on p.IdPlantaProduccion equals pu.IdPlantaProduccion
                               where (p.Habilitado == true) &&
                               (pu.UserID == UserID)
                               select p).ToList();

                foreach (var planta in plantas)
                {
                    list.Add(new Models.CTR_ProductoPlanta()
                    {
                        IdProducto = IdProducto,
                        IdPlantaProduccion = planta.IdPlantaProduccion,
                        PlantaProduccion = dc.PlantaProduccion.Single(X => X.IdPlantaProduccion == planta.IdPlantaProduccion),
                        Tiene = false,
                        TienePlanta = true
                    });
                }
            }
            else
            {
                var plantas = dc.CTR_GetPlantasPorIdProducto(UserID, IdProducto);
                foreach (var planta in plantas)
                {
                    list.Add(new Models.CTR_ProductoPlanta()
                    {
                        IdProducto = IdProducto,
                        IdPlantaProduccion = planta.IdPlantaProduccion,
                        PlantaProduccion = dc.PlantaProduccion.Single(X => X.IdPlantaProduccion == planta.IdPlantaProduccion),
                        Tiene = planta.Tiene.Value,
                        TienePlanta = planta.TienePlanta.Value
                    });
                }
            }


            return list;
        }

        public List<CTR_ProductoEnvase> GetEnvases(int IdProducto)
        {
            List<CTR_ProductoEnvase> list = new List<Models.CTR_ProductoEnvase>();

            var envases = dc.CTR_GetEnvasesPorIdProducto(IdProducto);

            foreach (var envase in envases)
            {
                list.Add(new Models.CTR_ProductoEnvase()
                {
                    IdProducto = IdProducto,
                    IdEnvase = envase.IdEnvase,
                    CTR_Envase = dc.CTR_Envase.Single(X => X.IdEnvase == envase.IdEnvase),
                    Tiene = envase.Tiene.Value
                });
            }

            return list;
        }

       
        public bool ValidacionEntrada(ModelStateDictionary modelState, HttpContext httpContext)
        {
            string errMsg = "";
            bool returnValue = true;
            if (string.IsNullOrEmpty(httpContext.Request["chkEmpresa"]))
            {
                errMsg = "Debe seleccionar al menos una empresa";
                modelState.AddModelError("EmpresasValidas", errMsg);
                returnValue = false;
            }

            if (returnValue && string.IsNullOrEmpty(httpContext.Request["chkEnvase"]))
            {
                errMsg = "Debe seleccionar al menos un envase";
                modelState.AddModelError("EnvasesValidos", errMsg);
                returnValue = false;
            }

            return returnValue;
        }
    }
}