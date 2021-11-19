using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Web;

namespace AgroFichasApi.Models
{
    public class FullFileSync
    {
        public string FileName;

        private SQLiteCommand InsertCommand<T>(T item) where T : ISynSerializable
        {
            var names = String.Join(",", item.SyncProperties);
            var values = String.Join(",", (from p in item.SyncProperties select "@" + p).ToArray());

            var s = "INSERT INTO {0} ({1}, LocalID, IsDirty) values ({2}, null, 0)";

            var cmd = new SQLiteCommand(String.Format(s, typeof(T).Name, names, values));

            cmd.Parameters.AddRange((from p in item.SyncProperties
                                     select new SQLiteParameter("@" + p, item.GetType().GetProperty(p).GetValue(item))).ToArray());

            return cmd;
        }

        private SQLiteCommand InsertCommandWithAlias<T>(T item) where T : ISynSerializableWithAlias
        {
            var names = String.Join(",", item.SyncProperties.Keys);
            var values = String.Join(",", (from p in item.SyncProperties select "@" + p.Key).ToArray());

            var s = "INSERT INTO {0} ({1}, LocalID, IsDirty) values ({2}, null, 0)";

            var cmd = new SQLiteCommand(String.Format(s, typeof(T).Name, names, values));


            cmd.Parameters.AddRange((from pair in item.SyncProperties
                                     select new SQLiteParameter("@" + pair.Key, item.GetType().GetProperty(pair.Value).GetValue(item))).ToArray());

            return cmd;
        }

