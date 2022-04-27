using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    public class ProductShopRow
    {
        public int ProductShopRowId { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int ProductShopRowQuantity { get; set; }
        public int ProductShopHeaderId { get; set; }
        public ProductShopHeader ProductShopHeader { get; set; }
    }
}
