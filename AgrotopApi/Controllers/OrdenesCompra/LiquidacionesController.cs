using AgrotopApi.Models;
using AgrotopApi.ViewModels;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Mvc;

namespace AgrotopApi.Controllers
{
    public class LiquidacionesController : BaseApplicationController
    {
        private static Logger logger = LogManager.GetLogger("fileLogger");

        public LiquidacionViewModel Get(string privateKey)
        {
            var dc = new AgroFichasDBDataContext();
            var model = new LiquidacionViewModel();

            string errMsg = "";

            logger.Debug("Controller = Liquidaciones, Method = Get");
            logger.Debug("privateKey = " + privateKey);

            try
            {
                var user = Account.CheckPrivateKey(privateKey, out errMsg);
                if (user == null)
                    throw new Exception("Unathorized");
                else
                    CurrentUser = user;

                logger.Info(string.Format("FullName = {0}, Email = {1}, PrivateKey = {2}", user.FullName, user.Email, user.PrivateKey));

                if (CheckPermiso(1) == false)
                {
                    errMsg = "No tienes los permisos de acceso suficientes para obtener las liquidaciones";
                    throw new Exception("Insufficient Permissions");
                }

                model.OK = true;
                model.Message = "OK";
                model.Liquidaciones = Liquidacion.GetLiquidaciones();

                logger.Debug(JsonConvert.SerializeObject(model));

                return model;
            }
            catch (Exception ex)
            {
                logger.Error("errMsg = " + errMsg);
                logger.Error(ex, ex.Message);

                model.OK = false;
                model.Message = errMsg;
                model.Liquidaciones = null;
                return model;
            }
        }
    }
}
