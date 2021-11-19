namespace AgroFichasWeb.Models
{
    public class Permiso
    {
        public Permiso() { }

        public Permiso(bool crear, bool leer, bool actualizar, bool borrar)
        {
            this.Crear = crear;
            this.Leer = leer;
            this.Actualizar = actualizar;
            this.Borrar = borrar;
        }

        public bool Actualizar                        { get; set; }
        public bool AdminControlVersiones             { get; set; }
        public bool AdminDctos                        { get; set; }
        public bool AdminFechasProduccion             { get; set; }
        public bool AdminFichaTecnicas                { get; set; }
        public bool AdminFrecuenciaAnalisis           { get; set; }
        public bool AutorizarOP                       { get; set; }
        public bool AutorizarRechazarEntradaItem      { get; set; }
        public bool AutorizarReprocesoSacosDañados    { get; set; }
        public bool Borrar                            { get; set; }
        public bool CALInformes                       { get; set; }
        public bool CALInformesPaletizacionPorTurno   { get; set; }
        public bool CerrarOP                          { get; set; }
        public bool Crear                             { get; set; }
        public bool CrearAnalisisPallet               { get; set; }
        public bool Leer                              { get; set; }
        public bool VerAnalisisPale                   { get; set; }
        public bool VerificarLC                       { get; set; }
        public bool VerificarRIT                      { get; set; }
        public bool VerCargaDividida                  { get; set; }
        public bool CrearCargaDividida                { get; set; }
        public bool AutorizarReproceso                { get; set; }
        public bool ReprocesarDespachosCargaGranel    { get; set; }
		public bool LiberarDespachosCargaGranel       { get; set; }
		public bool LiberarRetenidoPallet             { get; set; }
        public bool VerSacosDañados                   { get; set; }
        public bool CrearReprocesoSacosDañados        { get; set; }
        public bool VerReprocesoSacosDañados          { get; set; }
        public bool PaletizarSacosDañadosReprocesados { get; set; }
        public bool VerAreaCliente                    { get; set; }
        public bool CrearContrato                     { get; set; }
        public bool AnularContrato                    { get; set; }
        public bool VerContrato                       { get; set; }
    }
}