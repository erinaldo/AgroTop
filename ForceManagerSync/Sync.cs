using ForceManagerLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForceManagerSync
{
    class Sync
    {
        static AgroFichasDBDataContext dataContext = new AgroFichasDBDataContext();

        public static void dosync(List<Account> accounts)
        {
            Temporada temporada = dataContext.Temporada.OrderByDescending(X => X.IdTemporada).FirstOrDefault();

            foreach (var account in accounts)
            {
                CRM_Informacion informacion = dataContext.CRM_Informacion.SingleOrDefault(X => X.ID == account.id);
                if (informacion != null)
                {
                    // Actualización
                    informacion.NombreEmpresa                          = account.name;
                    informacion.Entorno                                = account.branchId != null ? account.branchId.value : "";
                    informacion.TipoEmpresa                            = account.typeId != null ? account.typeId.value : "";
                    informacion.Calificacion                           = account.rateId != null ? account.rateId.value : "";
                    informacion.Estado                                 = account.statusId != null ? account.statusId.value : "";
                    informacion.Rut                                    = !string.IsNullOrEmpty(account.vatNumber) ? account.vatNumber.Replace(".", "").ToLower().Trim() : "";
                    informacion.Responsable1                           = account.salesRepId1 != null ? account.salesRepId1.value : "";
                    informacion.Responsable2                           = account.salesRepId2 != null ? account.salesRepId2.value : "";
                    informacion.Responsable3                           = account.salesRepId3 != null ? account.salesRepId3.value : "";
                    informacion.Responsable4                           = account.salesRepId4 != null ? account.salesRepId4.value : "";
                    informacion.Responsable5                           = account.salesRepId5 != null ? account.salesRepId5.value : "";
                    informacion.EmpresaPublica                         = account.@public ?? false;
                    informacion.Comentarios                            = account.comment ?? "";
                    informacion.UserUpd                                = "dosync_update";
                    informacion.FechaHoraUpd                           = DateTime.Now;
                    informacion.IpUpd                                  = "Localhost";
                    informacion.IdAsesorAgricola1                      = GetSalesRep1(account.salesRepId1);
                    informacion.IdAsesorAgricola2                      = GetSalesRep2(account.salesRepId2);
                    informacion.IdAsesorAgricola3                      = GetSalesRep3(account.salesRepId3);
                    informacion.IdAsesorAgricola4                      = GetSalesRep4(account.salesRepId4);
                    informacion.IdAsesorAgricola5                      = GetSalesRep5(account.salesRepId5);

                    informacion.CRM_DatosContacto.Telefono1            = account.phone ?? "";
                    informacion.CRM_DatosContacto.Telefono2            = account.phone2 ?? "";
                    informacion.CRM_DatosContacto.Telefono3            = account.phone3 ?? "";
                    informacion.CRM_DatosContacto.Email                = account.email ?? "";
                    informacion.CRM_DatosContacto.Web                  = account.website ?? "";
                    informacion.CRM_DatosContacto.Direccion            = account.address1 ?? "";
                    informacion.CRM_DatosContacto.DetalleDeLaDireccion = account.address2 ?? "";
                    informacion.CRM_DatosContacto.Poblacion            = account.city ?? "";
                    informacion.CRM_DatosContacto.ProvReg              = account.region ?? "";
                    informacion.CRM_DatosContacto.CodigoPostal         = account.postcode ?? "";
                    informacion.CRM_DatosContacto.Pais                 = account.countryId != null ? account.countryId.value : "";
                    informacion.CRM_DatosContacto.Latitude             = account.latitude ?? 0;
                    informacion.CRM_DatosContacto.Longitude            = account.longitude ?? 0;
                    informacion.CRM_DatosContacto.UserUpd              = "dosync_update";
                    informacion.CRM_DatosContacto.FechaHoraUpd         = DateTime.Now;
                    informacion.CRM_DatosContacto.IpUpd                = "Localhost";

                    informacion.CRM_General.Comuna                     = account.Z_Comuna ?? "";
                    informacion.CRM_General.CuentaCorriente1           = (account.Z_CtaCorriente1 ?? 0).ToString();
                    informacion.CRM_General.CuentaCorriente2           = (account.Z_CtaCorriente2 ?? 0).ToString();
                    informacion.CRM_General.UserUpd                    = "dosync_update";
                    informacion.CRM_General.FechaHoraUpd               = DateTime.Now;
                    informacion.CRM_General.IpUpd                      = "Localhost";

                    informacion.CRM_DatosFiscales.Direccion            = account.Z_Direccion ?? "";
                    informacion.CRM_DatosFiscales.ComunaFiscal         = account.Z_ComunaFiscal ?? "";
                    informacion.CRM_DatosFiscales.Region               = account.Z_Region ?? "";
                    informacion.CRM_DatosFiscales.UserUpd              = "dosync_update";
                    informacion.CRM_DatosFiscales.FechaHoraUpd         = DateTime.Now;
                    informacion.CRM_DatosFiscales.IpUpd                = "Localhost";

                    //if (informacion.CRM_Objetivos.IdTemporada == temporada.IdTemporada)
                    //{
                    //    // Edita Objetivos de la temporada que viene 
                    //    informacion.CRM_Objetivos.HectareasAvena       = account.Z_HasAvena ?? 0;
                    //    informacion.CRM_Objetivos.Trimestre1Avena      = account.Z_T1Avena ?? 0;
                    //    informacion.CRM_Objetivos.Trimestre2Avena      = account.Z_T2Avena ?? 0;
                    //    informacion.CRM_Objetivos.Trimestre3Avena      = account.Z_T3Avena ?? 0;
                    //    informacion.CRM_Objetivos.Trimestre4Avena      = account.Z_T4Avena ?? 0;
                    //    informacion.CRM_Objetivos.HectareasLupino      = account.Z_HasLupino ?? 0;
                    //    informacion.CRM_Objetivos.Trimestre1Lupino     = account.Z_T1Lupino ?? 0;
                    //    informacion.CRM_Objetivos.Trimestre2Lupino     = account.Z_T2Lupino ?? 0;
                    //    informacion.CRM_Objetivos.Trimestre3Lupino     = account.Z_T3Lupino ?? 0;
                    //    informacion.CRM_Objetivos.Trimestre4Lupino     = account.Z_T4Lupino ?? 0;
                    //    informacion.CRM_Objetivos.HectareasRaps        = account.Z_HasRaps ?? 0;
                    //    informacion.CRM_Objetivos.Trimestre1Raps       = account.Z_T1Raps ?? 0;
                    //    informacion.CRM_Objetivos.Trimestre2Raps       = account.Z_T2Raps ?? 0;
                    //    informacion.CRM_Objetivos.Trimestre3Raps       = account.Z_T3Raps ?? 0;
                    //    informacion.CRM_Objetivos.Trimestre4Raps       = account.Z_T4Raps ?? 0;
                    //    informacion.CRM_Objetivos.HectareasTrigo       = account.Z_HasTrigo ?? 0;
                    //    informacion.CRM_Objetivos.Trimestre1Trigo      = account.Z_T1Trigo ?? 0;
                    //    informacion.CRM_Objetivos.Trimestre2Trigo      = account.Z_T2Trigo ?? 0;
                    //    informacion.CRM_Objetivos.Trimestre3Trigo      = account.Z_T3Trigo ?? 0;
                    //    informacion.CRM_Objetivos.Trimestre4Trigo      = account.Z_T4Trigo ?? 0;
                    //    informacion.CRM_Objetivos.UserUpd              = "dosync_update";
                    //    informacion.CRM_Objetivos.FechaHoraUpd         = DateTime.Now;
                    //    informacion.CRM_Objetivos.IpUpd                = "Localhost";
                    //}
                    //else
                    //{
                    //    // Crea Objetivos de la temporada que viene + 1
                    //    CRM_Objetivos objetivos = new CRM_Objetivos()
                    //    {
                    //        ID               = account.id,
                    //        HectareasAvena   = account.Z_HasAvena ?? 0,
                    //        Trimestre1Avena  = account.Z_T1Avena ?? 0,
                    //        Trimestre2Avena  = account.Z_T2Avena ?? 0,
                    //        Trimestre3Avena  = account.Z_T3Avena ?? 0,
                    //        Trimestre4Avena  = account.Z_T4Avena ?? 0,
                    //        HectareasLupino  = account.Z_HasLupino ?? 0,
                    //        Trimestre1Lupino = account.Z_T1Lupino ?? 0,
                    //        Trimestre2Lupino = account.Z_T2Lupino ?? 0,
                    //        Trimestre3Lupino = account.Z_T3Lupino ?? 0,
                    //        Trimestre4Lupino = account.Z_T4Lupino ?? 0,
                    //        HectareasRaps    = account.Z_HasRaps ?? 0,
                    //        Trimestre1Raps   = account.Z_T1Raps ?? 0,
                    //        Trimestre2Raps   = account.Z_T2Raps ?? 0,
                    //        Trimestre3Raps   = account.Z_T3Raps ?? 0,
                    //        Trimestre4Raps   = account.Z_T4Raps ?? 0,
                    //        HectareasTrigo   = account.Z_HasTrigo ?? 0,
                    //        Trimestre1Trigo  = account.Z_T1Trigo ?? 0,
                    //        Trimestre2Trigo  = account.Z_T2Trigo ?? 0,
                    //        Trimestre3Trigo  = account.Z_T3Trigo ?? 0,
                    //        Trimestre4Trigo  = account.Z_T4Trigo ?? 0,
                    //        UserIns          = "dosync",
                    //        FechaHoraIns     = DateTime.Now,
                    //        IpIns            = "Localhost",
                    //        UserUpd          = null,
                    //        FechaHoraUpd     = null,
                    //        IpUpd            = null,
                    //        IdTemporada      = temporada.IdTemporada
                    //    };
                    //    dataContext.CRM_Objetivos.InsertOnSubmit(objetivos);
                    //}

                    //dataContext.SubmitChanges();
                }
                else
                {
                    // Crea
                    informacion = new CRM_Informacion()
                    {
                        ID                   = account.id,
                        NombreEmpresa        = account.name,
                        Entorno              = account.branchId != null ? account.branchId.value : "",
                        TipoEmpresa          = account.typeId != null ? account.typeId.value : "",
                        Calificacion         = account.rateId != null ? account.rateId.value : "",
                        Estado               = account.statusId != null ? account.statusId.value : "",
                        Rut                  = account.vatNumber ?? "",
                        Responsable1         = account.salesRepId1 != null ? account.salesRepId1.value : "",
                        Responsable2         = account.salesRepId2 != null ? account.salesRepId2.value : "",
                        Responsable3         = account.salesRepId3 != null ? account.salesRepId3.value : "",
                        Responsable4         = account.salesRepId4 != null ? account.salesRepId4.value : "",
                        Responsable5         = account.salesRepId5 != null ? account.salesRepId5.value : "",
                        EmpresaPublica       = account.@public ?? false,
                        Comentarios          = account.comment ?? "",
                        Habilitado           = true,
                        UserIns              = "dosync",
                        FechaHoraIns         = DateTime.Now,
                        IpIns                = "Localhost",
                        UserUpd              = null,
                        FechaHoraUpd         = null,
                        IpUpd                = null,
                        IdAsesorAgricola1    = GetSalesRep1(account.salesRepId1),
                        IdAsesorAgricola2    = GetSalesRep2(account.salesRepId2),
                        IdAsesorAgricola3    = GetSalesRep3(account.salesRepId3),
                        IdAsesorAgricola4    = GetSalesRep4(account.salesRepId4),
                        IdAsesorAgricola5    = GetSalesRep5(account.salesRepId5),
                    };
                    dataContext.CRM_Informacion.InsertOnSubmit(informacion);

                    CRM_DatosContacto datosContacto = new CRM_DatosContacto()
                    {
                        ID                   = account.id,
                        Telefono1            = account.phone ?? "",
                        Telefono2            = account.phone2 ?? "",
                        Telefono3            = account.phone3 ?? "",
                        Email                = account.email ?? "",
                        Web                  = account.website ?? "",
                        Direccion            = account.address1 ?? "",
                        DetalleDeLaDireccion = account.address2 ?? "",
                        Poblacion            = account.city ?? "",
                        ProvReg              = account.region ?? "",
                        CodigoPostal         = account.postcode ?? "",
                        Pais                 = account.countryId != null ? account.countryId.value : "",
                        Latitude             = account.latitude ?? 0,
                        Longitude            = account.longitude ?? 0,
                        UserIns              = "dosync",
                        FechaHoraIns         = DateTime.Now,
                        IpIns                = "Localhost",
                        UserUpd              = null,
                        FechaHoraUpd         = null,
                        IpUpd                = null,
                    };
                    dataContext.CRM_DatosContacto.InsertOnSubmit(datosContacto);

                    CRM_General general = new CRM_General()
                    {
                        ID                   = account.id,
                        Comuna               = account.Z_Comuna ?? "",
                        CuentaCorriente1     = (account.Z_CtaCorriente1 ?? 0).ToString(),
                        CuentaCorriente2     = (account.Z_CtaCorriente1 ?? 0).ToString(),
                        UserIns              = "dosync",
                        FechaHoraIns         = DateTime.Now,
                        IpIns                = "Localhost",
                        UserUpd              = null,
                        FechaHoraUpd         = null,
                        IpUpd                = null,
                    };
                    dataContext.CRM_General.InsertOnSubmit(general);

                    CRM_DatosFiscales datosFiscales = new CRM_DatosFiscales()
                    {
                        ID                   = account.id,
                        Direccion            = account.Z_Direccion ?? "",
                        ComunaFiscal         = account.Z_ComunaFiscal ?? "",
                        Region               = account.Z_Region ?? "",
                        UserIns              = "dosync",
                        FechaHoraIns         = DateTime.Now,
                        IpIns                = "Localhost",
                        UserUpd              = null,
                        FechaHoraUpd         = null,
                        IpUpd                = null,
                    };
                    dataContext.CRM_DatosFiscales.InsertOnSubmit(datosFiscales);

                    //CRM_Objetivos objetivos = new CRM_Objetivos()
                    //{
                    //    ID                   = account.id,
                    //    HectareasAvena       = account.Z_HasAvena ?? 0,
                    //    Trimestre1Avena      = account.Z_T1Avena ?? 0,
                    //    Trimestre2Avena      = account.Z_T2Avena ?? 0,
                    //    Trimestre3Avena      = account.Z_T3Avena ?? 0,
                    //    Trimestre4Avena      = account.Z_T4Avena ?? 0,
                    //    HectareasLupino      = account.Z_HasLupino ?? 0,
                    //    Trimestre1Lupino     = account.Z_T1Lupino ?? 0,
                    //    Trimestre2Lupino     = account.Z_T2Lupino ?? 0,
                    //    Trimestre3Lupino     = account.Z_T3Lupino ?? 0,
                    //    Trimestre4Lupino     = account.Z_T4Lupino ?? 0,
                    //    HectareasRaps        = account.Z_HasRaps ?? 0,
                    //    Trimestre1Raps       = account.Z_T1Raps ?? 0,
                    //    Trimestre2Raps       = account.Z_T2Raps ?? 0,
                    //    Trimestre3Raps       = account.Z_T3Raps ?? 0,
                    //    Trimestre4Raps       = account.Z_T4Raps ?? 0,
                    //    HectareasTrigo       = account.Z_HasTrigo ?? 0,
                    //    Trimestre1Trigo      = account.Z_T1Trigo ?? 0,
                    //    Trimestre2Trigo      = account.Z_T2Trigo ?? 0,
                    //    Trimestre3Trigo      = account.Z_T3Trigo ?? 0,
                    //    Trimestre4Trigo      = account.Z_T4Trigo ?? 0,
                    //    UserIns              = "dosync",
                    //    FechaHoraIns         = DateTime.Now,
                    //    IpIns                = "Localhost",
                    //    UserUpd              = null,
                    //    FechaHoraUpd         = null,
                    //    IpUpd                = null,
                    //    IdTemporada          = temporada.IdTemporada
                    //};
                    //dataContext.CRM_Objetivos.InsertOnSubmit(objetivos);
                    dataContext.SubmitChanges();
                }
            }
        }

        #region MÉTODOS PRIVADOS

        private static Nullable<int> GetSalesRep1(salesRepId1 salesRepId1)
        {
            if (salesRepId1 != null)
                return salesRepId1.id;
            else
                return null;
        }

        private static Nullable<int> GetSalesRep2(salesRepId2 salesRepId2)
        {
            if (salesRepId2 != null)
                return salesRepId2.id;
            else
                return null;
        }

        private static Nullable<int> GetSalesRep3(salesRepId3 salesRepId3)
        {
            if (salesRepId3 != null)
                return salesRepId3.id;
            else
                return null;
        }

        private static Nullable<int> GetSalesRep4(salesRepId4 salesRepId4)
        {
            if (salesRepId4 != null)
                return salesRepId4.id;
            else
                return null;
        }

        private static Nullable<int> GetSalesRep5(salesRepId5 salesRepId5)
        {
            if (salesRepId5 != null)
                return salesRepId5.id;
            else
                return null;
        }

        #endregion
    }
}