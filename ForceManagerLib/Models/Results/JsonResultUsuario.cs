using System;

namespace ForceManagerLib.Models.Results
{
    public class JsonResultUsuario
    {
        public int id { get; set; }

        public string name { get; set; }

        public string lastName { get; set; }

        public string phone { get; set; }

        public string email { get; set; }

        public DateTime dateCreated { get; set; }

        public DateTime? dateDeleted { get; set; }

        public DateTime? dateUpdated { get; set; }
    }
}
