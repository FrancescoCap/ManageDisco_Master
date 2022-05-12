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
        public async Task<IActionResult> GetReservation([FromQuery] int eventId, [FromQuery]int id)
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
                    UserIdOwner = x.UserIdOwner,
                    CanAcceptReservation = x.ReservationStatusId != ReservationStatusValue.RESERVATIONSTATUS_REJECTED && x.ReservationStatusId != ReservationStatusValue.RESERVATIONSTATUS_APPROVED &&
                            HelperMethods.UserIsAdministrator(_user), //E' concettualmente sbagliato bloccare la funzionalità da qui. Dovrebbe essere un attributo a livello Utente
                    CanAcceptBudget = x.ReservationStatusId == ReservationStatusValue.RESERVATIONSTATUS_APPROVED &&
                            DateTime.Compare(x.EventParty.Date, DateTime.Today) > 0 && HelperMethods.UserIsInStaff(_user) && x.ReservationRealBudget == 0,
                    ReservationName = x.ReservationTableName,
                    TableId = x.TableId != null ? x.TableId : 0,
                    ReservationTablAssigned = $"{x.Table.TableAreaDescription} - {x.Table.TableNumber}"
                });
            

            if (_user.Roles.Any(x => x.Contains(RolesConstants.ROLE_PR)))
            {
                //Se sono un PR allora visualizzo tutte le prenotazioni fatte con il mio codice                
                reservationViews = reservationViews.Where(x => x.ReservationUserCode == _user.UserCode);

            }
            else if (_user.Roles.Any(x => x.Contains(RolesConstants.ROLE_CUSTOMER)))
            {
                //Se sono un cliente visualizzo le prenotazione associate al mio Id utente
                reservationViews = reservationViews.Where(x => x.UserIdOwner == _user.Id);
            }

            if (eventId != 0)
                reservationViews = reservationViews.Where(x => x.EventId == eventId);
            if (id > 0)
                reservationViews = reservationViews.Where(x => x.ReservationId == id);

            return Ok(await reservationViews.ToListAsync());
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
                    CanAcceptReservation = x.ReservationStatusId == ReservationStatusValue.RESERVATIONSTATUS_PENDING && _user.Roles.Contains(RolesConstants.ROLE_ADMINISTRATOR), 
                    //E' concettualmente sbagliato bloccare la funzionalità da qui. Dovrebbe essere un attributo a livello Utente
                    CanAcceptBudget = x.ReservationStatusId == ReservationStatusValue.RESERVATIONSTATUS_APPROVED &&
                            DateTime.Compare(x.EventParty.Date, DateTime.Today) > 0,
                    ReservationName = x.ReservationTableName,
                    TableId = x.TableId != 0 ? x.TableId.Value : 0
                });
           

            if (resStatus > 0)
                reservationViewTable.Reservations = reservationViewTable.Reservations.Where(x => x.ReservationStatusId == resStatus);

            if (HelperMethods.UserIsPrOrAdministrator(_user))
            {
                reservationViewTable.Reservations = reservationViewTable.Reservations.Where(x => x.EventId == eventId);
                //Se è un pr visualizzo solo le sue prenotazioni per quel pr
                if (HelperMethods.UserIsPr(_user))
                    reservationViewTable.Reservations = reservationViewTable.Reservations.Where(p => p.UserId == _user.Id);
            }
            reservationViewTable.CanAssignTable = HelperMethods.UserIsAdministrator(_user);


            return Ok(reservationViewTable);
        }

        // PUT: api/Reservations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        [Route("Edit")]
        public async Task<IActionResult> PutReservation([FromBody] ReservationPost reservation)
        {

            if (reservation == null || reservation.ReservationId < 1)
                return BadRequest();

            Reservation reservationOld = await _db.Reservation.FirstOrDefaultAsync(x => x.ReservationId == reservation.ReservationId);
            if (reservationOld == null)
                return NotFound();

            if (reservation.EventPartyId != 0)
                reservationOld.EventPartyId = reservation.EventPartyId;
            if (reservation.ReservationType != 0)
                reservationOld.ReservationTypeId = reservation.ReservationType;

            reservationOld.ReservationTableName = reservation.ReservationName;
            reservationOld.ReservationExpectedBudget = reservation.ReservationExpectedBudget;
            reservationOld.ReservationRealBudget = reservation.ReservationRealBudget;
            reservationOld.ReservationPeopleCount = reservation.ReservationPeopleCount;
           


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
                return BadRequest(new GeneralReponse() { Message = "Id prenotazione non valido.", OperationSuccess = false });
            Reservation reservation = await _db.Reservation.FirstOrDefaultAsync(x => x.ReservationId == reservationId);

            if (reservation == null)
                return NotFound(new GeneralReponse() { Message = "Non è stata trovata nessuna prenotazione con questo identificativo.", OperationSuccess = false});

            if (reservation.TableId == null)
                return BadRequest(new GeneralReponse() { Message = "Per la conferma del budget il tavolo deve essere prima assegnato.", OperationSuccess = false} );

            reservation.ReservationRealBudget = euro;
            _db.Entry(reservation).State = EntityState.Modified;

            /**************** 
             * budget confermato quindi aumento il credito del dipendente nei confronti del locale
             * Tutta la gestione dei dati è fatta tramite trigger
             ***************/          

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
                return BadRequest(new GeneralReponse() { OperationSuccess = false, Message = "Codice PR mancante." });

            if (reservationData.EventPartyId < 1)
                return BadRequest(new ReservationResponse() { Message = "Selezionare un evento." });

            if (reservationData.ReservationType < 1)
                return BadRequest(new ReservationResponse() { Message = "Selezionare un tipo di prenotazione." });

            if (String.IsNullOrEmpty(reservationData.ReservationName))
                return BadRequest(new ReservationResponse() { Message = "Indicare un nome per la prenotazione." });

            if (reservationData.ReservationPeopleCount < 1)
                return BadRequest(new ReservationResponse() { Message = "Indicare un numero di persone." });

            if (reservationData.ReservationExpectedBudget < 1)
                return BadRequest(new ReservationResponse() { Message = "Indicare un budget marginale di spesa." });

            if (reservationData.ReservationUserCodeValue == null)
            {
                reservationData.ReservationUserCodeValue = HelperMethods.UserIsPrOrAdministrator(_user) ? _user.UserCode : reservationData.ReservationUserCodeValue;
            }

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
            //Id del pr
            reservation.UserId = HelperMethods.UserIsPrOrAdministrator(_user) ? _user.Id : 
                _db.PrCustomer.FirstOrDefaultAsync(x => x.PrCustomerCustomerid == _user.Id).Result.PrCustomerPrId;

            if ((reservation.ReservationTypeId == 2 ||
                reservation.ReservationTypeId == 3) && reservation.ReservationExpectedBudget == 0)
                return BadRequest(new GeneralReponse() { Message = "Per questo tipo di prenotazione è richiesto un'aspettativa di budget", OperationSuccess = false });

            string confirmMessage = "La tua prenotazione è stata accettata.";

            Reservation alreadyPresentTableReservation = await _db.Reservation.FirstOrDefaultAsync(x => x.EventPartyId == reservationData.EventPartyId && x.TableId == reservationData.TableId);
            if (alreadyPresentTableReservation != null)
            {
                if (await ChangeReservationTable(reservation, alreadyPresentTableReservation))
                {
                    if (HelperMethods.UserIsAdministrator(_user))
                        confirmMessage = $"Il tavolo precedentemente assegnato a {alreadyPresentTableReservation.ReservationTableName} e stato ora prenotato per {reservation.ReservationTableName}";
                    
                }
            }

            _db.Reservation.Add(reservation);
            await _db.SaveChangesAsync();

            return Ok(new ReservationResponse() { ReservationCode = reservation.ReservationCode, Message = confirmMessage });
        }

        /// <summary>
        /// Associa un tavolo "fisico" alla prenotazione
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        // POST: api/Tables
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Route("AssignTable")]
        public async Task<ActionResult<Table>> PostTable([FromBody] TableAssignPost table)
        {
            if (table == null)
                return BadRequest(new GeneralReponse() { Message = "Formato dati non valido.", OperationSuccess = false });
            //if (table.EventId <= 0 || table.ReservationId <= 0)
            //    return BadRequest(new GeneralReponse() { Message = "Selezionare un evento e una prenotazione valida.", OperationSuccess = false });           

            Reservation reservation = await _db.Reservation.FirstOrDefaultAsync(x => x.ReservationId == table.ReservationId);
            if (reservation == null)
                return NotFound(new GeneralReponse() { Message = "Nessuna prenotazione trovata.", OperationSuccess = false });

            //Controllo che il tavolo non sia già assegnato. In caso provvedo all'eventuale scambio
            Reservation oldReservation = _db.Reservation.FirstOrDefault(x => x.TableId == table.TableId && x.EventPartyId == table.EventId); //recupero l'eventuale tavolo già posizionato per l'evento
            if (oldReservation != null)
            {
                if (await ChangeReservationTable(reservation, oldReservation))
                    return Ok(new GeneralReponse() { Message = $"Il tavolo precedentemente assegnato a {oldReservation.ReservationTableName} e stato ora prenotato per {reservation.ReservationTableName}", OperationSuccess = false });
                else
                    return BadRequest(new GeneralReponse() { Message = "Il tavolo è stato già assegnato.", OperationSuccess = false });
            }


            reservation.TableId = table.TableId;
            _db.Entry(reservation).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            return Ok();
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
                //    EventPartyId = x.EventPartyId,
                //    ReservationExpectedBudget = x.ReservationExpectedBudget,
                //    ReservationPeopleCount = x.ReservationPeopleCount,
                //    Reservation
                //})
                .FirstOrDefaultAsync();


            return Ok(reservations);
        }

        /// <summary>
        /// A seguito di due prenotazioni che richiedono lo stesso tavolo, confonta fra il tavolo già presente e quello della nuova richiesta chi presenta il budget più alto.
        /// A seconda del risultato provvede ad assegnare il tavolo a quello economicamente migliore.
        /// </summary>
        private async Task<bool> ChangeReservationTable(Reservation newReservation, Reservation oldReservation)
        {
            bool tableChanged = false;

            if (newReservation.ReservationExpectedBudget > oldReservation.ReservationExpectedBudget)
            {
                newReservation.TableId = oldReservation.TableId;
                oldReservation.TableId = null;

                _db.Entry(oldReservation).State = EntityState.Modified;
                _db.Entry(newReservation).State = EntityState.Modified;
                                
                tableChanged = true;
            }
            else
            {
                tableChanged = false;
            }

           
            return tableChanged;
        }

        private bool ReservationExists(int eventId)
        {
            return _db.Reservation.Any(x => x.UserId == _user.Id && x.EventPartyId == eventId && x.ReservationStatusId == ReservationStatusValue.RESERVATIONSTATUS_APPROVED);
        }
    }
}
