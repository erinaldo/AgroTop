using FileHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Importer
{
    class Program
    {
        static void Main(string[] args)
        {
            DoTrigo();
        }
        

        private static void DoRaps()
        {
            var engine = new FileHelperEngine(typeof(ContratoRaps));

            ContratoRaps[] contratos = engine.ReadFile(@"G:\Clientes\Agrotop\AgroFichas\Trabajo\Datos 2013\061213 OLEOTOP  2013-2014.tsv") as ContratoRaps[];

            string agricultores = "";
            decimal[] tot = { 0, 0, 0, 0, 0, 0, 0, 0 };

            foreach (var contrato in contratos)
            {
                var rut = contrato.Rut.Replace(".", "").Replace(",", "").Replace("\"", "").Replace(" ", "").ToUpper();

                var agparams = new object[] { 
                    rut, contrato.Nombre, contrato.Email, contrato.Fono,  1, "OLT-" + contrato.NroContrato,
                    contrato.SupTradicional, contrato.VolTradicional * 1000, 1,
                    contrato.SupUltratop ?? 0, contrato.VolUltratop * 1000, 2,
                    1, (contrato.Precio1 > 1000 ? contrato.Cantidad1 * 1000 : 0), (contrato.Precio1 > 1000 ? contrato.Precio1 / 1000 : 0),
                    1, (contrato.Precio2 > 1000 ? contrato.Cantidad2 * 1000 : 0), (contrato.Precio2 > 1000 ? contrato.Precio2 / 1000 : 0)
                };

                var ag = "exec AddContrato '{0}', '{1}', '{2}', '{3}', {4}, '{5}', {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, 'importer' \n";

                agricultores += String.Format(ag, agparams);


                //Console.WriteLine(contrato.NroContrato + "\t" + rut + "\t" +
                //    contrato.SupTradicional + "\t" +
                //    contrato.VolTradicional + "\t" +
                //    contrato.SupUltratop + "\t" +
                //    contrato.VolUltratop + "\t" 
                //    );

                tot[0] += contrato.SupTradicional;
                tot[1] += contrato.VolTradicional;
                tot[2] += contrato.SupUltratop ?? 0;
                tot[3] += contrato.VolUltratop;
                tot[4] += contrato.Cantidad1 ?? 0;
                tot[5] += contrato.Precio1 ?? 0;
                tot[6] += contrato.Cantidad2 ?? 0;
                tot[7] += contrato.Precio2 ?? 0;
            }

            Console.WriteLine(tot[0]);
            Console.WriteLine(tot[1]);
            Console.WriteLine(tot[2]);
            Console.WriteLine(tot[3]);

            Console.WriteLine(tot[4]);
            Console.WriteLine(tot[5]);
            Console.WriteLine(tot[6]);
            Console.WriteLine(tot[7]);

            File.WriteAllText("result.sql", agricultores);

            Console.ReadLine();
        }
        private static void DoAvena()
        {
            var engine = new FileHelperEngine(typeof(ContratoAvena));

            ContratoAvena[] contratos = engine.ReadFile(@"G:\Clientes\Agrotop\AgroFichas\Trabajo\Datos 2013\20131206 AVENATOP 2013-2014 .tsv") as ContratoAvena[];

            string agricultores = "";
            decimal[] tot = { 0, 0, 0, 0, 0, 0, 0, 0 };

            foreach (var contrato in contratos)
            {
                var rut = contrato.Rut.Replace(".", "").Replace(",", "").Replace(" ", "").Replace("\"", "").ToUpper();

                var agparams = new object[] { 
                    rut, contrato.Nombre, contrato.Email, contrato.Fono,  2, "AVT-" + contrato.NroContrato,
                    contrato.SupTradicional, contrato.VolTradicional * 1000, 3,
                    contrato.SupSymphony ?? 0, contrato.VolSymphony * 1000, 4,
                    1, (contrato.Precio1 > 0 ? contrato.Cantidad1 * 1000 : 0), (contrato.Precio1 > 0 ? contrato.Precio1 : 0),
                    1, 0, 0
                };

                var ag = "exec AddContrato '{0}', '{1}', '{2}', '{3}', {4}, '{5}', {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, 'importer' \n";

                agricultores += String.Format(ag, agparams);


                Console.WriteLine(contrato.NroContrato + "\t" + rut + "\t" +
                    contrato.Cantidad1 + "\t" +
                    contrato.Precio1 + "\t"
                    );

                tot[0] += contrato.SupTradicional;
                tot[1] += contrato.VolTradicional;
                tot[2] += contrato.SupSymphony ?? 0;
                tot[3] += contrato.VolSymphony;
                tot[4] += contrato.Cantidad1;
                tot[5] += contrato.Precio1;
            }

            Console.WriteLine(tot[0]);
            Console.WriteLine(tot[1]);
            Console.WriteLine(tot[2]);
            Console.WriteLine(tot[3]);

            Console.WriteLine(tot[4]);
            Console.WriteLine(tot[5]);

            File.WriteAllText("result.sql", agricultores);

            Console.ReadLine();
        }

        private static void DoTrigo()
        {
            var engine = new FileHelperEngine(typeof(ContratoTrigo));

            ContratoTrigo[] contratos = engine.ReadFile(@"G:\Clientes\Agrotop\AgroFichas\Trabajo\Datos 2013\131126 GRANOTOP CALUGA 2013-2014.tsv") as ContratoTrigo[];

            string agricultores = "";
            decimal[] tot = { 0, 0, 0, 0, 0, 0, 0, 0 };

            foreach (var contrato in contratos)
            {
                var rut = contrato.Rut.Replace(".", "").Replace(",", "").Replace("\"", "").Replace(" ", "").ToUpper();

                var agparams = new object[] { 
                    rut, contrato.Nombre, contrato.Email, contrato.Fono,  3, "GRT-"+ contrato.Prefix + "-" + contrato.NroContrato,
                    contrato.Superficie, contrato.Volumen * 1000, contrato.IdCultivo,
                    0, 0, 0,
                    1, 0, 0,
                    1, 0, 0
                };

                var ag = "exec AddContrato '{0}', '{1}', '{2}', '{3}', {4}, '{5}', {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, 'importer' \n";

                agricultores += String.Format(ag, agparams);


                //Console.WriteLine(contrato.NroContrato + "\t" + rut + "\t" +
                //    contrato.Cantidad1 + "\t" +
                //    contrato.Precio1 + "\t"
                //);

                tot[0] += contrato.Superficie;
                tot[1] += contrato.Volumen;
            }

            Console.WriteLine(tot[0]);
            Console.WriteLine(tot[1]);

            File.WriteAllText("result.sql", agricultores);

            Console.ReadLine();
        }
    }

    [DelimitedRecord("	")]
	public class ContratoRaps
	{
        [FieldTrim(TrimMode.Both)]
        public string NroContrato;

        [FieldTrim(TrimMode.Both)]
	    public string Nombre;

        [FieldTrim(TrimMode.Both)]
        public string Rut;

        [FieldTrim(TrimMode.Both)]
        public string Fono;

        [FieldTrim(TrimMode.Both)]
        public string Email;

        public decimal SupTradicional;

        public decimal VolTradicional;

        public decimal VolUltratop;

        public decimal? SupUltratop;

        public string placeholder1;

        public string placeholder2;

        public string Fecha1;

        public decimal? Cantidad1;

        public string NroContratos1;

        public decimal? Precio1;

        public string Fecha2;

        public decimal? Cantidad2;

        public string NroContratos2;

        public decimal? Precio2;

        public string therest;  
	}

    [DelimitedRecord("	")]
    public class ContratoAvena
    {
        [FieldTrim(TrimMode.Both)]
        public string NroContrato;

        [FieldTrim(TrimMode.Both)]
        public string Nombre;

        [FieldTrim(TrimMode.Both)]
        public string Rut;

        [FieldTrim(TrimMode.Both)]
        public string Fono;

        [FieldTrim(TrimMode.Both)]
        public string Email;

        public decimal SupTradicional;

        public decimal VolTradicional;

        public decimal? SupSymphony;

        public decimal VolSymphony;

        [FieldNullValue(typeof(Decimal), "0")]
        public decimal Cantidad1;

        [FieldNullValue(typeof(Decimal), "0")]
        public decimal Precio1;

        public string therest;
    }

    [DelimitedRecord("	")]
    public class ContratoTrigo
    {
        public int IdCultivo;

        public string Prefix;

        [FieldTrim(TrimMode.Both)]
        public string NroContrato;

        [FieldTrim(TrimMode.Both)]
        public string Nombre;

        [FieldTrim(TrimMode.Both)]
        public string Rut;

        [FieldTrim(TrimMode.Both)]
        public string Fono;

        [FieldTrim(TrimMode.Both)]
        public string Email;

        public decimal Superficie;

        public decimal Volumen;
    } 
}
