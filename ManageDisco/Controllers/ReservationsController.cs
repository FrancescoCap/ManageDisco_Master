using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ManageDisco.Context;
using ManageDisco.Model;
using ManageDisco.Helper;
using ManageDisco.Model.UserIdentity;
using Microsoft.AspNetCore.Authorization;

namespace ManageDisco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : BaseController
    {

        public ReservationsController(DiscoContext context) : base(context)
        {

        }

        // GET: api/Reservations
        [HttpGet]
        public async Task<IActionResult> GetReservation()
        {
            IQueryable<ReservationView> reservationViews = _db.Reservation
                .Select(x => new ReservationView()
                {
                    ReservationId = x.ReservationId,
                    ReservationCode = x.ReservationCode,
                    ReservationDate = x.ReservationDate,
                    EventId = x.EventPartyId,
                    EventName = x.EventParty.Name,
                    ReservationPeopleCount = x.ReservationPeopleCount,
                    ReservationTypeValue = x.ReservationType.ReservationTypeString,
                    ReservationUserCode = x.ReservationUserCodeValue,
                    ReservationExpectedBudget = x.ReservationExpectedBudget,
                    ReservationRealBudget = x.ReservationRealBudget,
                    ReservationStatusId = x.ReservationStatus.ReservationStatusId,
                    ReservationStatus = x.ReservationStatus.ReservationStatusValue,
                    UserId = x.UserId,
                    CanAcceptReservation = x.ReservationStatusId != ReservationStatusValue.RESERVATIONSTATUS_REJECTED && x.ReservationStatusId != ReservationStatusValue.RESERVATIONSTATUS_APPROVED &&
                            _user.Roles.Contains(RolesConstants.ROLE_ADMINISTRATOR), //E' concettualmente sbagliato bloccare la funzionalità da qui. Dovrebbe essere un attributo a livello Utente
                    CanAcceptBudget = x.ReservationStatusId == ReservationStatusValue.RESERVATIONSTATUS_APPROVED &&
                            DateTime.Compare(x.EventParty.Date, DateTime.Today) > 0,
                    ReservationName = x.ReservationTableName,
                    TableId = x.TableId != null ? x.TableId : 0
                });


            if (_user.Roles.Any(x => x.Contains(RolesConstants.ROLE_PR)))
            {
                //Se sono un PR allora visualizzo tutte le prenotazioni fatte con il mio codice                
                reservationViews = reservationViews.Where(x => x.ReservationUserCode == _user.UserCode);

            }
            else if (_user.Roles.Any(x => x.Contains(RolesConstants.ROLE_CUSTOMER)))
            {
                //Se sono un cliente visualizzo le prenotazione associate al mio Id utente
                reservationViews = reservationViews.Where(x => x.UserId == _user.Id);
            }


            return Ok(await reservationViews.ToListAsync());
        }

        // GET: api/Reservations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Reservation>> GetReservation(int id)
        {
            var reservation = await _db.Reservation.FindAsync(id);

            if (reservation == null)
            {
                return NotFound();
            }

            return reservation;
        }
        /// <summary>
        /// Get reservations for specific user (PR)
        /// </summary>
        /// <param name="prCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("User")]
        public async Task<IActionResult> GetUserReservation([FromQuery] string prCode)
        {
            if (prCode == string.Empty)
                return BadRequest();

            /* ReservationUserCode user = await _db.ReservationUserCode.FirstOrDefaultAsync(x => x.ReservationUserCodeValue == prCode);
             if (user == null)
                 return NotFound();

             List<ReservationView> userReservations = await _db.Reservation
                 .Where(x => x.ReservationUserCode.ReservationUserCodeValue == prCode)
                 .Select(x => new ReservationView()
                 {
                     ReservationId = x.ReservationId,
                     ReservationCode = x.ReservationCode,
                     ReservationTypeValue = x.ReservationType.ReservationTypeString,
                     ReservationDate = x.ReservationDate,
                     ReservationPeopleCount = x.ReservationPeopleCount,
                     ReservationUserCode = x.ReservationUserCode.ReservationUserCodeValue,
                     ReservationStatus = x.ReservationStatus.ReservationStatusValue,
                     EventName = x.EventParty.Name

                 }).ToListAsync();*/


            return Ok();
        }

        /// <summary>
        /// Api per la restituzione dei collaboratori con il rispettivo andamento lavorativo
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("UserReservations")]
        [Authorize(Roles = RolesConstants.ROLE_ADMINISTRATOR + "," + RolesConstants.ROLE_PR)]
        public async Task<IActionResult> GetCollaborators()
        {
            IQueryable<ReservationPayment> payments = _db.Reservation
                 .Select(x => new ReservationPayment()
                 {
                     User = x.User,
                     UserId = x.UserId
                 });



            if (_user.Roles.Any(x => x == RolesConstants.ROLE_PR))
            {
                //string prCode = _db.ReservationUserCode.FirstOrDefault(x => x.UserId == _user.Id).ReservationUserCodeValue;
                payments.Where(x => x.UserId == _user.Id);
            }

            var finalPayments = payments.ToList();

            return Ok(payments.ToList().GroupBy(x => x.User.Name));
        }

        [HttpGet]
        [Route("Type")]
        public async Task<IActionResult> GetReservationType()
        {
            List<ReservationType> reservationTypes = await _db.ReservationType.ToListAsync();

            return Ok(reservationTypes);
        }

        /// <summary>
        /// Api utilizzata per filtrare le prenotazioni per evento, ma anche per stato prenotazione
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="resStatus"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Filter/Event")]
        public async Task<IActionResult> GetReservationsFromEvent([FromQuery] int eventId, [FromQuery] int resStatus)
        {
            if (eventId <= 0)
                return NotFound("");
            ReservationViewTable reservationViewTable = new ReservationViewTable();

            reservationViewTable.Reservations = _db.Reservation.Where(x => x.EventPartyId == eventId)
                .Select(x => new ReservationView()
                {
                    ReservationId = x.ReservationId,
                    ReservationCode = x.ReservationCode,
                    ReservationDate = x.ReservationDate,
                    EventId = x.EventParty.Id,
                    EventName = x.EventParty.Name,
                    ReservationPeopleCount = x.ReservationPeopleCount,
                    ReservationTypeValue = x.ReservationType.ReservationTypeString,
                    ReservationUserCode = x.ReservationUserCodeValue,
                    ReservationExpectedBudget = x.ReservationExpectedBudget,
                    ReservationRealBudget = x.ReservationRealBudget,
                    ReservationStatusId = x.ReservationStatus.ReservationStatusId,
                    ReservationStatus = x.ReservationStatus.ReservationStatusValue,
                    ReservationTablAssigned = x.Table.TableNumber,
                    UserId = x.UserId,
                    CanAcceptReservation = x.ReservationStatusId != ReservationStatusValue.RESERVATIONSTATUS_REJECTED && x.ReservationStatusId != ReservationStatusValue.RESERVATIONSTATUS_APPROVED &&
                            _user.Roles.Contains(RolesConstants.ROLE_ADMINISTRATOR), //E' concettualmente sbagliato bloccare la funzionalità da qui. Dovrebbe essere un attributo a livello Utente
                    CanAcceptBudget = x.ReservationStatusId != ReservationStatusValue.RESERVATIONSTATUS_REJECTED &&
                            DateTime.Compare(x.EventParty.Date, DateTime.Today) > 0,
                    ReservationName = x.ReservationTableName,
                    TableId = x.TableId != 0 ? x.TableId.Value : 0
                });
           

            if (resStatus > 0)
                reservationViewTable.Reservations = reservationViewTable.Reservations.Where(x => x.ReservationStatusId == resStatus);

            if (_user.Roles.Contains(RolesConstants.ROLE_PR) || _user.Roles.Contains(RolesConstants.ROLE_ADMINISTRATOR))
            {
                reservationViewTable.Reservations = reservationViewTable.Reservations.Where(x => x.EventId == eventId);
                //Se è un pr visualizzo solo le sue prenotazioni per quell'evento
                if (_user.Roles.Contains(RolesConstants.ROLE_PR))
                    reservationViewTable.Reservations = reservationViewTable.Reservations.Where(p => p.UserId == _user.Id);
            }
            reservationViewTable.CanAssignTable = HelperMethods.UserIsAdministrator(_user);


            return Ok(reservationViewTable);
        }

        // PUT: api/Reservations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        [Route("Edit")]
        public async Task<IActionResult> PutReservation([FromQuery] int reservationId, [FromBody] ReservationPost reservation)
        {

            if (reservation == null || reservationId < 1 || reservation.EventPartyId < 1)
                return BadRequest();

            Reservation reservationOld = await _db.Reservation.FirstOrDefaultAsync(x => x.ReservationId == reservationId && x.EventPartyId == reservation.EventPartyId);
            if (reservationOld == null)
                return NotFound();

            reservationOld.EventPartyId = reservation.EventPartyId;
            reservationOld.ReservationPeopleCount = reservation.ReservationPeopleCount;
            reservationOld.ReservationTypeId = reservation.ReservationType;


            _db.Entry(reservationOld).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            return NoContent();
        }
        [HttpPut]
        public async Task<IActionResult> HandleReservation([FromQuery] int reservationId, [FromQuery] int status)
        {
            if (reservationId == 0)
                return BadRequest("Invalid reservationId");

            Reservation reservation = await _db.Reservation.FirstOrDefaultAsync(x => x.ReservationId == reservationId);
            if (reservation != null)
            {
                reservation.ReservationStatusId = status;
                _db.Entry(reservation).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }


            return NoContent();
        }

        [HttpPut]
        [Route("Budget/Confirm")]
        public async Task<IActionResult> ConfirmBudget([FromQuery] int reservationId, [FromQuery] int euro)
        {
            if (reservationId == 0)
                return BadRequest("Invalid reservation Id");
            Reservation reservation = await _db.Reservation.FirstOrDefaultAsync(x => x.ReservationId == reservationId);

            if (reservation == null)
                return NotFound("No reservation found");

            reservation.ReservationRealBudget = euro;
            _db.Entry(reservation).State = EntityState.Modified;

            //budget confermato quindi aumento il credito del dipendente nei confronti del locale
            //ReservationPayment reservationPayment = new ReservationPayment();       
            ///*I RESTANTI CAMPI SONO VALORIZZATI DAL TRIGGER*/
            //reservationPayment.UserId = reservation.UserId;
            //_db.ReservationPayment.Add(reservationPayment);

            await _db.SaveChangesAsync();

            return Ok();
        }

        // POST: api/Reservations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> PostReservation([FromBody] ReservationPost reservationData)
        {
            if (reservationData == null)
                return BadRequest();

            if (reservationData.ReservationUserCodeValue == string.Empty)
                return Ok(new ReservationResponse() { Message = "Missing PR code." });

            if (reservationData.EventPartyId < 1)
                return Ok(new ReservationResponse() { Message = "Missing event reference for reservation." });

            if (reservationData.ReservationType < 1)
                return Ok(new ReservationResponse() { Message = "Missing reservation type." });

            ReservationUserCode reservationUserCodeFound = await _db.ReservationUserCode.FirstOrDefaultAsync(x => x.ReservationUserCodeValue == 
                    (HelperMethods.UserIsPrOrAdministrator(_user) ? _user.UserCode : reservationData.ReservationUserCodeValue));

            if (reservationUserCodeFound == null)
                return Ok(new ReservationResponse() { Message = "Invalid reservation code." });

            //Check if user already have reservation for the event           
            //if (ReservationExists(reservationData.EventPartyId))
            //    return Ok(new ReservationResponse() { Message = "You already have reservation. Please edit it." });

            Reservation reservation = new Reservation()
            {
                ReservationCode = HelperMethods.GenerateRandomString(6),
                ReservationTypeId = reservationData.ReservationType,
                ReservationUserCodeValue = reservationData.ReservationUserCodeValue,
                ReservationPeopleCount = reservationData.ReservationPeopleCount,
                EventPartyId = reservationData.EventPartyId,
                ReservationExpectedBudget = reservationData.ReservationExpectedBudget,
                ReservationRealBudget = reservationData.ReservationRealBudget,
                ReservationStatusId = ReservationStatusValue.RESERVATIONSTATUS_PENDING,
                ReservationConfirmationNote = "",
                ReservationNote = reservationData.ReservationNote,
                ReservationTableName = reservationData.ReservationName
            };

            reservation.UserIdOwner = HelperMethods.UserIsPrOrAdministrator(_user) ? reservationData.ReservationOwnerId : _user.Id;
            reservation.UserId = HelperMethods.UserIdCustomer(_user) ? _db.ReservationUserCode.FirstOrDefault(x => x.ReservationUserCodeValue == reservationData.ReservationUserCodeValue).UserId :
                    _user.Id;

            if ((reservation.ReservationTypeId == 2 ||
                reservation.ReservationTypeId == 3) && reservation.ReservationExpectedBudget == 0)
                return Ok(new ReservationResponse() { Message = "For this prenotation type previsional budget is required." });


            _db.Reservation.Add(reservation);
            await _db.SaveChangesAsync();

            return Ok(new ReservationResponse() { ReservationCode = reservation.ReservationCode, Message = "Your reservation was successful inserted. Please save your reservation code." });
        }

        [HttpGet("Event")]
        public async Task<IActionResult> GetEventReservation([FromQuery] int eventId)
        {

            if (eventId <= 0)
                return BadRequest("Invalid event.");

            EventParty eventParty = await _db.Events.FirstOrDefaultAsync(x => x.Id == eventId);
            if (eventParty == null)
                return NotFound("Event not found.");

            Reservation reservations = await _db.Reservation
                .Where(x => x.UserIdOwner == _user.Id && x.EventPartyId == eventParty.Id)
                //.Select(x => new Reservation()
                //{
                //    EventId = x.EventPartyId,
                //    ReservationExpectedBudget = x.ReservationExpectedBudget,
                //    ReservationPeopleCount = x.ReservationPeopleCount,
                //    ReservationTy
                //})
                .FirstOrDefaultAsync();


            return Ok(reservations);
        }

        // DELETE: api/Reservations/5
        /* [HttpDelete("{id}")]
         public async Task<IActionResult> DeleteReservation(int id)
         {
             var reservation = await _db.Reservation.FindAsync(id);
             if (reservation == null)
             {
                 return NotFound();
             }

             _db.Reservation.Remove(reservation);
             await _db.SaveChangesAsync();

             return NoContent();
         }*/

        private bool ReservationExists(int eventId)
        {
            return _db.Reservation.Any(x => x.UserId == _user.Id && x.EventPartyId == eventId && x.ReservationStatusId == ReservationStatusValue.RESERVATIONSTATUS_APPROVED);
        }
    }
}
