using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    public class HomePhoto
    {
        public int HomePhotoId { get; set; }
        public string HomePhotoPath { get; set; }
        public int  PhotoTypeId{ get; set; }
        public PhotoType PhotoType { get; set; }
    }
    /// <summary>
    /// Modello figlio con contenitore dei valori base64 delle foto da inviare al client
    /// </summary>
    public class HomePhotoValue : HomePhoto
    {
        public string PhotoTypeDescription { get; set; }
        public List<string> Base64Image { get; set; } = new List<string>();
    }

    public class HomePhotoPost
    {
        public HomePhoto HomePhoto { get; set; }
        public string HomePhotoBase64 { get; set; }
        public int PhotoTypeId { get; set; }
        public string PhotoName { get; set; }
    }

    public class HomePhotoPut
    {
        /// <summary>
        /// Nome della foto da rimpiazzare
        /// </summary>
        public string PhotoName { get; set; }
        /// <summary>
        /// base64 della nuova foto
        /// </summary>
        public string Base64NewPhoto { get; set; }
    }
}
