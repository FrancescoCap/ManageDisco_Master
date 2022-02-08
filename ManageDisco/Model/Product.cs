using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public int CatalogId { get; set; }
        public Catalog Catalog { get; set; }
    }

    public class ProductCatalogView
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string CatalogName { get; set; }
        public int CatalogId { get; set; }
        public decimal ProductPrice { get; set; }
    }
}
