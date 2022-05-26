﻿using System;
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
using System.IO;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.Advanced;
using PdfSharpCore.Drawing;

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
        public async Task<IActionResult> GetReservation([FromQuery] int eventId, [FromQuery] int id)
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
                    IsReservationEditable = x.ReservationStatusId == ReservationStatusValue.RESERVATIONSTATUS_APPROVED && HelperMethods.UserIsAdministrator(_user),
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

            var reservationList = await reservationViews.ToListAsync();

            return Ok(reservationList.OrderByDescending(x => x.ReservationExpectedBudget).ThenByDescending(x => x.ReservationRealBudget).ThenByDescending(x => x.ReservationDate));
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
                }).OrderByDescending(x => x.ReservationExpectedBudget).ThenByDescending(x => x.ReservationRealBudget).ThenByDescending(x => x.ReservationDate);


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

        [Authorize(Roles = RolesConstants.ROLE_ADMINISTRATOR + "," + RolesConstants.ROLE_PR + "," + RolesConstants.ROLE_WAREHOUSE_WORKER)]
        [HttpGet]
        [Route("Table/PdfExport")]
        public async Task<IActionResult> ExportReservationsTablesPdf([FromQuery]int eventId)
        {
            if (eventId <= 0)
                return BadRequest(new GeneralReponse() { Message = "Per esportare i tavoli è necessario selezionare un evento.", OperationSuccess = false });
                       
            var eventParty = await _db.Events.FirstOrDefaultAsync(x => x.Id == eventId);

            FileHelper pdfHelper = new FileHelper();
            pdfHelper.SetPdfHeader($"Lista tavoli {eventParty.Name.ToUpper()} {eventParty.Date.ToShortDateString()}", XBrushes.Black, 20, "Calibri", true);
            pdfHelper.SetPdfRows(await GetTableRowsForExport(eventId), XBrushes.Black, 12, "Calibri", false);

            FileStreamResult file = new FileStreamResult(pdfHelper.GeneratePdf("test.pdf"), "application/pdf");
            file.FileDownloadName = "Test.pdf";

            return file;
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

            reservationOld.ReservationTableName = !String.IsNullOrEmpty(reservation.ReservationName) ? reservation.ReservationName : reservationOld.ReservationTableName;
            reservationOld.ReservationExpectedBudget = reservation.ReservationExpectedBudget != reservationOld.ReservationExpectedBudget && reservation.ReservationExpectedBudget > 0 ? reservation.ReservationExpectedBudget : reservationOld.ReservationExpectedBudget;
            reservationOld.ReservationRealBudget = reservation.ReservationRealBudget != reservationOld.ReservationRealBudget && reservation.ReservationRealBudget > 0 ? reservation.ReservationRealBudget : reservationOld.ReservationRealBudget;
            reservationOld.ReservationPeopleCount = reservation.ReservationPeopleCount != reservationOld.ReservationPeopleCount && reservation.ReservationPeopleCount > 0 ? reservation.ReservationPeopleCount : reservationOld.ReservationPeopleCount;


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
                return NotFound(new GeneralReponse() { Message = "Non è stata trovata nessuna prenotazione con questo identificativo.", OperationSuccess = false });

            if (reservation.TableId == null)
                return BadRequest(new GeneralReponse() { Message = "Per la conferma del budget il tavolo deve essere prima assegnato.", OperationSuccess = false });

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
                ReservationStatusId = reservationData.TableId != 0 ? ReservationStatusValue.RESERVATIONSTATUS_APPROVED : ReservationStatusValue.RESERVATIONSTATUS_PENDING, //if tableId is provided means it is accepted automatically
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
                if (HelperMethods.UserIsAdministrator(_user))
                {
                    if (await ChangeReservationTable(reservation, alreadyPresentTableReservation))
                    {
                        if (HelperMethods.UserIsAdministrator(_user))
                            confirmMessage = $"Il tavolo precedentemente assegnato a {alreadyPresentTableReservation.ReservationTableName} e stato ora prenotato per {reservation.ReservationTableName}";
                    }
                    else
                    {
                        confirmMessage = "La prenotazione è stata inserita ma il tavolo non assegnato perché già assegnto.";
                    }
                }
                else
                {
                    confirmMessage = $"Il tavolo è stato già assegnato.";
                    return BadRequest(new GeneralReponse() { Message = confirmMessage, OperationSuccess = false });
                }           
            }
            else
            {
                reservation.TableId = reservationData.TableId;
            }

            _db.Reservation.Add(reservation);
            await _db.SaveChangesAsync();

            return Ok(new ReservationResponse() { ReservationCode = reservation.ReservationCode, Message = confirmMessage });
        }

        [HttpGet]
        [Route("FreeTables")]
        public async Task<IActionResult> GetFreeTableForReservation([FromQuery] decimal budget, [FromQuery] int eventId)
        {
            if (budget == 0)
                return Ok("Budget cannot be zero.");
            if (eventId == 0)
                return Ok("Not valid event");

            var busyTables = await _db.Reservation.Where(x => x.EventPartyId == eventId).Select(x => x.TableId).ToListAsync();
            var freeTables = await _db.Table.Where(x =>  budget >= x.TableMinBudget &&!busyTables.Contains(x.TableId)).Select(x => new FreeReservationTables() { Description = $"{x.TableAreaDescription}-{x.TableNumber}" }).ToListAsync();

            return Ok(freeTables);
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
                .FirstOrDefaultAsync();


            return Ok(reservations);
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
            if (reservation.ReservationStatusId != 2)
                return BadRequest(new GeneralReponse() { Message = "Il tavolo non è stato accettato.", OperationSuccess = false });

            //Controllo che il tavolo non sia già assegnato. In caso provvedo all'eventuale scambio
            Reservation oldReservation = _db.Reservation.FirstOrDefault(x => x.TableId == table.TableId && x.EventPartyId == table.EventId); //recupero l'eventuale tavolo già posizionato per l'evento
            if (oldReservation != null)
            {
                if (await ChangeReservationTable(reservation, oldReservation))
                    return Ok(new GeneralReponse() { Message = $"Il tavolo precedentemente assegnato a {oldReservation.ReservationTableName} e stato ora prenotato per {reservation.ReservationTableName}", OperationSuccess = false });
                else
                {
                    if (HelperMethods.UserIsAdministrator(_user))
                        return BadRequest(new GeneralReponse() { Message = $"Il tavolo è stato già assegnato a {oldReservation.ReservationTableName}.", OperationSuccess = false });
                    else
                        return BadRequest(new GeneralReponse() { Message = $"Il tavolo è stato già prenotato.", OperationSuccess = false });
                }

            }
            else
            {
                reservation.TableId = table.TableId;
            }

           
            _db.Entry(reservation).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            return Ok(new GeneralReponse() { Message = $"Il tavolo è stato assegnato a {reservation.ReservationTableName}", OperationSuccess = false });
        }

        /// <summary>
        /// Genera una proposta di assegnazione dei tavoli per le prenotazioni.
        /// I criteri sono:
        /// 1. Delta più alto fra la quota minima necessaria per il tavolo e il budget previsto per lo stesso
        /// 2. Totale prenotazioni effettuate dal pr nel corso della stagione
        /// 3. Data di prenotazione
        /// Se dopo questi 3 confronti non è stato possibile assegnare un tavolo alla prenotazione, l'amministratore dovrà procedere a mano.
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        [Authorize(Roles = RolesConstants.ROLE_ADMINISTRATOR)]
        [HttpPost]
        [Route("AutoAssign")]
        public async Task<IActionResult> AutoAssignTable([FromQuery] int eventId)
        {
            //Lista contenitore di tutti i tavoli che rispettati i 3 criteri sono rimasti scoperti
            List<string> conflictTable = new List<string>();
            List<Reservation> tablesUsedId = new List<Reservation>();

            if (eventId < 1)
                return BadRequest();

            var discoTables = await _db.Table.ToListAsync();
            var reservation = await _db.Reservation.Where(x => x.EventPartyId == eventId && x.ReservationExpectedBudget > 0).OrderByDescending(x => x.ReservationExpectedBudget).ThenByDescending(x => x.ReservationRealBudget).ThenByDescending(x => x.ReservationDate).ToListAsync();
            foreach (Reservation r in reservation)
            {
                Table freeTable = GetFirstAvaiableTable(r, eventId, r.ReservationExpectedBudget);
                if (freeTable == null)  //probably suitable condition table are finished
                    continue;

                var actualReservation = reservation.FirstOrDefault(x => x.TableId == freeTable.TableId);

                if (actualReservation == null)
                {
                    //Non c'è una preotazione su questo tavolo
                    Reservation newReservation = reservation.FirstOrDefault(x => x.ReservationId == r.ReservationId);
                    newReservation.TableId = freeTable.TableId;
                    _db.Entry(newReservation).State = EntityState.Modified;                    
                }
                else
                {
                    //il tavolo è già impegnato. Devo fare i confronti ed eventualmente sostituire
                    bool tableChangeOwner = await ChangeReservationTable(r, actualReservation);
                    Table firstAvaiableTable = GetFirstAvaiableTable(actualReservation, eventId, actualReservation.ReservationExpectedBudget);
                    actualReservation.TableId = firstAvaiableTable != null ? firstAvaiableTable.TableId : null;
                }
                await _db.SaveChangesAsync();
            }
            
            return Ok();
        }


        //}
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
                _db.Reservation.Update(newReservation);
                _db.Reservation.Update(oldReservation);

                await _db.SaveChangesAsync();
                tableChanged = true;
            }
            else
            {
                tableChanged = false;
            }
            

            return tableChanged;
        }

        private Table GetFirstAvaiableTable(Reservation reservation, int eventId, decimal reservationBudget)
        {
            Table table = null;

            Dictionary<int, decimal> tableReservationBudget = new Dictionary<int, decimal>();
            
            var filteredTables = _db.Table.Where(x => x.TableMinBudget <= reservation.ReservationExpectedBudget).ToList();
            foreach (Table t in filteredTables)
            {
                var actualTableReservation = _db.Reservation.FirstOrDefault(x => x.TableId == t.TableId && x.EventPartyId == eventId);
                if (actualTableReservation == null)
                {
                    table = t;
                    break;
                }
                else //se entro qui significa che c'è già una prenotazione per quel tavolo
                {
                    //se la prenotazione sotto processo ha un budget superiore rispetto alla prenotazione già assegnata per quel tavolo restituisco quel tavolo perché va fatta una sostituzione
                    if (reservation.ReservationExpectedBudget > actualTableReservation.ReservationExpectedBudget)
                    {
                        table = t;
                        break;
                    }
                }
            }          

            return table;
        }

        private async Task<string[]> GetTableRowsForExport(int eventId)
        {
            IQueryable< Reservation> reservationsTemp = _db.Reservation.Include(i => i.Table).Where(x => x.EventPartyId == eventId && x.TableId != null);
            if (HelperMethods.UserIsPr(_user) || (HelperMethods.UserIsInStaff(_user) && !HelperMethods.UserIsAdministrator(_user)))
            {
                //if user is PR or WAREHOUSE_WORKER, export only their reservations
                reservationsTemp = reservationsTemp.Where(x => x.UserId == _user.Id);
            }
            Reservation[] reservations = await reservationsTemp.ToArrayAsync();
            string[] values = new string[reservations.Length];

            for (int i = 0; i < reservations.Length; i++)
            {
                values[i] = string.Format("{0} - {1}:{2} ({3} persone)", reservations[i].ReservationTableName, reservations[i].Table.TableAreaDescription, reservations[i].Table.TableNumber, reservations[i].ReservationPeopleCount);
            }

            return values;
        }

    }
}
