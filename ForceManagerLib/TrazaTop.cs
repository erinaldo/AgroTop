using ForceManagerLib.Models.Genericos;
using ForceManagerLib.Models.Requests;
using ForceManagerLib.Models.Results;
using ForceManagerLib.Models.TrazaTop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ForceManagerLib
{
    public class TrazaTop
    {
        private AgroFichasDBDataContext context = new AgroFichasDBDataContext();

        public void SincronizarSolicitudesContratos(bool isDevelopment = false)
        {
            try
            {
                Proxy proxy = new Proxy();
                string token = proxy.Login();

                int page = 0;
                bool condition = false;

                do
                {
                    string endpoint = string.Format("https://api.forcemanager.net/api/v4/activities?page={0}&where=typeId.id=14", page);
                    if (isDevelopment)
                    {
                        // Agricultor de Prueba
                        endpoint = string.Format("{0}%20and%20accountId.id=1268", endpoint);
                    }

                    CRM_Solicitud solicitud = new CRM_Solicitud()
                    {
                        Tipo         = "Solicitud de Contrato",
                        ApiEndpoint  = endpoint,
                        Pagina       = page,
                        Respuesta    = "",
                        FechaHoraIns = DateTime.Now,
                        IpIns        = "localhost",
                        UserIns      = "SincronizarSolicitudesContratos"
                    };
                    context.CRM_Solicitud.InsertOnSubmit(solicitud);
                    context.SubmitChanges();

                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(endpoint);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Accept = "*/*";
                    httpWebRequest.Headers.Add("X-Session-Key", token);
                    httpWebRequest.Method = "GET";

                    HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        solicitud.Respuesta = result;
                        context.SubmitChanges();

                        List<JsonResultSolicitudContrato> jsonResults = JsonConvert.DeserializeObject<List<JsonResultSolicitudContrato>>(result);
                        if (jsonResults.Count < 50)
                            condition = true;

                        foreach (JsonResultSolicitudContrato jsonResult in jsonResults)
                        {
                            SolicitudContrato solicitudContrato = context.SolicitudContrato.SingleOrDefault(sc => sc.IdSolicitudContrato == jsonResult.id);
                            CRM_AsesorAgricola asesorAgricola = context.CRM_AsesorAgricola.SingleOrDefault(aa => aa.ID == (jsonResult.salesRepId != null ? jsonResult.salesRepId.id : 0));

                            if (solicitudContrato == null)
                            {
                                solicitudContrato = new SolicitudContrato()
                                {
                                    IdSolicitudContrato  = jsonResult.id,
                                    Rut                  = jsonResult.Z_RUT ?? "",
                                    NombreProveedor      = jsonResult.accountId.value,
                                    IdCultivo            = GetCultivo(jsonResult.Z_idCultivo),
                                    Cultivo              = jsonResult.Z_idCultivo != null ? jsonResult.Z_idCultivo.value : "",
                                    PrecioCierre         = jsonResult.Z_PrecioCierre ?? 0,
                                    ToneladasCierre      = jsonResult.Z_ToneladasCierre ?? 0,
                                    IdTipoContrato       = GetTipoContrato(jsonResult.Z_idTipoContrato),
                                    TipoContrato         = jsonResult.Z_idTipoContrato != null ? jsonResult.Z_idTipoContrato.value : "",
                                    IdComunaOrigen       = GetComunaOrigen(jsonResult.Z_idComunaOrigen),
                                    ComunaOrigen         = jsonResult.Z_idComunaOrigen != null ? jsonResult.Z_idComunaOrigen.value : "",
                                    IdSucursalEntrega    = GetSucursalEntrega(jsonResult.Z_idSucursalEntrega),
                                    SucursalEntrega      = jsonResult.Z_idSucursalEntrega != null ? jsonResult.Z_idSucursalEntrega.value : "",
                                    Hectareas            = jsonResult.Z_HectareasContrato ?? 0,
                                    ToneladasTotales     = jsonResult.Z_Toneladas ?? 0,
                                    Predio               = jsonResult.Z_Predio ?? "",
                                    VerificadoCRM        = jsonResult.Z_idVerificado != null ? (jsonResult.Z_idVerificado.id == 2 ? true : false) : false,
                                    VerificadoFichas     = false,
                                    Autorizado           = false,
                                    ContratoCreado       = jsonResult.Z_Contrato_Creado.HasValue ? jsonResult.Z_Contrato_Creado.Value : false,
                                    CierreCreado         = jsonResult.Z_Cierre_Creado.HasValue ? jsonResult.Z_Cierre_Creado.Value : false,
                                    IdTemporada          = GetTemporada(jsonResult.Z_Temporadas),
                                    Temporada            = jsonResult.Z_Temporadas != null ? jsonResult.Z_Temporadas.value : "",
                                    IdEstado             = jsonResult.Z_idVerificado != null ? jsonResult.Z_idVerificado.id : 1,
                                    NombreAsesor         = string.Format("{0} {1}", asesorAgricola.Nombre, asesorAgricola.Apellido),
                                    EmailAsesor          = asesorAgricola.Email,
                                    FechaHoraIns         = DateTime.Now,
                                    IpIns                = "localhost",
                                    UserIns              = "SincronizarSolicitudesContratos",
                                    GastosTransportePara = "",
                                    PDFCreado            = false
                                };
                                context.SolicitudContrato.InsertOnSubmit(solicitudContrato);
                            }
                            else
                            {
                                solicitudContrato.Rut               = jsonResult.Z_RUT ?? "";
                                solicitudContrato.NombreProveedor   = jsonResult.accountId.value;
                                solicitudContrato.IdCultivo         = GetCultivo(jsonResult.Z_idCultivo);
                                solicitudContrato.Cultivo           = jsonResult.Z_idCultivo != null ? jsonResult.Z_idCultivo.value : "";
                                solicitudContrato.PrecioCierre      = jsonResult.Z_PrecioCierre ?? 0;
                                solicitudContrato.ToneladasCierre   = jsonResult.Z_ToneladasCierre ?? 0;
                                solicitudContrato.IdTipoContrato    = GetTipoContrato(jsonResult.Z_idTipoContrato);
                                solicitudContrato.TipoContrato      = jsonResult.Z_idTipoContrato != null ? jsonResult.Z_idTipoContrato.value : "";
                                solicitudContrato.IdComunaOrigen    = GetComunaOrigen(jsonResult.Z_idComunaOrigen);
                                solicitudContrato.ComunaOrigen      = jsonResult.Z_idComunaOrigen != null ? jsonResult.Z_idComunaOrigen.value : "";
                                solicitudContrato.IdSucursalEntrega = GetSucursalEntrega(jsonResult.Z_idSucursalEntrega);
                                solicitudContrato.SucursalEntrega   = jsonResult.Z_idSucursalEntrega != null ? jsonResult.Z_idSucursalEntrega.value : "";
                                solicitudContrato.Hectareas         = jsonResult.Z_HectareasContrato ?? 0;
                                solicitudContrato.ToneladasTotales  = jsonResult.Z_Toneladas ?? 0;
                                solicitudContrato.Predio            = jsonResult.Z_Predio ?? "";
                                solicitudContrato.VerificadoCRM     = jsonResult.Z_idVerificado != null ? (jsonResult.Z_idVerificado.id == 2 ? true : false) : false;
                                solicitudContrato.ContratoCreado    = jsonResult.Z_Contrato_Creado.HasValue ? jsonResult.Z_Contrato_Creado.Value : false;
                                solicitudContrato.CierreCreado      = jsonResult.Z_Cierre_Creado.HasValue ? jsonResult.Z_Cierre_Creado.Value : false;
                                solicitudContrato.IdTemporada       = GetTemporada(jsonResult.Z_Temporadas);
                                solicitudContrato.Temporada         = jsonResult.Z_Temporadas != null ? jsonResult.Z_Temporadas.value : "";
                                solicitudContrato.IdEstado          = jsonResult.Z_idVerificado != null ? jsonResult.Z_idVerificado.id : 1;
                                solicitudContrato.NombreAsesor      = string.Format("{0} {1}", asesorAgricola.Nombre, asesorAgricola.Apellido);
                                solicitudContrato.EmailAsesor       = asesorAgricola.Email;
                                solicitudContrato.FechaHoraUpd      = DateTime.Now;
                                solicitudContrato.IpUpd             = "localhost";
                                solicitudContrato.UserUpd           = "SincronizarSolicitudesContratos";
                            }

                            //Cultivo 1
		                    //    Variedad 1
			                //        Superficie
			                //        Tons (Kilos)
		                    //    Variedad 2
			                //        Superficie
			                //        Tons (Kilos)
                            if (jsonResult.Z_idCultivo != null && jsonResult.Z_idVariedadSolicitudContrato != null)
                            {
                                foreach (SolicitudContratoVariedad solicitudContratoVariedad in context.SolicitudContratoVariedad.Where(scv => scv.IdSolicitudContrato == solicitudContrato.IdSolicitudContrato))
                                    if (jsonResult.Z_idVariedadSolicitudContrato.SingleOrDefault(ivsc => ivsc.id == solicitudContratoVariedad.IdVariedad) == null)
                                        context.SolicitudContratoVariedad.DeleteOnSubmit(solicitudContratoVariedad);

                                foreach (Z_idVariedadSolicitudContrato z_IdVariedadSolicitudContrato in jsonResult.Z_idVariedadSolicitudContrato)
                                {
                                    SolicitudContratoVariedad solicitudContratoVariedad = context.SolicitudContratoVariedad.SingleOrDefault(scv => scv.IdSolicitudContrato == solicitudContrato.IdSolicitudContrato && scv.IdVariedad == z_IdVariedadSolicitudContrato.id);
                                    if (solicitudContratoVariedad != null)
                                    {
                                        solicitudContratoVariedad.Hectareas    = jsonResult.Z_HectareasContrato ?? 0;
                                        solicitudContratoVariedad.Toneladas    = jsonResult.Z_Toneladas ?? 0;
                                        solicitudContratoVariedad.FechaHoraUpd = DateTime.Now;
                                        solicitudContratoVariedad.IpUpd        = "localhost";
                                        solicitudContratoVariedad.UserUpd      = "SincronizarSolicitudesContratos";
                                    }
                                    else
                                    {
                                        solicitudContratoVariedad = new SolicitudContratoVariedad();
                                        solicitudContratoVariedad.IdSolicitudContrato = solicitudContrato.IdSolicitudContrato;
                                        solicitudContratoVariedad.IdVariedad          = z_IdVariedadSolicitudContrato.id;
                                        solicitudContratoVariedad.Variedad            = z_IdVariedadSolicitudContrato.value;
                                        solicitudContratoVariedad.Hectareas           = jsonResult.Z_HectareasContrato ?? 0;
                                        solicitudContratoVariedad.Toneladas           = jsonResult.Z_Toneladas ?? 0;
                                        solicitudContratoVariedad.FechaHoraIns        = DateTime.Now;
                                        solicitudContratoVariedad.IpIns               = "localhost";
                                        solicitudContratoVariedad.UserIns             = "SincronizarSolicitudesContratos";
                                        context.SolicitudContratoVariedad.InsertOnSubmit(solicitudContratoVariedad);
                                    }
                                }
                            }
                            else
                            {
                                context.SolicitudContratoVariedad.DeleteAllOnSubmit(context.SolicitudContratoVariedad.Where(scv => scv.IdSolicitudContrato == solicitudContrato.IdSolicitudContrato));
                            }

                            context.SubmitChanges();
                        }
                    }

                    httpWebResponse.Close();

                    page++;
                } while (!condition);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void SincronizarAsesoresAgricolas()
        {
            try
            {
                Proxy proxy = new Proxy();
                string token = proxy.Login();

                int page = 0;
                bool condition = false;

                do
                {
                    string endpoint = string.Format("https://api.forcemanager.net/api/v4/users?page={0}", page);

                    CRM_Solicitud solicitud = new CRM_Solicitud()
                    {
                        Tipo         = "Sincronización de Asesores Agrícolas",
                        ApiEndpoint  = endpoint,
                        Pagina       = page,
                        Respuesta    = "",
                        FechaHoraIns = DateTime.Now,
                        IpIns        = "localhost",
                        UserIns      = "SincronizarAsesoresAgricolas"
                    };
                    context.CRM_Solicitud.InsertOnSubmit(solicitud);
                    context.SubmitChanges();

                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(endpoint);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Accept = "*/*";
                    httpWebRequest.Headers.Add("X-Session-Key", token);
                    httpWebRequest.Method = "GET";

                    HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        solicitud.Respuesta = result;
                        context.SubmitChanges();

                        List<JsonResultUsuario> jsonResults = JsonConvert.DeserializeObject<List<JsonResultUsuario>>(result);
                        if (jsonResults.Count < 50)
                            condition = true;

                        foreach (JsonResultUsuario jsonResult in jsonResults)
                        {
                            CRM_AsesorAgricola asesorAgricola = context.CRM_AsesorAgricola.SingleOrDefault(u => u.ID == jsonResult.id);
                            if (asesorAgricola == null)
                            {
                                asesorAgricola = new CRM_AsesorAgricola()
                                {
                                    ID           = jsonResult.id,
                                    Nombre       = jsonResult.name,
                                    Apellido     = jsonResult.lastName,
                                    Telefono     = jsonResult.phone,
                                    Email        = jsonResult.email,
                                    FechaHoraIns = jsonResult.dateCreated,
                                    FechaHoraUpd = jsonResult.dateUpdated,
                                    FechaHoraDel = jsonResult.dateDeleted
                                };

                                context.CRM_AsesorAgricola.InsertOnSubmit(asesorAgricola);
                            }
                            else
                            {
                                asesorAgricola.Nombre       = jsonResult.name;
                                asesorAgricola.Apellido     = jsonResult.lastName;
                                asesorAgricola.Telefono     = jsonResult.phone;
                                asesorAgricola.Email        = jsonResult.email;
                                asesorAgricola.FechaHoraUpd = jsonResult.dateUpdated;
                                asesorAgricola.FechaHoraDel = jsonResult.dateDeleted;
                            }

                            context.SubmitChanges();
                        }
                    }

                    httpWebResponse.Close();

                    page++;
                } while (!condition);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public ResponseCierreCreado SetCierreCreado(RequestCierreCreado request)
        {
            ResponseCierreCreado response = new ResponseCierreCreado();
            if (request != null)
            {
                try
                {
                    Proxy proxy = new Proxy();
                    string token = proxy.Login();

                    string endpoint = string.Format("https://api.forcemanager.net/api/v4/activities/{0}", request.IdSolicitudContrato);

                    CRM_Solicitud solicitud = new CRM_Solicitud()
                    {
                        Tipo         = string.Format("Set Cierre Creado - Solicitud {0}", request.IdSolicitudContrato),
                        ApiEndpoint  = endpoint,
                        Pagina       = 0,
                        Respuesta    = "",
                        FechaHoraIns = DateTime.Now,
                        IpIns        = "localhost",
                        UserIns      = "SetCierreCreado"
                    };
                    context.CRM_Solicitud.InsertOnSubmit(solicitud);
                    context.SubmitChanges();

                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(endpoint);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Accept = "*/*";
                    httpWebRequest.Headers.Add("X-Session-Key", token);
                    httpWebRequest.Method = "PUT";

                    JsonRequestCierreCreado jsonRequest = new JsonRequestCierreCreado()
                    {
                        Z_Cierre_Creado = request.CierreCreado
                    };

                    string putData = JsonConvert.SerializeObject(jsonRequest);
                    var bytes = Encoding.ASCII.GetBytes(putData);
                    httpWebRequest.ContentLength = bytes.Length;
                    using (var stream = httpWebRequest.GetRequestStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }

                    HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        solicitud.Respuesta = result;
                        context.SubmitChanges();

                        JsonResultCierreCreado jsonResult = JsonConvert.DeserializeObject<JsonResultCierreCreado>(result);

                        if (jsonResult.Message == "entity updated")
                        {
                            response.OK = true;
                            response.Mensaje = string.Format("OK - {0}", jsonResult.Message);
                        }
                        else
                        {
                            response.OK = false;
                            response.Mensaje = jsonResult.Message;
                        }
                    }

                    httpWebResponse.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            else
            {
                response.OK = false;
                response.Mensaje = "";
            }

            return response;
        }

        public ResponseContratoCreado SetContratoCreado(RequestContratoCreado request)
        {
            ResponseContratoCreado response = new ResponseContratoCreado();
            if (request != null)
            {
                try
                {
                    Proxy proxy = new Proxy();
                    string token = proxy.Login();

                    string endpoint = string.Format("https://api.forcemanager.net/api/v4/activities/{0}", request.IdSolicitudContrato);

                    CRM_Solicitud solicitud = new CRM_Solicitud()
                    {
                        Tipo         = string.Format("Set Contrato Creado - Solicitud {0}", request.IdSolicitudContrato),
                        ApiEndpoint  = endpoint,
                        Pagina       = 0,
                        Respuesta    = "",
                        FechaHoraIns = DateTime.Now,
                        IpIns        = "localhost",
                        UserIns      = "SetContratoCreado"
                    };
                    context.CRM_Solicitud.InsertOnSubmit(solicitud);
                    context.SubmitChanges();

                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(endpoint);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Accept = "*/*";
                    httpWebRequest.Headers.Add("X-Session-Key", token);
                    httpWebRequest.Method = "PUT";

                    JsonRequestContratoCreado jsonRequest = new JsonRequestContratoCreado()
                    {
                        Z_Contrato_Creado = request.ContratoCreado
                    };

                    string putData = JsonConvert.SerializeObject(jsonRequest);
                    var bytes = Encoding.ASCII.GetBytes(putData);
                    httpWebRequest.ContentLength = bytes.Length;
                    using (var stream = httpWebRequest.GetRequestStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }

                    HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        solicitud.Respuesta = result;
                        context.SubmitChanges();

                        JsonResultContratoCreado jsonResult = JsonConvert.DeserializeObject<JsonResultContratoCreado>(result);

                        if (jsonResult.Message == "entity updated")
                        {
                            response.OK = true;
                            response.Mensaje = string.Format("OK - {0}", jsonResult.Message);
                        }
                        else
                        {
                            response.OK = false;
                            response.Mensaje = jsonResult.Message;
                        }
                    }

                    httpWebResponse.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            else
            {
                response.OK = false;
                response.Mensaje = "";
            }

            return response;
        }

        public ResponseVerificado SetVerificado(RequestVerificado request)
        {
            ResponseVerificado response = new ResponseVerificado();
            if (request != null)
            {
                try
                {
                    Proxy proxy = new Proxy();
                    string token = proxy.Login();

                    string endpoint = string.Format("https://api.forcemanager.net/api/v4/activities/{0}", request.IdSolicitudContrato);

                    CRM_Solicitud solicitud = new CRM_Solicitud()
                    {
                        Tipo         = string.Format("Set {1} - Solicitud {0}", request.IdSolicitudContrato, request.Nombre),
                        ApiEndpoint  = endpoint,
                        Pagina       = 0,
                        Respuesta    = "",
                        FechaHoraIns = DateTime.Now,
                        IpIns        = "localhost",
                        UserIns      = "SetVerificado"
                    };
                    context.CRM_Solicitud.InsertOnSubmit(solicitud);
                    context.SubmitChanges();

                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(endpoint);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Accept = "*/*";
                    httpWebRequest.Headers.Add("X-Session-Key", token);
                    httpWebRequest.Method = "PUT";

                    JsonRequestVerificado jsonRequest = new JsonRequestVerificado()
                    {
                        Z_idVerificado = request.IdEstado
                    };

                    string putData = JsonConvert.SerializeObject(jsonRequest);
                    var bytes = Encoding.ASCII.GetBytes(putData);
                    httpWebRequest.ContentLength = bytes.Length;
                    using (var stream = httpWebRequest.GetRequestStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }

                    HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        solicitud.Respuesta = result;
                        context.SubmitChanges();

                        JsonResultVerificado jsonResult = JsonConvert.DeserializeObject<JsonResultVerificado>(result);

                        if (jsonResult.Message == "entity updated")
                        {
                            response.OK = true;
                            response.Mensaje = string.Format("OK - {0}", jsonResult.Message);
                        }
                        else
                        {
                            response.OK = false;
                            response.Mensaje = jsonResult.Message;
                        }
                    }

                    httpWebResponse.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            else
            {
                response.OK = false;
                response.Mensaje = "";
            }

            return response;
        }

        public ResponseCreateDocumentResource SubirContratosYAnexos(RequestCreateDocumentResource request)
        {
            ResponseCreateDocumentResource response = new ResponseCreateDocumentResource();
            if (request != null)
            {
                try
                {
                    Proxy proxy = new Proxy();
                    string token = proxy.Login();

                    SolicitudContrato solicitudContrato = context.SolicitudContrato.Single(sc => sc.IdSolicitudContrato == request.IdSolicitudContrato);
                    Contrato contrato                   = solicitudContrato.Contrato;
                    List<DoctoContrato> doctoContratos  = context.DoctoContrato.Where(dc => dc.IdContrato == solicitudContrato.IdContrato && dc.DoctoValido).ToList();

                    if (!solicitudContrato.Agricultor.IdForceManager.HasValue)
                    {
                        return new ResponseCreateDocumentResource()
                        {
                            OK = false,
                            Mensaje = "Este Agricultor no esta vinculado a ForceManager"
                        };
                    }

                    string endpoint = string.Format("https://api.forcemanager.net/api/v4/accounts/{0}/documents", solicitudContrato.Agricultor.IdForceManager);

                    foreach (DoctoContrato doctoContrato in doctoContratos)
                    {
                        CRM_Solicitud solicitud = new CRM_Solicitud()
                        {
                            Tipo         = string.Format("Subir Contratos y Anexos - Solicitud {0} - {1} Nº {2}", request.IdSolicitudContrato, doctoContrato.TipoDoctoContrato.Descripcion, doctoContrato.Correlativo),
                            ApiEndpoint  = endpoint,
                            Pagina       = 0,
                            Respuesta    = "",
                            FechaHoraIns = DateTime.Now,
                            IpIns        = "localhost",
                            UserIns      = "SubirContratosYAnexos"
                        };
                        context.CRM_Solicitud.InsertOnSubmit(solicitud);
                        context.SubmitChanges();

                        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(endpoint);
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Accept = "*/*";
                        httpWebRequest.Headers.Add("X-Session-Key", token);
                        httpWebRequest.Method = "POST";

                        JsonRequestCreateDocumentResource jsonRequest = new JsonRequestCreateDocumentResource()
                        {
                            folderId = -1,
                            name     = "",
                            level    = 2,
                        };

                        if (doctoContrato.IdTipoDoctoContrato == 1)
                        {
                            jsonRequest.name = string.Format("{0}.pdf", doctoContrato.TipoDoctoContrato.Descripcion);
                        }
                        else
                        {
                            jsonRequest.name = string.Format("{0} Nro {1}.pdf", doctoContrato.TipoDoctoContrato.Descripcion, doctoContrato.Correlativo);
                        }

                        string postData = JsonConvert.SerializeObject(jsonRequest);
                        byte[] bytes = Encoding.ASCII.GetBytes(postData);
                        httpWebRequest.ContentLength = bytes.Length;
                        using (Stream stream = httpWebRequest.GetRequestStream())
                        {
                            stream.Write(bytes, 0, bytes.Length);
                        }

                        HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                        using (var streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();
                            solicitud.Respuesta = result;
                            context.SubmitChanges();

                            JsonResultCreateDocumentResource jsonResult = JsonConvert.DeserializeObject<JsonResultCreateDocumentResource>(result);
                            if (jsonResult.locator != "")
                            {
                                UploadToAmazon(jsonResult.locator, doctoContrato);
                            }
                        }

                        httpWebResponse.Close();
                    }

                    // Yayyy
                    response.OK = true;
                    response.Mensaje = "";
                }
                catch (Exception ex)
                {
                    response.OK = false;
                    response.Mensaje = "";

                    Console.WriteLine(ex.ToString());
                }
            }
            else
            {
                response.OK = false;
                response.Mensaje = "";
            }

            return response;
        }

        private int? GetCultivo(Z_idCultivo z_IdCultivo)
        {
            int? id = null;
            if (z_IdCultivo != null)
            {
                Cultivo cultivo = context.Cultivo.SingleOrDefault(c => c.IdForceManagerCRM == z_IdCultivo.id);
                if (cultivo != null)
                {
                    id = cultivo.IdCultivo;
                }
            }
            return id;
        }

        private int? GetTipoContrato(Z_idTipoContrato z_IdTipoContrato)
        {
            int? id = null;
            if (z_IdTipoContrato != null)
            {
                TipoContrato tipoContrato = context.TipoContrato.SingleOrDefault(tc => tc.IdForceManagerCRM == z_IdTipoContrato.id);
                if (tipoContrato != null)
                {
                    id = tipoContrato.IdTipoContrato;
                }
            }
            return id;
        }

        private int? GetComunaOrigen(Z_idComunaOrigen z_IdComunaOrigen)
        {
            int? id = null;
            if (z_IdComunaOrigen != null)
            {
                Comuna comuna = context.Comuna.SingleOrDefault(c => c.IdForceManagerCRM == z_IdComunaOrigen.id);
                if (comuna != null)
                {
                    id = comuna.IdComuna;
                }
            }
            return id;
        }

        private int? GetSucursalEntrega(Z_idSucursalEntrega z_IdSucursalEntrega)
        {
            int? id = null;
            if (z_IdSucursalEntrega != null)
            {
                Sucursal sucursal = context.Sucursal.SingleOrDefault(s => s.IdForceManagerCRM == z_IdSucursalEntrega.id);
                if (sucursal != null)
                {
                    id = sucursal.IdSucursal;
                }
            }
            return id;
        }

        private int? GetTemporada(Z_Temporadas z_Temporadas)
        {
            int? id = null;
            if (z_Temporadas != null)
            {
                Temporada temporada = context.Temporada.SingleOrDefault(t => t.IdForceManagerCRM == z_Temporadas.id);
                if (temporada != null)
                {
                    id = temporada.IdTemporada;
                }
            }
            return id;
        }

        private int? GetVariedad(Z_idVariedadSolicitudContrato z_IdVariedadSolicitudContrato)
        {
            int? id = null;
            if (z_IdVariedadSolicitudContrato != null)
            {
                CultivoContrato cultivoContrato = context.CultivoContrato.SingleOrDefault(cc => cc.IdForceManagerCRM == z_IdVariedadSolicitudContrato.id);
                if (cultivoContrato != null)
                {
                    id = cultivoContrato.IdCultivoContrato;
                }
            }
            return id;
        }

        private string SanitizacionArchivo(string rutaArchivo)
        {
            return string.Format(@"{0}\{1}", Properties.Settings.Default.AgroFichasWebRoot, rutaArchivo.Replace("~/", "").Replace("/", @"\"));
        }

        private bool UploadToAmazon(string locator, DoctoContrato doctoContrato)
        {
            bool OK = true;

            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(locator);
                httpWebRequest.ContentType = "application/octet-stream";
                httpWebRequest.Accept = "*/*";
                httpWebRequest.Method = "PUT";

                FileStream fileStream = new FileStream(SanitizacionArchivo(doctoContrato.RutaArchivo), FileMode.Open);
                MemoryStream memoryStream = new MemoryStream();
                fileStream.CopyTo(memoryStream);

                byte[] bytes = memoryStream.ToArray();
                httpWebRequest.ContentLength = bytes.Length;
                using (Stream stream = httpWebRequest.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }

                CRM_Solicitud solicitud = new CRM_Solicitud()
                {
                    Tipo = "Upload To Amazon",
                    ApiEndpoint = locator,
                    Pagina = 0,
                    Respuesta = "",
                    FechaHoraIns = DateTime.Now,
                    IpIns = "localhost",
                    UserIns = "UploadToAmazon"
                };

                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    OK = true;
                    solicitud.Respuesta = "OK";
                }
                else
                {
                    OK = false;
                    solicitud.Respuesta = "Error";
                }

                context.CRM_Solicitud.InsertOnSubmit(solicitud);
                context.SubmitChanges();

                httpWebResponse.Close();
                memoryStream.Close();
                fileStream.Close();
            }
            catch
            {
                OK = false;
            }

            return OK;
        }
    }
}
