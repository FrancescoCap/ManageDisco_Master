using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    /// <summary>
    /// Magazzino bottiglie
    /// </summary>
    public class Warehouse
    {
        public int WarehouseId { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int WarehouseQuantity { get; set; }
    }
    public class WarehouseView: Warehouse
    {
        public string ProductName { get; set; }
    }

    public class WarehousePut
    {
        public int ProductId { get; set; }
        public int WarehouseQuantity { get; set; }
    }
}
