using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    /// <summary>
    /// Rappresenta le aree in cui ci sono i tavoli. Ogni personale di ogni discoteca
    /// </summary>
    public class Table
    {
        [Key]
        public int TableId { get; set; }
        public string TableAreaDescription { get; set; }
        public string TableNumber { get; set; }
        public string DiscoEntityId { get; set; }
        public DiscoEntity DiscoEntity { get; set; }
        public decimal TableMinBudget { get; set; }
    }     
    
    public class TableAssignPost
    {
        public int EventId { get; set; }
        public int ReservationId { get; set; }
        public int TableId { get; set; }
    }
    /// <summary>
    /// Modello per la restituzione del file PDF della piantina
    /// </summary>
    public class TableMapReponse
    {
        public string FileName { get; set; }
        public string Path { get; set; }
    }
    /// <summary>
    /// Modello padre per la visualizzazione dei tavoli dell'evento
    /// </summary>
    public class TableOrderViewHeader 
    {
        public int EventId { get; set; } 
        public List<TableOrderView> Tables { get; set; } = new List<TableOrderView>();
    }

    /// <summary>
    /// Modello per la tabella di visualizzazione dei tavoli della serata con proprietà aggiuntive
    /// </summary>
    public class TableOrderView: Table
    {
        public string TableName { get; set; }
        public int EventId { get; set; }
        /// <summary>
        /// Data di esistenza del tavolo. Corrisponde sempre alla data di svolgimento dell'evento
        /// </summary>
        public DateTime TableDate { get; set; }
        /// <summary>
        /// True se il tavolo ha effettuato almeno un ordine
        /// </summary>
        public bool HasOrder { get; set; }
    }

}
