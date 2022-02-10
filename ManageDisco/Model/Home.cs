using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    /// <summary>
    /// Contiene tutte le informazioni che devono essere mostrate sulla home (eventi, foto serate ecc...). Per ora non la iscrivo a db
    /// </summary>
    public class Home
    {
        public List<HomePhotoValue> HomePhoto { get; set; }
        public List<EventPartyImages> Events { get; set; }
        public List<PhotoType> PhotoType { get; set; }
        public List<ContactGroup> Contacts { get; set; }
        public DiscoEntity DiscoEntity { get; set; }

    }
}
