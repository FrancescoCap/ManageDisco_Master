using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    public class TableOrderHeader
    {
        [Key]
        public int TableOrderHeaderId { get; set; }
        public decimal TableOrderHeaderSpending { get; set; }
        public decimal TableOrderHeaderExit { get; set; }
        public int TableId { get; set; }
        public Table Table { get; set; }
        public string TableOrderHeaderCouponCode { get; set; }
    }
    public class TableOrderSummary
    {
        public int TableOrderHeaderId { get; set; }
        public int TableId { get; set; }
        public string TableNam { get; set; }
        public decimal TableOrderHeaderSpending { get; set; }
        public decimal TableOrderHeaderExit { get; set; }
        public List<TableOrderRowView> Rows { get; set; }
    }
}
