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
        public List<EventPartyImages> Events { get; set; }
        
    }
}
