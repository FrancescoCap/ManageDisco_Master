using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    public class EventPhotoDescriptionValues
    {
        public static readonly string EVENT_IMAGE_TYPE_COVER = "Evento_Copertina"; //Home
        public static readonly string EVENT_IMAGE_TYPE_PREVIEW = "Evento_Anteprima";   //Lista eventi
        public static readonly string EVENT_IMAGE_TYPE_EVENT_DETAIL = "Evento_Dettaglio";   //Lista eventi

        //Parola chiave da utilizzare come elemento di paragone nel LIKE per ottenre tutte le phototypeId per l'evento
        public static readonly string EVENT_IMAGE_FILTER_LIKE = "Evento";
        public static readonly string HOME_IMAGE_FILTER_LIKE = "Home";
        public static readonly string HOME_IMAGE_MOMENTS_FILTER_LIKE = "Home_Momenti";

        //public static readonly string EVENT_IMAGE_TYPE_DETAILS_ONE = "Evneto_Dettaglio_1"; //Slider dettaglio eventi
        //public static readonly string EVENT_IMAGE_TYPE_DETAILS_TWO = "Evento_Dettaglio_2"; //Slider dettaglio eventi
        //public static readonly string EVENT_IMAGE_TYPE_DETAILS_THREE = "Evento_Dettaglio_3"; //Slider dettaglio eventi

    }
    /// <summary>
    /// Tabella per il tipo di foto (copertina, dettaglio evento ecc...)
    /// </summary>
    public class PhotoType
    {
        public int PhotoTypeId { get; set; }
        public string PhotoTypeDescription { get; set; }
        public int PhotoTypeMaxNumber { get; set; }
    }
}
