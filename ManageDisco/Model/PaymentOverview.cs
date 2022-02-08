using ManageDisco.Model.UserIdentity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    /// <summary>
    /// Rappresenta gli importi totali dei pagamenti vs collaboratori.
    /// Viene aggiornata automaticamente da un trigger che si attiva in ReservationPayments
    /// </summary>
    public class PaymentOverview
    {
        public int PaymentOverviewId { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        /// <summary>
        /// Rappresenta il totale incassato dalle prenotazioni
        /// </summary>
        public decimal TotalIncoming { get; set; }  //somma di tutti i real budget
        /// <summary>
        /// Rappresenta il credito totale che il collaboratore avanza
        /// </summary>
        public decimal TotalCreditResume { get; set; }  //differenza fra il 10% di total incoming meno total credit payed
        /// <summary>
        /// Rappresenta il credito totale pagato
        /// </summary>
        public decimal TotalCreditPayed { get; set; }   //somma dei paymentsAmount in ReservationPayment
    }
    public class ReservationPaymentView
    {
        public int PaymentId { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        /// <summary>
        /// Totale incassato da prenotazioni
        /// </summary>
        public decimal TotalIncoming { get; set; }
        /// <summary>
        /// Totale pagato (10%)
        /// </summary>
        public decimal TotalPayed { get; set; }
        /// <summary>
        /// Totale da pagare
        /// </summary>
        public decimal ResumeCredit { get; set; }
    }

}
