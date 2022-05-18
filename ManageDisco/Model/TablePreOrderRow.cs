using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    public class TablePreOrderRow
    {
        [Key]
        public int TablePreOrderRowId { get; set; }
        public int TablePreOrderHeaderId { get; set; }
        public TableOrderHeader TablePreOrderHeader { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int TablePreOrderRowQuantity { get; set; }
    }
}
