using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    /// <summary>
    /// Rappresenta i contatti del locale
    /// </summary>
    public class Contact
    {
        [Key]
        public int ContactId { get; set; }
        public string ContactDescription { get; set; }
        public int ContactTypeId { get; set; }
        public ContactType ContactType { get; set; }
    }

    public class ContactView:Contact
    {
        public string ContactTypeDescription { get; set; }
    }

    public class ContactGroup
    {
        public int ContactTypeId { get; set; }
        public string ContactTypeDescription { get; set; }
        public List<string> ContactsValues { get; set; }

    }
}
