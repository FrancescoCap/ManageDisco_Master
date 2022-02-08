using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    public class Catalog
    {
        [Key]
        public int CatalogId { get; set; }
        public string CatalogName { get; set; }
    }
}
