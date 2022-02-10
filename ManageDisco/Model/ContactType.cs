using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    /// <summary>
    /// Rappresenta il tipo di contantto (cel, email ecc...)
    /// </summary>
    public class ContactType
    {
        [Key]
        public int ContactTypeId { get; set; }
        public string ContactTypeDescription { get; set; }
    }
}
