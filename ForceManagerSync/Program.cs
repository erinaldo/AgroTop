using ForceManagerLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ForceManagerSync
{
    class Program
    {
        private static AgroFichasDBDataContext dataContext = new AgroFichasDBDataContext();

        static void Main(string[] args)
        {
            Console.WriteLine("" +
                @"
 _______   ______   .______        ______  _______ .___  ___.      ___      .__   __.      ___       _______  _______ .______             ______ .______      .___  ___. 
|   ____| /  __  \  |   _  \      /      ||   ____||   \/   |     /   \     |  \ |  |     /   \     /  _____||   ____||   _  \     _     /      ||   _  \     |   \/   | 
|  |__   |  |  |  | |  |_)  |    |  ,----'|  |__   |  \  /  |    /  ^  \    |   \|  |    /  ^  \   |  |  __  |  |__   |  |_)  |   (_)   |  ,----'|  |_)  |    |  \  /  | 
|   __|  |  |  |  | |      /     |  |     |   __|  |  |\/|  |   /  /_\  \   |  . `  |   /  /_\  \  |  | |_ | |   __|  |      /          |  |     |      /     |  |\/|  | 
|  |     |  `--'  | |  |\  \----.|  `----.|  |____ |  |  |  |  /  _____  \  |  |\   |  /  _____  \ |  |__| | |  |____ |  |\  \----._    |  `----.|  |\  \----.|  |  |  | 
|__|      \______/  | _| `._____| \______||_______||__|  |__| /__/     \__\ |__| \__| /__/     \__\ \______| |_______|| _| `._____(_)    \______|| _| `._____||__|  |__| " +
                "" +
                "");

            Console.WriteLine("-- ACCOUNTS SYNCHRONIZATION --");

            Console.WriteLine("VER. 1.1.alpha");

            Proxy FMProxy = new Proxy();
            string token = FMProxy.Login();
            if (!string.IsNullOrEmpty(token))
            {
                TrazaTop trazaTop = new TrazaTop();
                trazaTop.SincronizarAsesoresAgricolas();

                List<Account> accounts = FMProxy.GetAllAccounts();
                List<Account> accounts_no_asociados = new List<Account>();

                List<Agricultor> agricultores = (from X in dataContext.Agricultor
                                                 where X.Rut != string.Empty
                                                 && X.Habilitado == true
                                                 select X).ToList();

                foreach (Account account in accounts)
                {
                    Agricultor agricultor = agricultores.FirstOrDefault(X => X.Rut.Replace(".", "").Replace(",", "").ToLower() == account.vatNumber.Replace(".", "").Replace(",", "").ToLower());
                    if (agricultor != null)
                    {
                        agricultor.IdForceManager = account.id;
                        dataContext.SubmitChanges();
                    }
                    else
                    {
                        accounts_no_asociados.Add(account);
                    }
                }

                Reporte reporte = new Reporte();
                reporte.CreatePdf(accounts_no_asociados);

                accounts = FMProxy.SyncAccounts();
                Sync.dosync(accounts);

                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("Error en el inicio de sesión");
                Console.ReadLine();
            }
        }
    }
}
