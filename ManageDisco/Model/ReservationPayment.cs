using ManageDisco.Model.UserIdentity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    /// <summary>
    /// Tabella per salvare i pagamenti fatti ai collaboratori per le prenotazioni effettuate
    /// </summary>
    public class ReservationPayment
    {
        public int ReservationPaymentId { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        /// <summary>
        /// Somma pagata
        /// </summary>
        public decimal ReservationPaymentAmount { get; set; }
        /// <summary>
        /// Data pagamento
        /// </summary>
        public DateTime ReservationPaymentDate { get; set; } = DateTime.Now;
        public List<Reservation> Reservations { get; set; }
        /// <summary>
        /// Causale
        /// </summary>
        public string ReservationPaymentDescription { get; set; }
    }    
}
