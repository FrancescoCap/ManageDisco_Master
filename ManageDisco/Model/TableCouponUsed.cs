using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    /// <summary>
    /// Tabella di storico dell'utilizzo di coupon tavolo per gli ordini. Tabella necessaria a fini statistici
    /// </summary>
    public class TableCouponUsed
    {
        [Key]
        public int TableCouponId { get; set; }
        public int TableCouponTableId { get; set; }
        public string TableCouponCouponCode { get; set; }
        public int TableCouponEventId { get; set; }
        
    }
}
