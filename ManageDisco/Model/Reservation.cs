using ManageDisco.Model.UserIdentity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    public class Reservation
    {
        public int ReservationId { get; set; }
        /// <summary>
        /// Codice prenotazione generato
        /// </summary>
        public string ReservationCode { get; set; }
        public int ReservationPeopleCount { get; set; }
        public int EventPartyId { get; set; }
        public EventParty EventParty { get; set; }
        /// <summary>
        /// Id del PR con il quale è stata effettuata la prenotazione
        /// </summary>
        public string UserId { get; set; }
        public User User { get; set; }
        /// <summary>
        /// Id dell'utente CLIENTE a cui è "intestato" il tavolo
        /// </summary>
        public string UserIdOwner { get; set; }
        /// <summary>
        /// Codice di prenotazione del PR
        /// </summary>
        public string ReservationUserCodeValue { get; set; }
        public DateTime ReservationDate { get; set; } = DateTime.Today;
        /// <summary>
        /// Tipo prenotazione
        /// </summary>
        public int ReservationTypeId { get; set; }
        public ReservationType ReservationType { get; set; }
        /// <summary>
        /// Budget provvisorio per prenotazioni tavolo
        /// </summary>
        public int ReservationExpectedBudget { get; set; }
        /// <summary>
        /// Budget effettivo del tavolo
        /// </summary>
        public int ReservationRealBudget { get; set; }
        /// <summary>
        /// Indica se la prenotazione è stata accettata
        /// </summary>
        public int ReservationStatusId { get; set; }
        public ReservationStatus ReservationStatus { get; set; }
        /// <summary>
        /// Note da allegare alla prenotazione
        /// </summary>
        public string ReservationNote { get; set; }
        /// <summary>
        /// Note da allegare all'accettazione/rifiuto della prenotazione
        /// </summary>
        public string ReservationConfirmationNote { get; set; }        
        /// <summary>
        /// Lista dei pagamenti al PR per la prentazione
        /// </summary>
        public List<ReservationPayment> ReservationPayment { get; set; }       
        /// <summary>
        /// Nr. effettivo di exit cambiati. 
        /// </summary>
        public int ReservationRealPeopleCount { get; set; } //Questo dato sarà modificato dal "magazziniere"
        /// <summary>
        /// Tavolo assegnato alla prenotazione (vedi piantina)
        /// </summary>
        public int? TableId { get; set; } = null;        
        public Table Table { get; set; }
        /// <summary>
        /// Identificativo della prenotazione (nome tavolo)
        /// </summary>
        public string ReservationTableName { get; set; }
    }

    public class ReservationView
    {
        public int ReservationId { get; set; }
        public string ReservationCode { get; set; }
        public int ReservationPeopleCount { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string ReservationUserCode { get; set; }
        public DateTime ReservationDate { get; set; }
        public string ReservationTypeValue { get; set; }
        /// <summary>
        /// Budget provvisorio per prenotazioni tavolo
        /// </summary>
        public int ReservationExpectedBudget { get; set; }
        /// <summary>
        /// Budget effettivo del tavolo
        /// </summary>
        public int ReservationRealBudget { get; set; }
        /// <summary>
        /// Utente che ha prenotato 
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// Utente a cui è riferita la prenotazione
        /// </summary>
        public string UserIdOwner { get; set; }
        /// <summary>
        /// True se l'utente può accettare le richieste di prenotazione
        /// </summary>
        public bool CanAcceptReservation { get; set; }
        /// <summary>
        /// True se la prenotazione è modificabile
        /// </summary>
        public bool IsReservationEditable { get; set; }
        /// <summary>
        /// True se è possibile confermare il budget
        /// </summary>
        public bool CanAcceptBudget { get; set; }
        public int ReservationStatusId { get; set; }
        public string ReservationStatus { get; set; }
        /// <summary>
        /// Nome tavolo
        /// </summary>
        public string ReservationName { get; set; }
        /// <summary>
        /// Nr. tavolo assegnato
        /// </summary>
        public string ReservationTablAssigned { get; set;  }
        public int? TableId { get; set; }
    }

    public class ReservationViewTable
    {
       public bool CanAssignTable { get; set; }
       public IQueryable<ReservationView> Reservations { get; set; }
        
    }

    public class ReservationPost
    {        
        public int ReservationId { get; set; }
        public string ReservationUserCodeValue { get; set; }
        public int ReservationPeopleCount { get; set; }
        public int EventPartyId { get; set; }
        public int ReservationType { get; set; }
        public int ReservationExpectedBudget { get; set; }
        public int ReservationRealBudget { get; set; }
        public int TableId { get; set; }
        public string ReservationNote { get; set; }
        public string ReservationName { get; set; }
        public string ReservationOwnerId { get; set; }
    }

    public class ReservationResponse
    {
        /// <summary>
        /// Codice associato alla prenotazione
        /// </summary>
        public string ReservationCode { get; set; }
        public string Message { get; set; }
    }
}
