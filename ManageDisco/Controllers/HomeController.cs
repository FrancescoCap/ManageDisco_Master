using ManageDisco.Context;
using ManageDisco.Helper;
using ManageDisco.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ManageDisco.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : BaseController
    {
        public HomeController(DiscoContext db, IConfiguration configuration) : base(db, configuration)
        {
        }



        // GET: api/<HomeController>
        [HttpGet]
        [Route("Info")]
        public async Task<IActionResult> GetHome()
        {
            Home homeData = new Home();

            List<Task> tasks = new List<Task>()
            {
                new Task(() =>
                {
                    homeData.Events =  _db.Events
                        .Where(x => x.Date.CompareTo(DateTime.Today) == 0 || x.Date.CompareTo(DateTime.Today) > 0)
                        .Select(x => new EventPartyImages(){
                            Id = x.Id,
                            Name = x.Name,
                            Description = x.Description,
                            Date = x.Date
                        })
                        .OrderByDescending(x => x.Date).ThenBy(x => x.Name)
                        .ToList();

                    homeData.Events.ForEach(x =>
                    {
                        var photos = _db.EventPhoto
                        .Where(e => (e.EventPhotoEventId == x.Id && e.PhotoType.PhotoTypeDescription == EventPhotoDescriptionValues.EVENT_IMAGE_TYPE_COVER))
                        .OrderBy(e => e.PhotoType.PhotoTypeId)
                        .ToList();

                       photos.ForEach(f =>
                       {
                           string base64Value = Convert.ToBase64String(HelperMethods.GetBytesFromStream(
                               HelperMethods.GetFileStreamToFtp(f.EventPhotoImagePath, ftpUser, ftpPassword)));

                           x.LinkImage.Add(base64Value);
                       });
                    });
                }),
                new Task(() =>
                {
                    homeData.PhotoType = _db.PhotoType.ToList();
                }),
                new Task(() =>
                {
                    var photoType = _db.PhotoType.Where(x => x.PhotoTypeDescription.Contains("Home")).ToList();
                    homeData.HomePhoto = new List<HomePhotoValue>();

                    photoType.ForEach(pt => {

                        homeData.HomePhoto.AddRange(_db.HomePhoto
                        .Where(x => x.PhotoTypeId == pt.PhotoTypeId)
                        .Select(x => new HomePhotoValue(){
                          PhotoTypeDescription = x.PhotoType.PhotoTypeDescription,
                          PhotoType = x.PhotoType,
                          PhotoTypeId = x.PhotoTypeId,
                          HomePhotoPath = x.HomePhotoPath,
                          HomePhotoId = x.HomePhotoId
                        })
                        .OrderByDescending(x => x.PhotoTypeId)
                        //Prendo il nr. max di photo consentito dal tipo di galleria
                        .Take(pt.PhotoTypeMaxNumber)
                        .ToList());
                        /*
                         * VOGLIO AVERE UN'IMMAGINE DI DEFAULT DA RESTITUIRE NEL CASO IN CUI PER LA GALLERIA D'IMMAGINI DEL TIPO PROCESSATO
                         * CI SIANO MENO FOTO DEL LIMITE MASSIMO.
                         * NON VOLEVO SALVARE L'IMMAGINE DI DEFAULT A DB PERCHE' ALTRIMENTI ANDAVO A CONDIZIONARE LA VISUALIZZAZIONE DELLE FOTO NELLA HOME,
                         * QUINDI:
                         * 1. CONTROLLO SE PER IL TIPO PROCESSATO CI SONO TUTTE LE FOTO
                         * 2. NEL CASO DI ESITO FALSE VADO A MAPPARE DINAMICAMENTE A HOMEPHOTOPATH IL PERCORSO DELL'IMMAGINE DI DEFAULT. IL NORMALE PERCORSO DEL
                         * FLUSSO FARA' IL RESTO
                         */
                        bool photoAreCompleted = homeData.HomePhoto.Count == pt.PhotoTypeMaxNumber;
                        if (!photoAreCompleted)
                        {
                            bool photoForTypeExist = homeData.HomePhoto.Any(e => e.PhotoTypeId == pt.PhotoTypeId);
                            int photoDiff = pt.PhotoTypeMaxNumber - (photoForTypeExist ? homeData.HomePhoto.Count(c => c.PhotoType.PhotoTypeId == pt.PhotoTypeId) : 0);
                            for (int i = 0; i < photoDiff; i++)
                            {
                                homeData.HomePhoto.Add(new HomePhotoValue(){
                                    PhotoTypeId = pt.PhotoTypeId,
                                    PhotoTypeDescription = pt.PhotoTypeDescription,
                                    HomePhotoPath = $"{ftpAddress}/{HelperMethods.NO_IMAGE_PHOTONAME}"
                                });
                            }
                        }

                    });
                     homeData.HomePhoto.ForEach(x =>
                     {
                           var imgByte = HelperMethods.GetBytesFromStream(HelperMethods.GetFileStreamToFtp(x.HomePhotoPath, ftpUser, ftpPassword));
                           string base64Image = Convert.ToBase64String(imgByte);

                         x.Base64Image.Add(base64Image);
                         x.HomePhotoPath = x.HomePhotoPath.Split('/').Last();

                     });

                }),
                new Task(() =>
                {
                    //recuperare i contatti e raggrupparli per ContactType
                    var groupedContact = _db.Contact
                    .Include(i => i.ContactType)
                    .ToList()
                    .GroupBy(x => x.ContactType.ContactTypeDescription);

                    homeData.Contacts = new List<ContactGroup>();// new Dictionary<string, List<string>>();
                   foreach(var key in groupedContact)
                    {
                        ContactGroup contact = new ContactGroup();
                        contact.ContactTypeDescription = key.Key;
                        contact.ContactTypeId = key.First().ContactType.ContactTypeId;
                        contact.ContactsValues = new List<string>();
                        
                        foreach(var k in key)
                        {
                           contact.ContactsValues.Add(k.ContactDescription);
                        }
                        homeData.Contacts.Add(contact);
                    }

                }),
                new Task(() =>
                {
                    homeData.DiscoEntity = _db.DiscoEntity.FirstOrDefault();
                })
            };


            tasks.ForEach(t =>
            {
                t.Start();
                //attendo che il task termini l'esecuzione perchè se faccio partire subito anche l'altro si generare un errore
                //sull'utilizzo di una stessa istanza di tipo DbContext in due thread differenti
                t.Wait();
            });
            Task.WaitAll(tasks.ToArray());

            return Ok(homeData);
        }

        /// <summary>
        /// Api per l'upload delle foto nella home
        /// </summary>
        /// <param name="value"></param>
        [HttpPost]
        public async Task<IActionResult> UploadHomePhoto([FromBody] List<HomePhotoPost> photos)
        {
            if (photos == null || photos.Count == 0)
                return BadRequest();


            var photoTypesNames = _db.PhotoType.Where(x => x.PhotoTypeDescription.Contains(EventPhotoDescriptionValues.HOME_IMAGE_FILTER_LIKE));

            photos.ForEach(x =>
           {
               //se è un inserimento di una nuova foto devo sapere quante ne sono già presenti così continuare la numerazione e non sovrascrivere le vecchie
               int photoIndex = _db.HomePhoto.Count(i => i.PhotoType.PhotoTypeId == x.PhotoTypeId) + 1;
               var fileName = x.PhotoName;
               if (fileName == null || fileName == HelperMethods.NO_IMAGE_PHOTONAME)
               {
                   fileName = photoTypesNames.Where(t => t.PhotoTypeId == x.PhotoTypeId).FirstOrDefault().PhotoTypeDescription + "_" + photoIndex.ToString();
               }
               var fileExtension = "webp";
               //tolgo l'estensione per quando mi arriva il nome del file dal client, altrimenti l'upload su ftp viene errato
               fileName = fileName.Replace("." + fileExtension, "");
               var imgContent = x.HomePhotoBase64.Split(',').Last();

               /*
                * Se il file è già presente sull'ftp significa che è un aggiornamento dell'immagine e quindi non ho necesittà di scrivere
                * il riferimento al path nel db altrimenti le immagini verrebbero duplicate 
                */
               if (!HelperMethods.CheckFileFromFtp(ftpAddress, ftpUser, ftpPassword, fileName + "." + fileExtension))
               {
                   _db.HomePhoto.Add(new HomePhoto()
                   {
                       HomePhotoPath = $"{ftpAddress}/{fileName}.{fileExtension}",
                       PhotoTypeId = x.PhotoTypeId
                   });
               }

               HelperMethods.UploadFileToFtp(ftpAddress, ftpUser, ftpPassword, $"{fileName}.{fileExtension}", Convert.FromBase64String(imgContent));
               /*  Il salvataggio non lo faccio in asincrono perchè altrimenti parte l'upload della foto successiva generando l'eccezione
               *   sull'utilizzo di una stessa instaza DbContext su due thread differenti.
               *   Ho bisogno di salvare all'interno del ciclo perchè photoIndex deve essere sempre aggiornato con il count
               */
               _db.SaveChanges();

           });



            return Ok();
        }

        [HttpPut]
        [Route("UpdatePhoto")]
        public async Task<IActionResult> UpdateHomePhoto([FromBody] HomePhotoPut photo)
        {
            if (photo == null)
                return BadRequest();

            //siccome a db non devo cambiare il titolo perchè è sempre lo stesso, per aggiornare la foto devo solo cambiare la foto sull'ftp
            byte[] photoBytes = Convert.FromBase64String(photo.Base64NewPhoto);
            await HelperMethods.UploadFileToFtp(ftpAddress, ftpUser, ftpPassword, photo.PhotoName, photoBytes);

            return Ok();
        }


        /// <summary>
        /// Elimina la riga dal database con il riferimento alla foto. E' utile per la gestione di cambio di singole foto, altrimenti vengono mostrate doppie
        /// perchè la query recupera sempre le ultime 3 aggiunte
        /// </summary>
        /// <returns></returns>
        private async void DeletePhotoReference(string description)
        {
            await Task.Run(async () =>
            {
                HomePhoto homePhoto = await _db.HomePhoto.FirstOrDefaultAsync(x => x.HomePhotoPath == description);
                if (homePhoto == null)
                    return;

                _db.HomePhoto.Remove(homePhoto);
                await _db.SaveChangesAsync();
            });

        }
    }
}
