namespace AgrotopApi.Models
{
    public class ResultOrdenProduccion
    {
        public bool OK { get; set; }
        public string Error { get; set; }
        public string rowKey { get; set; }
        public string FamiliaProductos { get; set; }
        public string Producto { get; set; }
        public string Espesor { get; set; }
        public string Saco { get; set; }
        public string PesoSaco { get; set; }
        public string Contenedor { get; set; }
        public string SacosPorContenedor { get; set; }
        public string cntCont { get; set; }
        public string cntSaco { get; set; }
        public string cntProd { get; set; }
        public string cntPorCont { get; set; }
    }
}