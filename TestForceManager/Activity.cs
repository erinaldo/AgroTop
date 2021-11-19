using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestForceManager
{
    class Activity
    {
        public Z_Agro1 Z_Agro1 { get; set; }
        public Z_Agro2 Z_Agro2 { get; set; }
        public Z_Agro3 Z_Agro3 { get; set; }
        public Z_Agro4 Z_Agro4 { get; set; }
        public Z_Agro5 Z_Agro5 { get; set; }
        public Z_CultivoAnterior Z_CultivoAnterior { get; set; }
        public string Z_Dosis { get; set; }
        public string Z_Dosis1 { get; set; }
        public string Z_Dosis2 { get; set; }
        public string Z_Dosis3 { get; set; }
        public string Z_Dosis4 { get; set; }
        public string Z_Dosis5 { get; set; }
        public string Z_Obs1 { get; set; }
        public string Z_Obs2 { get; set; }
        public string Z_Obs3 { get; set; }
        public string Z_Obs4 { get; set; }
        public string Z_Obs5 { get; set; }
        public string Z_Potrero { get; set; }
        public string Z_UMedida1 { get; set; }
        public string Z_UMedida2 { get; set; }
        public string Z_UMedida3 { get; set; }
        public string Z_UMedida4 { get; set; }
        public string Z_UMedida5 { get; set; }
        public Z_Variedad Z_Variedad { get; set; }
        public accountId accountId { get; set; }
        public bool? checkin { get; set; }
        public int? checkinTypeId { get; set; }
        public DateTime? checkoutDate { get; set; }
        public string comment { get; set; }
        public contactId contactId { get; set; }
        public DateTime? date { get; set; }
        public DateTime? dateCreated { get; set; }
        public DateTime? dateDeleted { get; set; }
        public DateTime? dateUpdated { get; set; }
        public bool deleted { get; set; }
        public int? extId { get; set; }
        public bool? geocoded { get; set; }
        public int? geocodingAccuracy { get; set; }
        public int? id { get; set; }
        public float? latitude { get; set; }
        public float? longitude { get; set; }
        public opportunityId opportunityId { get; set; }
        public int? permissionLevel { get; set; }
        public bool? readOnly { get; set; }
        public salesRepId salesRepId { get; set; }
        public int? salesRepIdCreated { get; set; }
        public int? salesRepIdDeleted { get; set; }
        public int? salesRepIdUpdated { get; set; }
        public typeId typeId { get; set; }
    }

    class Z_Agro1
    {
        public int id { get; set; }
        public string value { get; set; }
    }

    class Z_Agro2
    {
        public int id { get; set; }
        public string value { get; set; }
    }

    class Z_Agro3
    {
        public int id { get; set; }
        public string value { get; set; }
    }

    class Z_Agro4
    {
        public int id { get; set; }
        public string value { get; set; }
    }

    class Z_Agro5
    {
        public int id { get; set; }
        public string value { get; set; }
    }

    class Z_CultivoAnterior
    {
        public int id { get; set; }
        public string value { get; set; }
    }

    class Z_Variedad
    {
        public int id { get; set; }
        public string value { get; set; }
    }

    class accountId
    {
        public int id { get; set; }
        public string value { get; set; }
    }

    class contactId
    {
        public int id { get; set; }
        public string value { get; set; }
    }

    class opportunityId
    {
        public int id { get; set; }
        public string value { get; set; }
    }

    class salesRepId
    {
        public int id { get; set; }
        public string value { get; set; }
    }

    class typeId
    {
        public int id { get; set; }
        public string value { get; set; }
    }
}