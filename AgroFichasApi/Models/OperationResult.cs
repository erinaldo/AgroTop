using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgroFichasApi.Models
{
    public class OperationResult
    {
        public bool OK { get; set; }
        public string Msg { get; set; }
        public string LogCode { get; set; }
        public Exception LogException { get; set; }

        private OperationResult()
        {

        }

        public OperationResult(bool ok, string msg, string logCode, Exception logException)
        {
            this.OK = ok;
            this.Msg = msg;
            this.LogCode = logCode;
            this.LogException = logException;
        }

        public override string ToString()
        {
            return "OK        => " + OK + "\r\n" +
                   "Msg       => " + Msg + "\r\n" +
                   "LogCode   => " + LogCode + "\r\n" +
                   "Exception => " + (LogException != null ? LogException.ToString() : "(null)");
        }



    }
}