        public void FillResponse(AgroFichasDBDataContext dc, SYS_User user)
        {
            FileName = Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data/sqlite/syncs"), DateTime.Now.ToString("yyyyMMdd_hhmmss_") + user.UserName + ".db3");
            string TempFile = FileName + ".temp";

            using (var con = new SQLiteConnection("Data Source=" + TempFile + ";Version=3;"))
            {
                con.Open();

                var ddl = File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/sqlite/DDL_AgroFichasDB.sql"));
                using (var ddlCmd = new SQLiteCommand(ddl, con))
                {
                    ddlCmd.ExecuteNonQuery();
                }

                var cmds = new List<SQLiteCommand>();

                //Proveedores
                cmds.AddRange((from pr in dc.Proveedor 
                               orderby pr.Nombre 
                               select InsertCommand<Proveedor>(pr)).ToList());

                //IndexAgricultores
                cmds.AddRange((from ag in dc.Agricultor
                               select InsertCommand<IndexAgricultor>(new IndexAgricultor { Rut = ag.Rut, ID = ag.ID })).ToList());

                //Agricultores
                if (user.SYS_PermisoUsuario.SingleOrDefault(p => p.IdPermiso == 5) != null) //Puede verlos a todos
                {
                    cmds.AddRange((from ag in dc.Agricultor
                                   where ag.Habilitado
                                   orderby ag.Nombre
                                   select InsertCommand<Agricultor>(ag)).ToList());
                }
                else //Sólo los con acceso
                {
                    cmds.AddRange((from ag in dc.Agricultor 
                                   join us in dc.UsuarioAgricultor on ag.IdAgricultor equals us.IdAgricultor
                                   where ag.Habilitado
                                      && us.UserID == user.UserID
                                   orderby ag.Nombre
                                   select InsertCommand<Agricultor>(ag)).ToList());
                }

                //Predios
                cmds.AddRange((from ag in dc.Agricultor
                                join pr in dc.Predio on ag.IdAgricultor equals pr.IdAgricultor
                                where ag.Habilitado
                                    && pr.Habilitado
                                select InsertCommand<Predio>(pr)).ToList());

                //Potreros
                cmds.AddRange((from ag in dc.Agricultor
                         join pr in dc.Predio on ag.IdAgricultor equals pr.IdAgricultor
                         join po in dc.Potrero on pr.IdPredio equals po.IdPredio
                         where ag.Habilitado
                            && pr.Habilitado
                               select InsertCommand<Potrero>(po)).ToList());
                
                //Comunas
                cmds.AddRange((from re in dc.Region
                               join pr in dc.Provincia on re.IdRegion equals pr.IdRegion
                               join co in dc.Comuna on pr.IdProvincia equals co.IdProvincia
                               where re.DoSync
                               orderby co.Orden
                               select InsertCommand<Comuna>(co)).ToList());

                //Temporadas
                var temporadaActiva = dc.Temporada.Where(t => t.ActivaFichas).First();
                cmds.Add(InsertCommandWithAlias<Temporada>(temporadaActiva));

                //Cultivos
                cmds.AddRange((from cu in dc.Cultivo select InsertCommand<Cultivo>(cu)).ToList());

                //Variedades
                cmds.AddRange((from va in dc.Variedad select InsertCommand<Variedad>(va)).ToList());

                //Tipos de siembra
                cmds.AddRange((from ts in dc.TipoSiembra select InsertCommand<TipoSiembra>(ts)).ToList());

                //Estados de Siembra
                cmds.AddRange((from es in dc.EstadoSiembra select InsertCommand<EstadoSiembra>(es)).ToList());

                //Importancia de Seguimiento
                cmds.AddRange((from iss in dc.ImportanciaSeguimiento select InsertCommand<ImportanciaSeguimiento>(iss)).ToList());

                //Siembras
                cmds.AddRange((from s in dc.Siembra where s.IdTemporada == temporadaActiva.IdTemporada select InsertCommand<Siembra>(s)).ToList());

                //SiembrasPotrero
                cmds.AddRange((from sp in dc.SiembraPotrero where sp.IdTemporada == temporadaActiva.IdTemporada select InsertCommand<SiembraPotrero>(sp)).ToList());

                //TiposRecomendacion
                cmds.AddRange((from tr in dc.TipoRecomendacion select InsertCommand<TipoRecomendacion>(tr)).ToList());

                //UM
                cmds.AddRange((from um in dc.UM select InsertCommand<UM>(um)).ToList());

                //Quimico
                cmds.AddRange((from qu in dc.Quimico select InsertCommand<Quimico>(qu)).ToList());

                //TiposFicha
                cmds.AddRange((from tf in dc.TipoFicha select InsertCommand<TipoFicha>(tf)).ToList());

                //TiposFichaCultivo
                cmds.AddRange((from tfc in dc.TipoFichaCultivo select InsertCommand<TipoFichaCultivo>(tfc)).ToList());

                //Ficha
                cmds.AddRange((from fi in dc.Ficha where fi.IdTemporada == temporadaActiva.IdTemporada select InsertCommand<Ficha>(fi)).ToList());

                //Recomendacion
                cmds.AddRange((from re in dc.Recomendacion
                               join fi in dc.Ficha on re.IdFicha equals fi.IdFicha
                               where fi.IdTemporada == temporadaActiva.IdTemporada
                               select InsertCommand<Recomendacion>(re)).ToList());

                //FichaPotrero
                cmds.AddRange((from fp in dc.FichaPotrero
                         join fi in dc.Ficha on fp.IdFicha equals fi.IdFicha
                         where fi.IdTemporada == temporadaActiva.IdTemporada
                         select InsertCommand<FichaPotrero>(fp)).ToList());

                //FotosFicha
                cmds.AddRange((from ff in dc.FotoFicha
                               join fi in dc.Ficha on ff.IdFicha equals fi.IdFicha
                               where fi.IdTemporada == temporadaActiva.IdTemporada
                               select InsertCommand<FotoFicha>(ff)).ToList());

                //FichaPreSiembra
                cmds.AddRange((from fi in dc.FichaPreSiembra where fi.IdTemporada == temporadaActiva.IdTemporada select InsertCommand<FichaPreSiembra>(fi)).ToList());

                //RecomendacionPreSiembra
                cmds.AddRange((from re in dc.RecomendacionPreSiembra
                               join fi in dc.FichaPreSiembra on re.IdFichaPreSiembra equals fi.IdFichaPreSiembra
                               where fi.IdTemporada == temporadaActiva.IdTemporada
                               select InsertCommand<RecomendacionPreSiembra>(re)).ToList());

                //FichaPreSiembraPotrero
                cmds.AddRange((from fp in dc.FichaPreSiembraPotrero
                               join fi in dc.FichaPreSiembra on fp.IdFichaPreSiembra equals fi.IdFichaPreSiembra
                               where fi.IdTemporada == temporadaActiva.IdTemporada
                               select InsertCommand<FichaPreSiembraPotrero>(fp)).ToList());

                //FotosFichaPreSiembra
                cmds.AddRange((from ff in dc.FotoFichaPreSiembra
                               join fi in dc.FichaPreSiembra on ff.IdFichaPreSiembra equals fi.IdFichaPreSiembra
                               where fi.IdTemporada == temporadaActiva.IdTemporada
                               select InsertCommand<FotoFichaPreSiembra>(ff)).ToList());

                //IntecionSiembra
                cmds.AddRange((from s in dc.IntencionSiembra where s.IdTemporada == temporadaActiva.IdTemporada select InsertCommand<IntencionSiembra>(s)).ToList());

                using (var tran = con.BeginTransaction())
                {
                    foreach (var cmd in cmds)
                    {
                        cmd.Connection = con;
                        cmd.Transaction = tran;
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                    }

                    tran.Commit();
                    tran.Dispose();
                }
                
                con.Close();
            }

            GC.Collect();

            File.Copy(TempFile, FileName);

        }
    }


}