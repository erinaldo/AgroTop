
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.Models
{
    public partial class LOG_ControlCamionPlanta
    {
        private AgroFichasDBDataContext dc = new AgroFichasDBDataContext();

        public bool IsValid
        {
            get { return (GetRuleViolations().Count() == 0); }
        }

        public IEnumerable<RuleViolation> GetRuleViolations()
        {
            if (this.IdPedido == 0)
                yield return new RuleViolation("El número de pedido es requerido", "IdPedido");

            string error = "";
            if (ValidarPedido(out error) == false)
                yield return new RuleViolation(error, "IdPedido");

            yield break;
        }

        partial void OnValidate(System.Data.Linq.ChangeAction action)
        {
            if (!IsValid)
                throw new ApplicationException("Rule violations prevent saving");
        }

        private bool ValidarPedido(out string errMsg)
        {
            errMsg = "";
            var pedido = dc.LOG_Pedido.SingleOrDefault(x => x.IdPedido == this.IdPedido);
            if (pedido != null)
            {
                if (pedido.IdEstado == 2)//En tránsito a planta de origen
                {
                    if (this.PasoActual == 1)//Llegada a portería de origen
                    {
                        //No hay registros del camión en planta
                        if (GetEstadoActualControlCamionEnPlanta(this.IdPedido) == 0)
                        {
                            return true;
                        }
                        else
                        {
                            errMsg = "El voucher ya ha sido creado anteriormente.";
                            return false;
                        }
                    }
                    else//Registra pasos 2, 3 y 4
                    {
                        if (GetEstadoActualControlCamionEnPlanta(this.IdPedido) == 4)//Salida por portería de origen
                        {
                            errMsg = "Se han registrado todos los pasos en planta de origen.";
                            return false;
                        }

                        return true;
                    }
                }

                if (pedido.IdEstado == 3 || pedido.IdEstado == 4)//En tránsito a planta de destino
                {
                    if (this.PasoActual == 5)//Llegada a portería de destino
                    {
                        if (GetEstadoActualControlCamionEnPlanta(this.IdPedido) == 4)//Salida por portería de origen
                        {
                            return true;
                        }
                        else
                        {
                            errMsg = "El pedido aun no ha completado todas las fases (entrada por portería, inicio carga, término carga, salida por portería) en planta de origen.";
                            return false;
                        }
                    }
                    else//Registra pasos 6, 7 y 8
                    {
                        if (GetEstadoActualControlCamionEnPlanta(this.IdPedido) == 8)//Salida por portería de destino
                        {
                            errMsg = "Se han registrado todos los pasos en planta de destino.";
                            return false;
                        }

                        return true;
                    }
                }

                errMsg = "El pedido no esta marcado como en tránsito a planta origen/destino.";
                return false;
            }
            else
            {
                errMsg = "El pedido no existe.";
                return false;
            }
        }

        private int GetEstadoActualControlCamionEnPlanta(int IdPedido)
        {
            var controlCamionPlanta = dc.LOG_ControlCamionPlanta.Where(x => x.IdPedido == IdPedido).OrderByDescending(x => x.IdEstadoControlCamionPlanta).FirstOrDefault();
            if (controlCamionPlanta != null)
            {
                return controlCamionPlanta.IdEstadoControlCamionPlanta;
            }
            else
            {
                return 0;
            }
        }
    }
}