using MoreLinq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CarteraAsesorAgricola
{
    class Program
    {
        private static AgroFichasDBDataContext dataContext = new AgroFichasDBDataContext();

        static void Main(string[] args)
        {
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine(".oPYo.                                                         .oo                       o                ");
            Console.WriteLine("8.                                                            .P 8                       8                ");
            Console.WriteLine("`boo   ooYoYo. .oPYo. oPYo. .oPYo. .oPYo. .oPYo. .oPYo.      .P  8 .oPYo. oPYo. .oPYo.  o8P .oPYo. .oPYo. ");
            Console.WriteLine(".P     8' 8  8 8    8 8  `' 8oooo8 Yb..   .oooo8 Yb..       oPooo8 8    8 8  `' 8    8   8  8    8 8    8 ");
            Console.WriteLine("8      8  8  8 8    8 8     8.       'Yb. 8    8   'Yb.    .P    8 8    8 8     8    8   8  8    8 8    8 ");
            Console.WriteLine("`YooP' 8  8  8 8YooP' 8     `Yooo' `YooP' `YooP8 `YooP'   .P     8 `YooP8 8     `YooP'   8  `YooP' 8YooP' ");
            Console.WriteLine(":.....:..:..:..8 ....:..:::::.....::.....::.....::.....:::..:::::..:....8 ..:::::.....:::..::.....:8 ....:");
            Console.WriteLine(":::::::::::::::8 ::::::::::::::::::::::::::::::::::::::::::::::::::::ooP'.:::::::::::::::::::::::::8 :::::");
            Console.WriteLine(":::::::::::::::..::::::::::::::::::::::::::::::::::::::::::::::::::::...:::::::::::::::::::::::::::..:::::");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Cartera Asesor Agrícola");
            Console.WriteLine("-----------------------");
            Console.WriteLine("Sincronizador");
            Console.WriteLine("v1.0");

            using (var reader = new StreamReader(string.Format(@"{0}\Agricultores.csv", Properties.Settings.Default.App_Data)))
            {
                List<CSVLine> agricultores = new List<CSVLine>();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    agricultores.Add(new CSVLine()
                    {
                        IdAgricultor = 0,
                        Rut = values[1],
                        Nombre = values[2],
                        Email = values[3],
                        Fono1 = values[4],
                        Fono2 = values[5],
                        Asesor = values[6],
                    });
                }

                var agricultores_sin_asesor = agricultores.RemoveAll(X => X.Asesor == String.Empty);

                Console.WriteLine("{0} NO tienen asesor asignado", agricultores_sin_asesor);
                Console.WriteLine("--");
                Console.WriteLine("");

                var asesores = agricultores.DistinctBy(X => X.Asesor).Select(Y => Y.Asesor).OrderBy(Z => Z);
                foreach (var asesor in asesores)
                {
                    Console.WriteLine(asesor);

                    List<SYS_User> users = dataContext.SYS_User.Where(X => X.FullName == asesor).ToList();
                    if (users.Count == 0)
                    {
                        Console.WriteLine("--> Asesor '{0}' no existe", asesor);
                    }
                    else if (users.Count > 1)
                    {
                        foreach (SYS_User user in users)
                        {
                            Console.WriteLine("--> {0} {1} '{2}' duplicado", user.UserID, user.FullName, user.UserName);
                        }
                    }
                    else if (users.Count == 1)
                    {
                        var user = users.Single();
                        Console.WriteLine("--> {0} {1} '{2}'", user.UserID, user.FullName, user.UserName);
                    }
                }

                foreach (var agricultor in agricultores)
                {
                    Agricultor agricultor1 = dataContext.Agricultor.SingleOrDefault(X => X.Rut == agricultor.Rut);
                    if (agricultor1 == null)
                    {
                        Console.WriteLine("Agricultor {0} {1} no existe", agricultor.Rut, agricultor.Nombre);
                    }
                }

                int ingresos = 0;

                foreach (var agricultor in agricultores)
                {
                    Agricultor agricultor1 = dataContext.Agricultor.SingleOrDefault(X => X.Rut == agricultor.Rut);
                    if (agricultor1 != null)
                    {
                        List<SYS_User> users = dataContext.SYS_User.Where(X => X.FullName == agricultor.Asesor).ToList();
                        if (users.Count == 0)
                        {
                            continue;
                        }
                        else if (users.Count > 1)
                        {
                            foreach (SYS_User user in users)
                            {
                                UsuarioAgricultor usuarioAgricultor = dataContext.UsuarioAgricultor.SingleOrDefault(X => X.IdAgricultor == agricultor1.IdAgricultor && X.UserID == user.UserID);
                                if (usuarioAgricultor != null) continue;

                                usuarioAgricultor = new UsuarioAgricultor()
                                {
                                    UserID = user.UserID,
                                    IdAgricultor = agricultor1.IdAgricultor,
                                    MobileTag = "",
                                    UserIns = "CarteraAsesorAgricola",
                                    FechaHoraIns = DateTime.Now,
                                    IpIns = "Localhost"
                                };
                                dataContext.UsuarioAgricultor.InsertOnSubmit(usuarioAgricultor);
                                dataContext.SubmitChanges();
                                ingresos++;
                            }
                        }
                        else if (users.Count == 1)
                        {
                            var user = users.Single();
                            UsuarioAgricultor usuarioAgricultor = dataContext.UsuarioAgricultor.SingleOrDefault(X => X.IdAgricultor == agricultor1.IdAgricultor && X.UserID == user.UserID);
                            if (usuarioAgricultor != null) continue;

                            usuarioAgricultor = new UsuarioAgricultor()
                            {
                                UserID = user.UserID,
                                IdAgricultor = agricultor1.IdAgricultor,
                                MobileTag = "",
                                UserIns = "CarteraAsesorAgricola",
                                FechaHoraIns = DateTime.Now,
                                IpIns = "Localhost"
                            };
                            dataContext.UsuarioAgricultor.InsertOnSubmit(usuarioAgricultor);
                            dataContext.SubmitChanges();
                            ingresos++;
                        }
                    }
                }

                Console.WriteLine("Total de ingresos -{0}-", ingresos);

                Console.ReadLine();
            }
        }
    }

    class CSVLine
    {
        public int IdAgricultor { get; set; }

        public string Rut { get; set; }

        public string Nombre { get; set; }

        public string Email { get; set; }

        public string Fono1 { get; set; }

        public string Fono2 { get; set; }

        public string Asesor { get; set; }
    }
}
