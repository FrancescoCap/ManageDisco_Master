using ManageDisco.Model.UserIdentity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    public class ReservationUserCode
    {
        public int ReservationUserCodeId { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public string ReservationUserCodeValue { get; set; }
    }
}
