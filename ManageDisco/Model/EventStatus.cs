using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{

    public class EventStatus
    {
        public int EventStatusId { get; set; }
        public string EventStatusDescription { get; set; }
    }

    public class EventStatusConstants
    {
        public static string STATUS_SCHEDULED = "Programmato";
        public static string STATUS_ONGOING = "In corso";
        public static string STATUS_END = "Terminato";
        public static string STATUS_CANCELLED = "Annullato";
    }
}
