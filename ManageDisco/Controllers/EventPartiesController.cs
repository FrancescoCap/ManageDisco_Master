using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ManageDisco.Context;
using ManageDisco.Model;
using ManageDisco.Model.UserIdentity;
using ManageDisco.Helper;
using System.Net.FtpClient;
using System.Net;
using System.IO;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ManageDisco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventPartiesController : BaseController
    {
        public EventPartiesController(DiscoContext db,
            IConfiguration configuration,
            UserManager<User> userManager) : base(db, configuration, userManager)
        {
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("General")]
        public async Task<ActionResult<EventPartyOverview>> GetEventsGeneral([FromQuery] bool WithReservation)
        {
            List<EventPartyList> events = await _db.Events
                .Where(x => x.Date.Year == DateTime.Today.Year)
                .Select(x => new EventPartyList()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    MaxAge = x.MaxAge,
                    Date = x.Date,
                    //LinkImage = x.LinkImage,
                    EventPartyStatusDescription = x.Date.CompareTo(DateTime.Today) > 0 ?
                                    EventStatusConstants.STATUS_SCHEDULED : x.Date.CompareTo(DateTime.Today) == 0 ?
                                        EventStatusConstants.STATUS_ONGOING : x.Date.Year == DateTime.Today.Year - 100 ? EventStatusConstants.STATUS_CANCELLED : EventStatusConstants.STATUS_END,
                    UserHasReservation = false
                }).OrderByDescending(x => x.Date).ToListAsync();


            events.ForEach(x =>
            {
                var address = _db.EventPhoto.FirstOrDefault(p => p.EventPhotoEventId == x.Id).EventPhotoImagePath;
                string base64Value = Convert.ToBase64String(HelperMethods.GetBytesFromStream(HelperMethods.GetFileStreamToFtp(address, ftpUser, ftpPassword)));
                x.ImagePreview = base64Value;

            });



            EventPartyOverview partyOverview = new EventPartyOverview();
            partyOverview.Events = events;
            partyOverview.UserCanAddEvent = false;
            partyOverview.UserCanAddReservation = true;
            partyOverview.UserCanDeleteEvent = false;

            return Ok(partyOverview);
        }

        
       
        [HttpGet]
        public async Task<ActionResult<EventPartyOverview>> GetEvents([FromQuery]bool loadImages, [FromQuery] bool withReservations)
        {
            List<EventPartyList> events = await _db.Events
                .Where(x => x.Date.Year == DateTime.Today.Year)
                .Select(x => new EventPartyList()
                {                    
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    MaxAge = x.MaxAge,
                    Date = x.Date,
                    //LinkImage = x.LinkImage,
                    EventPartyStatusDescription = x.Date.CompareTo(DateTime.Today) > 0 ? 
                                    EventStatusConstants.STATUS_SCHEDULED : x.Date.CompareTo(DateTime.Today) == 0 ? 
                                        EventStatusConstants.STATUS_ONGOING : x.Date.Year == DateTime.Today.Year -100 ? EventStatusConstants.STATUS_CANCELLED : EventStatusConstants.STATUS_END,
                    UserHasReservation =  _db.Reservation.Any(r => r.EventPartyId == x.Id && r.UserIdOwner == _user.Id)
                }).OrderByDescending(x => x.Date).ToListAsync();

            if (loadImages)
            {
                events.ForEach(x =>
                {
                    var foundEvent = _db.EventPhoto.FirstOrDefault(p => p.EventPhotoEventId == x.Id);                    
                    
                    string base64Value = foundEvent != null ? 
                        Convert.ToBase64String(HelperMethods.GetBytesFromStream(HelperMethods.GetFileStreamToFtp(foundEvent.EventPhotoImagePath, ftpUser, ftpPassword))) : 
                        HelperMethods.GetBase64DefaultNoImage(ftpAddress, ftpUser, ftpPassword);

                    x.ImagePreview = base64Value;
                });
            }          

            EventPartyOverview partyOverview = new EventPartyOverview();
            partyOverview.Events = events;
            partyOverview.UserCanAddEvent = HelperMethods.UserIsAdministrator(_user) || _user.UserCanHandleEvents;
            partyOverview.UserCanAddReservation = HelperMethods.UserIsCustomer(_user);
            partyOverview.UserIsInStaff = HelperMethods.UserIsInStaff(_user);
            partyOverview.UserCanDeleteEvent = HelperMethods.UserIsAdministrator(_user) || _user.UserCanHandleEvents;

            if (withReservations)
            {
                partyOverview.EventReservations = new List<ReservationView>();
                partyOverview.Events.ForEach(async x =>
                {
                    var reservations = _db.Reservation
                        .Where(r => r.EventPartyId == x.Id)
                        .Select(r => new ReservationView() {
                            ReservationId = r.ReservationId,
                            ReservationCode = r.ReservationCode,
                            ReservationDate = r.ReservationDate,
                            EventId = r.EventPartyId,
                            EventName = r.EventParty.Name,
                            ReservationPeopleCount = r.ReservationPeopleCount,
                            ReservationTypeValue = r.ReservationType.ReservationTypeString,
                            ReservationUserCode = r.ReservationUserCodeValue,
                            ReservationExpectedBudget = r.ReservationExpectedBudget,
                            ReservationRealBudget = r.ReservationRealBudget,
                            ReservationStatusId = r.ReservationStatus.ReservationStatusId,
                            ReservationStatus = r.ReservationStatus.ReservationStatusValue,
                            UserId = r.UserId,
                            UserIdOwner = r.UserIdOwner,
                            CanAcceptReservation = r.ReservationStatusId != ReservationStatusValue.RESERVATIONSTATUS_REJECTED && r.ReservationStatusId != ReservationStatusValue.RESERVATIONSTATUS_APPROVED &&
                            HelperMethods.UserIsAdministrator(_user), //E' concettualmente sbagliato bloccare la funzionalità da qui. Dovrebbe essere un attributo a livello Utente
                            IsReservationEditable = r.ReservationStatusId == ReservationStatusValue.RESERVATIONSTATUS_APPROVED && HelperMethods.UserIsAdministrator(_user),
                            CanAcceptBudget = r.ReservationStatusId == ReservationStatusValue.RESERVATIONSTATUS_APPROVED &&
                            DateTime.Compare(r.EventParty.Date, DateTime.Today) > 0 && HelperMethods.UserIsInStaff(_user) && r.ReservationRealBudget == 0,
                            ReservationName = r.ReservationTableName,
                            TableId = r.TableId != null ? r.TableId : 0,
                            ReservationTablAssigned = $"{r.Table.TableAreaDescription} - {r.Table.TableNumber}"

                        }).ToList();

                    partyOverview.EventReservations.AddRange(reservations);
                });
            }

            return Ok(partyOverview);
        }

        [AllowAnonymous]
        [HttpGet("Details/General")]
        public async Task<ActionResult<EventParty>> GetEventPartyGeneral([FromQuery] int eventId)
        {
            var eventParty = await _db.Events
                .Where(x => x.Id == eventId)
                .Select(x => new EventPartyDetail()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Date = x.Date,
                    MaxAge = x.MaxAge,
                    EntrancePrice = x.EntrancePrice,
                    FreeEntranceDescription = x.FreeEntranceDescription,
                    TablePrice = x.TablePrice,
                    EventIsEnd = x.Date.CompareTo(DateTime.Today) < 0,  //la data dell'evento NON è precedente alla data odierna
                    UserCanEditInfo = false
                }).FirstOrDefaultAsync();

            var eventImgs = await _db.EventPhoto.Where(x => x.EventPhotoEventId == eventId &&
                    x.PhotoType.PhotoTypeDescription.Contains(EventPhotoDescriptionValues.EVENT_IMAGE_TYPE_EVENT_DETAIL)).ToListAsync();

            if (eventImgs != null && eventImgs.Count > 0)
            {
                eventImgs.ForEach(x =>
                {
                    var imgBytes = HelperMethods.GetBytesFromStream(HelperMethods.GetFileStreamToFtp(x.EventPhotoImagePath, ftpUser, ftpPassword));
                    eventParty.LinkImage.Add(Convert.ToBase64String(imgBytes));
                });

            }

            if (eventParty == null)
            {
                return NotFound();
            }

            return Ok(eventParty);
        }

        // GET: api/EventParties/5
        [HttpGet("Details")]
        public async Task<ActionResult<EventParty>> GetEventParty([FromQuery]int eventId)
        {
            var eventParty = await _db.Events
                .Where(x => x.Id == eventId)
                .Select(x => new EventPartyDetail() {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Date = x.Date,
                    MaxAge = x.MaxAge,
                    EntrancePrice = x.EntrancePrice,
                    FreeEntranceDescription = x.FreeEntranceDescription,
                    TablePrice = x.TablePrice,
                    UserCanEditInfo = HelperMethods.UserIsAdministrator(_user) || _user.UserCanHandleEvents,
                    UserIsInStaff = HelperMethods.UserIsAdministrator(_user) || HelperMethods.UserIsInStaff(_user),
                    EventIsEnd = x.Date.CompareTo(DateTime.Today) < 0,  //la data dell'evento è precedente alla data odierna 
                    UserCanEnrollFreeEntrance = _user.Gender == GenderCostants.GENDER_FEMALE,
                    FreeEntranceEnabled = x.FreeEntranceEnabled,
                    EventPartyStatusDescription = x.Date.CompareTo(DateTime.Today) > 0 ?
                                    EventStatusConstants.STATUS_SCHEDULED : x.Date.CompareTo(DateTime.Today) == 0 ?
                                        EventStatusConstants.STATUS_ONGOING : x.Date.Year == DateTime.Today.Year - 100 ? EventStatusConstants.STATUS_CANCELLED : EventStatusConstants.STATUS_END
                }).FirstOrDefaultAsync();

            var eventImgs = await _db.EventPhoto.Where(x => x.EventPhotoEventId == eventId && 
                    x.PhotoType.PhotoTypeDescription.Contains(EventPhotoDescriptionValues.EVENT_IMAGE_TYPE_EVENT_DETAIL)).ToListAsync();

            if (eventParty == null)
            {
                return NotFound();
            }

            if (eventImgs != null && eventImgs.Count > 0)
            {
                eventImgs.ForEach(x =>
                {
                    var imgBytes = HelperMethods.GetBytesFromStream(HelperMethods.GetFileStreamToFtp(x.EventPhotoImagePath, ftpUser, ftpPassword));
                    eventParty.LinkImage.Add(Convert.ToBase64String(imgBytes));
                });

            }
            else
            {
                eventParty.LinkImage.Add(HelperMethods.GetBase64DefaultNoImage(ftpAddress, ftpUser, ftpPassword));
            }           

            return Ok(eventParty);
        }

        // POST: api/EventParties
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<EventParty>> PostEventParty([FromBody]EventPartyImages eventParty)
        {
            if (eventParty == null)
                return BadRequest();

            if (eventParty.FreeEntranceDescription == String.Empty)
                eventParty.FreeEntranceDescription = "-";
            if (String.IsNullOrEmpty(eventParty.Name))
                return BadRequest(new GeneralReponse() { OperationSuccess = false, Message = "Inserire un nome per l'evento." });
            if (eventParty.Date == null)
                return BadRequest(new GeneralReponse() { OperationSuccess = false, Message = "Inserire una data per l'evento." });
            if (eventParty.EntrancePrice == 0 || eventParty.TablePrice == 0)
                return BadRequest(new GeneralReponse() { OperationSuccess = false, Message = "Inserire i prezzi per l'evento." });

            _db.Entry(eventParty).State = EntityState.Added;
            await _db.SaveChangesAsync();
                       

            int photoIndex = 1;
            int photoTypeIndex = 0;
            //get EventPhotoType to assign to EvenPhoto
            var eventPhotoType = _db.PhotoType.Where(x => x.PhotoTypeDescription.Contains(EventPhotoDescriptionValues.EVENT_IMAGE_FILTER_LIKE)).ToList();

            foreach (string s in eventParty.LinkImage)
            {
                //Per convertire il formato base64 in immagine devo prelevare solo quello che trovo dopo l'header e quindi dopo l'ultima virgola
                var imgContent = s.Split(',').Last();
                string fileName = photoIndex.ToString() + "_" + eventParty.Id;
                string fileExtension = "webp";
                await HelperMethods.UploadFileToFtp(ftpAddress, ftpUser, ftpPassword, $"{fileName}.{fileExtension}", Convert.FromBase64String(imgContent));
                //Devo gestire l'index dell'array del tipo di foto. Se entro qui vuol dire che devo inserire le foto del dettaglio
                if (photoTypeIndex > eventPhotoType.Count - 1)
                    photoTypeIndex = eventPhotoType.Count-1;

                _db.EventPhoto.Add(new EventPhoto()
                {
                    EventPhotoEventId = eventParty.Id,
                    EventPhotoImagePath = $"{ftpAddress}/{fileName}.{fileExtension}",
                    PhotoTypeId = eventPhotoType[photoTypeIndex].PhotoTypeId
                });

                photoIndex++;
                photoTypeIndex++;
            }
            //Faccio due saveChanges perchè le foto devono essere caricate solo nel momento in cui l'inserimento dei dati dell'evento è stato effettuato
            await _db.SaveChangesAsync();
                   

            return Ok();
        }
        /// <summary>
        /// API cancellazione evento (inteso nella programmazione, non dalla lista)
        /// </summary>
        /// <returns></returns>
        [HttpPut("Cancel")]
        public async Task<IActionResult> CancelEvent([FromQuery] int eventId)
        {
            if (eventId <= 0)
                return BadRequest("Invalid event id.");

            EventParty ev = await _db.Events.FirstOrDefaultAsync(x => x.Id == eventId);
            if (ev == null)
                return NotFound();

            ev.Date = DateTime.Today.AddYears(-100);
            await _db.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Roles = RolesConstants.ROLE_ADMINISTRATOR)]
        [HttpPut]
        [Route("FreeEntrance")]
        public async Task<IActionResult> ChangeFreeEntranceStatus([FromQuery]int eventId)
        {
            if (eventId == 0)
                return BadRequest(new GeneralReponse() { OperationSuccess = false, Message = "Evento non valido." });

            EventParty eventParty = await _db.Events.FirstOrDefaultAsync(x => x.Id == eventId);
            if (eventParty == null)
                return BadRequest("Evento non valido.");

            eventParty.FreeEntranceEnabled = !eventParty.FreeEntranceEnabled;

            _db.Entry(eventParty).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("Details/Edit")]
        public async Task<IActionResult> EditPrice([FromQuery]int eventId, [FromBody] EventPartyPutModel price)
        {
            if (eventId <= 0)
                return BadRequest("Invalid id for event.");
            if (price == null)
                return BadRequest("Invalid model data.");

            EventParty evtParty = await _db.Events.FirstOrDefaultAsync(x => x.Id == eventId);
            if (evtParty == null)
                return NotFound("Event not found.");

            evtParty.EntrancePrice = price.EntrancePrice;
            evtParty.FreeEntranceDescription = price.FreeEntranceDescription;
            evtParty.TablePrice = price.TablePrice;
            evtParty.Description = price.Description;

            _db.Entry(evtParty).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return Ok();
        }

        // PUT: api/EventParties/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /*[HttpPut("{id}")]
        public async Task<IActionResult> PutEventParty(int id, EventParty eventParty)
        {
            if (id != eventParty.Id)
            {
                return BadRequest();
            }

            _context.Entry(eventParty).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventPartyExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        */

        // DELETE: api/EventParties/5
         [HttpDelete]
         public async Task<IActionResult> DeleteEventParty([FromQuery] int eventId)
         {
            if (eventId == 0)
                return BadRequest();

            EventParty eventParty = await _db.Events.FirstOrDefaultAsync(x => x.Id == eventId);
            if (eventParty == null)
                return NotFound("Event not found");

             _db.Events.Remove(eventParty);
             await _db.SaveChangesAsync();

             return NoContent();
         }

        private bool EventPartyExists(int id)
        {
            return _db.Events.Any(e => e.Id == id);
        }
    }
}
