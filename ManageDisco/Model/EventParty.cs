using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    public class EventParty
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public string Description { get; set; }
        public int MaxAge { get; set; }

        public DateTime Date { get; set; }
        
        /// <summary>
        /// Prezzo ingresso in serata
        /// </summary>
        public decimal EntrancePrice { get; set; }
        /// <summary>
        /// Prezzo tavolo
        /// </summary>
        public decimal TablePrice { get; set; }
        /// <summary>
        /// Descrizione condizioni per l'omaggio
        /// </summary>
        public string FreeEntranceDescription { get; set; }
        public bool FreeEntranceEnabled { get; set; }
        
    }
    /// <summary>
    /// Modello figlio dalla classe padre EventParty per ereditare tutti i campi descrittivi.
    /// Contiene un una lista di stringhe per il riferimento alle immagini. Il campo non è in db.
    /// Viene creato per gestire principalmente l'inserimento degli eventi con relative immagini.
    /// </summary>
    public class EventPartyImages : EventParty
    {
        public List<string> LinkImage { get; set; } = new List<string>();
    }

    public class EventPartyDetail : EventParty
    {
        public bool EventIsEnd { get; set; }
        public bool UserCanEditInfo { get; set; }
        public bool UserCanEnrollFreeEntrance { get; set; }
        public string EventPartyStatusDescription { get; set; }
        /// <summary>
        /// Lista con tutte le immagini in base64
        /// </summary>
        public List<string> LinkImage { get; set; } = new List<string>();
    }

    public class EventPartyList: EventParty
    {
        public string EventPartyStatusDescription { get; set; }
        public bool UserHasReservation { get; set; }
        /// <summary>
        /// Base64 dell'immagine di copertina
        /// </summary>
        public string ImagePreview { get; set; }
    }
    /// <summary>
    /// Panoramica generale degli eventi (in client rappresentate in tabella) con flag aggiuntivi per le azioni dell'utente
    /// </summary>
    public class EventPartyOverview
    {
        public bool UserCanAddReservation { get; set; }
        public bool UserCanAddEvent { get; set; }
        public bool UserCanDeleteEvent { get; set; }        
        public List<EventPartyList> Events { get; set; }
    }
    /// <summary>
    /// Modello utilizzato per i campi modificabili
    /// </summary>
    public class EventPartyPutModel
    {
        /// <summary>
        /// Prezzo ingresso in serata
        /// </summary>
        public decimal EntrancePrice { get; set; }
        /// <summary>
        /// Prezzo tavolo
        /// </summary>
        public decimal TablePrice { get; set; }
        /// <summary>
        /// Descrizione condizioni per l'omaggio
        /// </summary>
        public string FreeEntranceDescription { get; set; }
        public string Description { get; set; }
    }
}
