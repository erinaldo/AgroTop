using AgroFichasWeb.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;

namespace AgroFichasWeb.ViewModels
{
    public class ImportarSaldosCtaCteViewModel
    {
        [Required]
        public int IdEmpresa { get; set; }

        [Required(ErrorMessage="Seleccione el archivo a importar")]
        public string FilePath { get; set; }

        public List<SaldoCtaCteCSV> Items { get; private set; }

        public int CuentaItems { get; private set; }
        public long TotalMontoDocumentado { get; private set; }
        public long TotalMontoSaldoCtaCte{ get; private set; }

        public void Load()
        {
            Items = new List<SaldoCtaCteCSV>();

            var lines = File.ReadAllLines(this.FilePath);

            int i = 0;
            foreach (var line in lines)
            {
                i++;
                var item = new SaldoCtaCteCSV() { Raw = line };

                var data = line.Split('\t');
                if (data.Length >= 4)
                {
                    item.NumeroLinea = i;
                    item.Rut = data[0].Trim().Replace(".", "").Replace(",", "").Replace(" ", "").ToUpper();
                    item.Nombre = data[1].Trim();
                    item.MontoDocumentado = ParseLong(data[2]);
                    item.MontoCtaCte = ParseLong(data[3]);

                    if (item.MontoDocumentado != 0 || item.MontoCtaCte != 0)
                    {
                        item.Valid = true;
                        if (item.Rut.Length < 8 || item.Rut.Length > 11 || !item.Rut.Contains("-"))
                        {
                            item.Valid = false;
                            item.ValidMessage = "El Rut no es válido";
                        }
                        else if (item.Nombre.Length < 5)
                        {
                            item.Valid = false;
                            item.ValidMessage = "El Nombre es muy corto (5 mínimo)";
                        }
                        else if (item.Nombre.Length > 200)
                        {
                            item.Valid = false;
                            item.ValidMessage = "El Nombre es muy largo (200 máximo)";
                        }
                        Items.Add(item);
                    }
                }

            }

            this.CuentaItems = Items.Count;
            this.TotalMontoDocumentado = Items.Sum(it => it.MontoDocumentado);
            this.TotalMontoSaldoCtaCte = Items.Sum(it => it.MontoCtaCte);
        }

       
        public void Persist(string userName, string ip)
        {
            var dc = new AgroFichasDBDataContext();
            var dcAg = new AgroFichasDBDataContext();

            var updateSerial = Path.GetFileName(this.FilePath).Substring(0, 8);

            foreach (var item in this.Items)
            {
                var saldo = dc.SaldoCtaCte.FirstOrDefault(s => s.IdEmpresa == this.IdEmpresa && s.Agricultor.Rut == item.Rut);
                if (saldo == null)
                {
                    if (item.MontoCtaCte != 0 || item.MontoDocumentado != 0)
                    {
                        var agricultor = dc.Agricultor.FirstOrDefault(a => a.Rut == item.Rut);
                        if (agricultor == null)
                        {
                            agricultor = new Agricultor()
                            {
                                Rut = item.Rut,
                                Nombre = item.Nombre,
                                Email = "",
                                Fono1 = "",
                                Fono2 = "",
                                IDAvenatop = "",
                                IDGranotop = "",
                                IDOleotop = "",
                                Habilitado = true,
                                IdProveedor = 1,
                                MobileTag = "",
                                UserIns = userName,
                                FechaHoraIns = DateTime.Now,
                                IpIns = ip

                            };
                            dcAg.Agricultor.InsertOnSubmit(agricultor);
                            dcAg.SubmitChanges();
                        }

                        saldo = new SaldoCtaCte()
                        {
                            IdEmpresa = this.IdEmpresa,
                            Comentarios = "",
                            UserIns = userName,
                            FechaHoraIns = DateTime.Now,
                            IpIns = ip,
                            IdAgricultor = agricultor.IdAgricultor
                        };

                        dc.SaldoCtaCte.InsertOnSubmit(saldo);
                    }
                    else
                    {
                        //El saldo no existe en la BD y tiene valor 0 en el archivo => no lo insertamos
                        continue;
                    }
                }
                else
                {
                    saldo.UserUpd = userName;
                    saldo.FechaHoraUpd = DateTime.Now;
                    saldo.IpUpd = ip;
                }

                saldo.UpdateSerial = updateSerial;
                saldo.MontoCtaCte = (int)item.MontoCtaCte;
                saldo.MontoDocumentado = (int)item.MontoDocumentado;
            }

            dc.SubmitChanges();
            dc.CleanSaldoCtaCteAfterImport(this.IdEmpresa, updateSerial);
        }

        private long ParseLong(string str)
        {
            str = str.Trim().Replace("\"", "").Replace("'", "").Replace(" ", "").Replace(".", "").Replace(",", "");
            long value;
            if (long.TryParse(str, out value))
                return value;
            else
                return 0;
        }
    }

	public class SaldoCtaCteCSV
	{
        public int NumeroLinea;

        public string Rut;

        public string Nombre;

	    public long MontoDocumentado;

        public long MontoCtaCte;

        public bool Valid;

        public string ValidMessage;

        public string Raw;
	}
}