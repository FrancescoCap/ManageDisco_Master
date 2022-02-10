using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    /// <summary>
    /// Tabella per l'associazione delle varie foto (copertina, dettaglio ecc..) all'evento
    /// </summary>
    public class EventPhoto
    {
        public int EventPhotoId { get; set; }
        public string EventPhotoImagePath { get; set; }
        public int EventPhotoEventId { get; set; }
        public int PhotoTypeId { get; set; }
        public PhotoType PhotoType { get; set; }
    }
}
