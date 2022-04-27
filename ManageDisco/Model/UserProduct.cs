using ManageDisco.Model.UserIdentity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    public class UserProduct
    {
        [Key]
        public int UserProductId { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public int ProductShopHeaderId { get; set; }
        public ProductShopHeader ProductShopHeader { get; set; }
        /// <summary>
        /// True se il coupon è stato utilizzato
        /// </summary>
        public bool UserProductUsed { get; set; }
        public string UserProductCode { get; set; }
    }
}
