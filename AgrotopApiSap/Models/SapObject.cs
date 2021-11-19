using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgrotopApiSap.Models
{
    public class SapObject
    {
        public SAPbobsCOM.Company oCompany { get; set; }

        public SapObject()
        {
        }

        public void ResolveCredentials(int IdEmpresa, bool TESTMODE)
        {
            switch (IdEmpresa)
            {
                case 2:
                    if (TESTMODE)
                    {
                        this.oCompany               = (new SAPbobsCOM.Company());
                        this.oCompany.DbServerType  = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2014;
                        this.oCompany.DbUserName    = "sa";
                        this.oCompany.DbPassword    = "Agrotop2016*";
                        this.oCompany.Server        = "192.168.11.92";
                        this.oCompany.CompanyDB     = "Z_PRUEBA_AVENATOP_DOLAR";
                        //this.oCompany.UserName      = "manager";
                        //this.oCompany.Password      = "ats2735";
                        this.oCompany.UserName      = "fichas";
                        this.oCompany.Password      = "2735";
                        this.oCompany.LicenseServer = "192.168.11.92:30000";
                        this.oCompany.UseTrusted    = false;
                        this.oCompany.language      = SAPbobsCOM.BoSuppLangs.ln_Spanish_La;
                    }
                    else
                    {

                    }
                    break;
            }
        }
    }
}