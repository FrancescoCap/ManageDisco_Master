using ManageDisco.Model.UserIdentity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    public class Coupon
    {
        [Key]
        public int CouponId { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public int EventId { get; set; }
        public EventParty Event { get; set; }
        public bool CouponValidated { get; set; }
    }

    public class CouponResponse:Coupon
    {
        public string Link { get; set; }
        public Bitmap ImageStream { get; set; }
    }

    public class CouponValidation
    {
        public List<CouponValidationRow> Products { get; set; } = new List<CouponValidationRow>();
    }

    public class CouponValidationRow
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int ProductQuantity { get; set; }
    }

}
