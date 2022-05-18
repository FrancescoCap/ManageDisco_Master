using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    /**
     NON SONO INSCRITTE A DATABASE PERCHE' PER ORA NON LO REPUTO NECESSARIO.
     LE STATISTICHE SONO DATI DINAMICI
     */
    public class FreeEntrancePercentage
    {
        public int CouponSent { get; set; }
        public decimal CouponValidated { get; set; }
        public List<FreeEntrance> CouponList { get; set; }
    }
    public class FreeEntrance
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public bool Validated { get; set; }
    }

    public class EventTableOrder
    {
        /// <summary>
        /// Somma totale di tutti gli ordini dei tavoli
        /// </summary>
        public decimal TotalOrderTable { get; set; }
        public int TableCount { get; set; }
        /// <summary>
        /// Nr. di exit cambiati
        /// </summary>
        public decimal PeopleCountFromTable { get; set; }
        public List<EventTableOrderCoupon> TableCoupons { get; set; }

    }
    /// <summary>
    /// Rappresenta i tavoli che hanno utilizzato un coupon
    /// </summary>
    public class EventTableOrderCoupon
    {
        public string TableName { get; set; }
        public string CouponCode { get; set; }
        public string CouponDescription { get; set; }
    }
}
