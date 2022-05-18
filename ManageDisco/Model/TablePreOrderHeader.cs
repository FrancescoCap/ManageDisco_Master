using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    public class TablePreOrderHeader
    {
        [Key]
        public int TablePreOrderHeaderId { get; set; }
        public decimal TablePreOrderHeaderSpending { get; set; }
        public decimal TablePreOrderHeaderExit { get; set; }
        public int ReservationId { get; set; }
        public Reservation Reservation { get; set; }
        public string TableOrderHeaderCouponCode { get; set; }
        public bool IsAccepted { get; set; }
    }
}
