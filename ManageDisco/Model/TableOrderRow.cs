﻿using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    public class TableOrderRow
    {
        public int TableOrderRowId { get; set; }
        public int TableOrderHeaderId { get; set; }
        public TableOrderHeader TableOrderHeader { get; set; }       
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int TableOrderRowQuantity { get; set; }
    }

    public class TableOrderRowView: TableOrderRow
    {
        public string ProductName { get; set; }
    }

    public class TableOrderPut
    {
        public Dictionary<int,int> ProductsId { get; set; }
        public decimal ProductsSpendingAmount { get; set; }
        public int ExitChanged { get; set; }
        /// <summary>
        /// Reppresenta un coupon per omaggi tavolo acquistati dallo shop
        /// </summary>
        public string ShopCoupon { get; set; }
        public int EventId { get; set; }
    }
}
