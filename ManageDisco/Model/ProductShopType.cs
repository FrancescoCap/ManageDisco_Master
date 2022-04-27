using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    public static class ProductShopTypeContants
    {
        public const string PRODUCT_SHOP_TYPE_TABLE = "TAVOLO";
        public const string PRODUCT_SHOP_TYPE_ENTRY = "INGRESSO";
        public const string PRODUCT_SHOP_TYPE_PRODUCT = "PRODOTTO";
    }
    public class ProductShopType
    {
        public int ProductShopTypeId { get; set; }
        public string ProductShopTypeDescription { get; set; }
    }
}
