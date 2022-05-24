using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    /// <summary>
    /// Rappresenta lo shop dove l'utente può utilizzare i punti accumulati per ottenere premi
    /// </summary>
    public class ProductShopHeader
    {
        [Key]
        public int ProductShopHeaderIdId { get; set; }
        public string ProductShopHeaderName { get; set; }
        public string ProductShopHeaderDescription { get; set; }
        public decimal ProductShopHeaderPrice { get; set; }
        public int ProductShopTypeId { get; set; }
        public ProductShopType ProductShopType { get; set; }
        public string ProductShopImagePath { get; set; }
    }

    public class ProductShopPost
    {
        public string ProductShopHeaderName { get; set; }
        public string ProductShopHeaderDescription { get; set; }
        public decimal ProductShopHeaderPrice { get; set; }
        public int ProductShopTypeId { get; set; }        
        public List<ProductShopRow> Rows { get; set; }
        public string ProductShopImageName { get; set; }
        public string ProductShopBase64Image { get; set; }
    }

    public class ProductShopView
    {
        public bool CanUserHandleItems { get; set; }
        public List<ProductShopItems> Items { get; set; }
    }

    public class ProductShopItems
    {
        public int ProductShopHeaderIdId { get; set; }
        public string ProductShopHeaderName { get; set; }
        public string ProductShopHeaderDescription { get; set; }
        public decimal ProductShopHeaderPrice { get; set; }
        public int ProductShopTypeId { get; set; }
        public ProductShopType ProductShopType { get; set; }
        public List<ProductShopRow> ProductShopRow { get; set; }
        public string ProductShopImagePath { get; set; }
        public string productShopBase64Image { get; set; }
    }
}

