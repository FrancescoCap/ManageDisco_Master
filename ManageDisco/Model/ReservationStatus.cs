using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    public class ReservationStatusValue
    {
        public const int RESERVATIONSTATUS_PENDING = 1;
        public const int RESERVATIONSTATUS_APPROVED = 2;
        public const int RESERVATIONSTATUS_REJECTED = 3;
    }
    
    public class ReservationStatus
    {
        public int ReservationStatusId { get; set; }
        public string ReservationStatusValue { get; set; }
    }
}
