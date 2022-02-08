using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    /// <summary>
    /// Tabella che contiene tutti i clienti che si sono registrati con un determinato codice PR
    /// </summary>
    public class PrCustomer
    {
        [Key]
        public int PrCustomerId { get; set; }
        public string PrCustomerCustomerid { get; set; }
        public string PrCustomerPrId { get; set; }
    }
    /// <summary>
    /// Modello per la visualizzazione dei clienti associati al pr
    /// </summary>
    public class PrCustomerView
    {
        public string CustomerId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
    }
}
