using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ForceManagerLib
{
    public class Proxy
    {
        private static AgroFichasDBDataContext dataContext = new AgroFichasDBDataContext();

        public List<Account> GetAllAccounts()
        {
            string token = Login();
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    int page = 0;
                    bool we_reach_the_end = false;
                    string request_key = string.Format("getallaccounts_request_key_{0}", System.Guid.NewGuid());
                    List<Account> accountsList = new List<Account>();

                    do
                    {
                        StringBuilder IDS = new StringBuilder();
                        int count = dataContext.Agricultor.Where(X => X.IdForceManager.HasValue).ToList().Count;
                        int I = 1;
                        foreach (Agricultor agricultor in dataContext.Agricultor.Where(X => X.IdForceManager.HasValue).ToList())
                        {
                            if (I == count)
                                IDS.Append(agricultor.IdForceManager);
                            else
                                IDS.Append(string.Format("{0},", agricultor.IdForceManager));
                            I++;
                        }

                        string endpoint = "";
                        if (IDS.Length == 0)
                            endpoint = string.Format("https://api.forcemanager.net/api/v4/accounts?page={0}&where=typeId.id=36 AND vatNumber<>''", page);
                        else
                            endpoint = string.Format("https://api.forcemanager.net/api/v4/accounts?page={0}&where=typeId.id=36 AND vatNumber<>'' AND id NOT IN ({1})", page, IDS.ToString());

                        Console.WriteLine("LISTED URL");
                        Console.WriteLine(@"Endpoint:\N{0}", endpoint);

                        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(endpoint);
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Accept = "*/*";
                        httpWebRequest.Headers.Add("X-Session-Key", token);
                        httpWebRequest.Method = "GET";

                        HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                        using (var streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();
                            System.IO.File.WriteAllText(string.Format(@"{2}\logs\getallaccounts_{0}_page_{1}.txt", request_key, (page + 1), Properties.Settings.Default.ForceManagerData), result);
                            List<Account> accounts = JsonConvert.DeserializeObject<List<Account>>(result);
                            if (accounts.Count == 0)
                                we_reach_the_end = true;

                            accountsList = accountsList.Concat(accounts).ToList();
                        }

                        httpWebResponse.Close();

                        page++;
                    } while (!we_reach_the_end);

                    return accountsList;
                }
                catch (Exception ex)
                {
                    string getallaccounts_error_key = string.Format("getallaccounts_error_key_{0}", System.Guid.NewGuid());
                    System.IO.File.WriteAllText(string.Format(@"{1}\error\{0}.txt", getallaccounts_error_key, Properties.Settings.Default.ForceManagerData), ex.ToString());
                }
            }

            return new List<Account>();
        }

        public List<Activity> GetAllActivities(int id)
        {
            string token = Login();
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    int page = 0;
                    bool we_reach_the_end = false;
                    string request_key = string.Format("getallactivities_{0}_request_key_{1}", id, System.Guid.NewGuid());
                    List<Activity> activitiesList = new List<Activity>();

                    do
                    {
                        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(string.Format("https://api.forcemanager.net/api/v4/activities?page={0}&where=accountId.id={1}", page, id));
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Accept = "*/*";
                        httpWebRequest.Headers.Add("X-Session-Key", token);
                        httpWebRequest.Method = "GET";

                        HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                        using (var streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();
                            System.IO.File.WriteAllText(string.Format(@"{2}\logs\{0}_page_{1}.txt", request_key, (page + 1), Properties.Settings.Default.ForceManagerData), result);
                            List<Activity> activities = JsonConvert.DeserializeObject<List<Activity>>(result);
                            if (activities.Count == 0)
                                we_reach_the_end = true;

                            activitiesList = activitiesList.Concat(activities).ToList();
                        }

                        httpWebResponse.Close();

                        page++;
                    } while (!we_reach_the_end);

                    return activitiesList;
                }
                catch (Exception ex)
                {
                    string getallactivities_error_key = string.Format("getallactivities_{0}_error_key_{1}", id, System.Guid.NewGuid());
                    System.IO.File.WriteAllText(string.Format(@"{1}\error\{0}.txt", getallactivities_error_key, Properties.Settings.Default.ForceManagerData), ex.ToString());
                }
            }

            return new List<Activity>();
        }

        public Activity GetASpecificActivity(int id)
        {
            string token = Login();
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    string request_key = string.Format("getaspecificactivity_{0}_request_key_{1}", id, System.Guid.NewGuid());
                    List<Activity> allActivities = new List<Activity>();

                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(string.Format("https://api.forcemanager.net/api/v4/activities/{0}", id));
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Accept = "*/*";
                    httpWebRequest.Headers.Add("X-Session-Key", token);
                    httpWebRequest.Method = "GET";

                    HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        System.IO.File.WriteAllText(string.Format(@"{1}\logs\{0}.txt", request_key, Properties.Settings.Default.ForceManagerData), result);
                        httpWebResponse.Close();
                        return JsonConvert.DeserializeObject<Activity>(result);
                    }
                }
                catch (Exception ex)
                {
                    string getaspecificactivity_error_key = string.Format("getaspecificactivity_{0}_error_key_{1}", id, System.Guid.NewGuid());
                    System.IO.File.WriteAllText(string.Format(@"{1}\error\{0}.txt", getaspecificactivity_error_key, Properties.Settings.Default.ForceManagerData), ex.ToString());
                }
            }

            return null;
        }

        public string Login()
        {
            try
            {
                Passport passport = new Passport()
                {
                    username = Properties.Settings.Default.username,
                    password = Properties.Settings.Default.password
                };

                Authentication auth = new Authentication();

                ForceManager forceManager = dataContext.ForceManager.SingleOrDefault(X => X.Fecha.Date == DateTime.Now.Date);
                if (forceManager != null)
                {
                    auth.token = forceManager.Token;
                }
                else
                {
                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.forcemanager.net/api/v4/login");
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Accept = "*/*";
                    httpWebRequest.Method = "POST";

                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        streamWriter.Write(JsonConvert.SerializeObject(passport));
                        streamWriter.Flush();
                        streamWriter.Close();
                    }

                    HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                    {
                        string result = streamReader.ReadToEnd();
                        auth = JsonConvert.DeserializeObject<Authentication>(result);
                    }

                    httpWebResponse.Close();

                    forceManager = new ForceManager()
                    {
                        Fecha = DateTime.Now.Date,
                        Token = auth.token,
                        FechaHoraIns = DateTime.Now,
                        IpIns = "Localhost",
                        UserIns = "forcemanagerlib"
                    };
                    dataContext.ForceManager.InsertOnSubmit(forceManager);
                    dataContext.SubmitChanges();
                }

                return auth.token;
            }
            catch (Exception ex)
            {
                string login_error_key = string.Format("login_error_key_{0}", System.Guid.NewGuid());
                System.IO.File.WriteAllText(string.Format(@"{1}\error\{0}.txt", login_error_key, Properties.Settings.Default.ForceManagerData), ex.ToString());
            }

            return string.Empty;
        }

        public List<Account> SyncAccounts()
        {
            string token = Login();
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    int page = 0;
                    bool we_reach_the_end = false;
                    string request_key = string.Format("getallaccounts_request_key_{0}", System.Guid.NewGuid());
                    List<Account> accountsList = new List<Account>();

                    do
                    {
                        string endpoint = string.Format("https://api.forcemanager.net/api/v4/accounts?page={0}&&where=typeId.id=36", page);

                        Console.WriteLine("Endpoint {0}", endpoint);

                        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(endpoint);
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Accept = "*/*";
                        httpWebRequest.Headers.Add("X-Session-Key", token);
                        httpWebRequest.Method = "GET";

                        HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                        using (var streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();
                            System.IO.File.WriteAllText(string.Format(@"{2}\logs\getallaccounts_{0}_page_{1}.txt", request_key, (page + 1), Properties.Settings.Default.ForceManagerData), result);
                            List<Account> accounts = JsonConvert.DeserializeObject<List<Account>>(result);
                            if (accounts.Count == 0)
                                we_reach_the_end = true;

                            accountsList = accountsList.Concat(accounts).ToList();
                        }

                        httpWebResponse.Close();

                        page++;
                    } while (!we_reach_the_end);

                    return accountsList;
                }
                catch (Exception ex)
                {
                    string getallaccounts_error_key = string.Format("getallaccounts_error_key_{0}", System.Guid.NewGuid());
                    System.IO.File.WriteAllText(string.Format(@"{1}\error\{0}.txt", getallaccounts_error_key, Properties.Settings.Default.ForceManagerData), ex.ToString());
                }
            }

            return new List<Account>();
        }
    }
}