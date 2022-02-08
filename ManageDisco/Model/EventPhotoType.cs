using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    public class EventPhotoDescriptionValues
    {
        public static readonly string EVENT_IMAGE_TYPE_COVER = "COPERTINA"; //Home
        public static readonly string EVENT_IMAGE_TYPE_PREVIEW = "ANTEPRIMA";   //Lista eventi
        public static readonly string EVENT_IMAGE_TYPE_DETAILS_ONE = "DETTAGLIO_1"; //Slider dettaglio eventi
        public static readonly string EVENT_IMAGE_TYPE_DETAILS_TWO = "DETTAGLIO_2"; //Slider dettaglio eventi
        public static readonly string EVENT_IMAGE_TYPE_DETAILS_THREE = "DETTAGLIO_3"; //Slider dettaglio eventi

    }
    /// <summary>
    /// Tabella per il tipo di foto (copertina, dettaglio evento ecc...)
    /// </summary>
    public class EventPhotoType
    {
        public int EventPhotoTypeId { get; set; }
        public string EventPhotoDescription { get; set; }
    }
}
