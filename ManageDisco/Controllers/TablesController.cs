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

namespace ManageDisco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TablesController : BaseController
    {
        public TablesController(DiscoContext db) : base(db)
        {
        }

        // GET: api/Tables
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Table>>> GetTable()
        {
            return await _db.Table.ToListAsync();
        }


        [HttpGet]
        [Route("Map")]
        public async Task<IActionResult> GetTableMap()
        {
            /*
             * ALGORITMO MOMENTANEO. QUANDO IMPLEMENTERO' IL SERVER IL FILE DOVRA' ESSERE MESSO SULL'FTP. AL CLIENT PERO' PASSO SEMPRE E SOLO IL PATH.
             * RECUPERERO IL NOME DEL FILE A SECONDA DELLA DISCOTECA A CUI APPARTIENE L'UTENTE (SE COLLABORATORE) O SU CUI SI STA EFFETTUANDO LA PRENOTAZIONE (IN CASO DI CLIENTE)
             */

            return base.Ok(new TableMapReponse() { Path = @"C:\Users\Francesco\source\repos\ManageDisco\ManageDisco\Resource\pianta_tavoli.pdf", FileName = "pianta_tavoli.pdf" });
        }

        /// <summary>
        /// Restituisce i tavoli presenti per un determinato evento
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("TablesOrder")]
        public async Task<IActionResult> GetConfirmedTablesForEvent([FromQuery] int eventId)
        {

            IQueryable<TableOrderView> tables = _db.Reservation
                 .Where(x => x.TableId != null && x.ReservationStatusId == ReservationStatusValue.RESERVATIONSTATUS_APPROVED)
                 .Select(x => new TableOrderView()
                 {
                     TableId = x.TableId.Value,
                     TableAreaDescription = x.Table.TableAreaDescription,
                     TableNumber = x.Table.TableNumber,
                     TableName = x.ReservationTableName,
                     EventId = x.EventPartyId,
                     TableDate = x.EventParty.Date,
                     HasOrder = _db.TableOrderHeader.Any(o => o.TableId == x.TableId && x.EventPartyId == eventId)
                 });

            if (eventId > 0)
                tables = tables.Where(x => x.EventId == eventId);
            else
                tables = tables.Where(x => x.TableDate.CompareTo(DateTime.Today) == 0);

            TableOrderViewHeader header = new TableOrderViewHeader();
            header.Tables = await tables.ToListAsync();
            header.EventId = tables.FirstOrDefault() == null ? eventId : tables.FirstOrDefault().EventId;

            return Ok(header);
        }

        /// <summary>
        /// Restituisce la lista degli ordini effettuati dal tavolo
        /// </summary>
        /// <param name="id"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("TableOrdersList")]
        public async Task<IActionResult> GetOrderList([FromQuery] int tableId)
        {
            //Per tutta la serata il tavolo avrà sempre e solo un header in cui verranno
            //aggiornati i valori ad ogni ordine
            TableOrderHeader header = await _db.TableOrderHeader
                .Where(x => x.TableId == tableId)
                .Select(x => new TableOrderHeader()
                {
                    TableOrderHeaderId = x.TableOrderHeaderId,
                    TableOrderHeaderExit = x.TableOrderHeaderExit,
                    TableOrderHeaderSpending = x.TableOrderHeaderSpending

                }).FirstOrDefaultAsync();

            List<TableOrderRowView> rows = await _db.TableOrderRow
                .Where(x => x.TableOrderHeader.TableId == tableId)
                .Select(x => new TableOrderRowView()
                {
                    TableOrderHeaderId = x.TableOrderHeaderId,
                    TableOrderRowQuantity = x.TableOrderRowQuantity,
                    ProductId = x.ProductId,
                    ProductName = x.Product.ProductName

                }).ToListAsync();

            var groupedRow = rows
                .GroupBy(x => x.ProductName)
                .Select(x => new TableOrderRowView
                {
                    ProductName = x.Key,
                    TableOrderRowQuantity = x.Sum(x => x.TableOrderRowQuantity)
                });

            TableOrderSummary summary = new TableOrderSummary();
            summary.Rows = groupedRow.ToList();
            summary.TableOrderHeaderExit = header.TableOrderHeaderExit;
            summary.TableOrderHeaderSpending = header.TableOrderHeaderSpending;
            summary.TableOrderHeaderId = header.TableOrderHeaderId;


            return Ok(summary);
        }

        [HttpGet]
        [Route("EventTables")]
        public async Task<IActionResult> GetEventTables([FromQuery] int eventId)
        {
            TableEventView tableEventView = new TableEventView();

            List<Task> tasks = new List<Task>()
            {
                new Task(() => { 
                    //events
                    tableEventView.EventParties = _db.Events.ToList();
                }),
                new Task(async() => {
                    tableEventView.ReservationStatus = _db.ReservationStatus.ToList();
                }),
                new Task(async() => {
                    tableEventView.Tables = _db.Table.ToList();
                }),
                new Task(async () => {

                    var tmpReservations = _db.Reservation
                        .Select(x => new ReservationView(){
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
                            ReservationName = x.ReservationTableName,
                            TableId = x.TableId != null ? x.TableId : 0,
                            ReservationTablAssigned = $"{x.Table.TableAreaDescription} - {x.Table.TableNumber}"
                        });

                    if (eventId > 0)
                       tmpReservations = tmpReservations.Where(x => x.EventId == eventId);
                    else
                    {
                        var startDate = DateTime.Now.Date.AddDays(-7);
                        tmpReservations = tmpReservations.Where(x => x.ReservationDate.Date.CompareTo(startDate) > 0);
                    }

                    if (HelperMethods.UserIsPr(_user))
                        tmpReservations = tmpReservations.Where(x => x.UserId == _user.Id);

                    tableEventView.Reservations = tmpReservations.ToList();
                })
            };

            foreach (Task t in tasks)
            {
                t.Start();
                t.Wait();
            }

            tableEventView.UserCanHandleReservation = HelperMethods.UserIsAdministrator(_user);

            return Ok(tableEventView);
        }

        // PUT: api/Tables/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTable(int id, Table table)
        {
            if (id != table.TableId)
            {
                return BadRequest();
            }

            _db.Entry(table).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TableExists(id))
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
        /// <summary>
        /// Assegna un ordine al tavolo
        /// </summary>
        /// <param name="tableId"></param>
        /// <param name="orderInfo"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("TableOrder")]
        public async Task<IActionResult> SetTableOrder([FromQuery] int tableId, [FromBody] TableOrderPut orderInfo)
        {
            if (tableId <= 0)
                return BadRequest("Invalid table");
            if (orderInfo == null || orderInfo.ProductsId == null || !orderInfo.ProductsId.Any())
                return Ok();


            Table table = await _db.Table.FirstOrDefaultAsync(x => x.TableId == tableId);
            if (table == null)
                return NotFound("Table not found");

            List<Product> product = await _db.Product.Where(x => orderInfo.ProductsId.Keys.Contains(x.ProductId)).ToListAsync();
            if (product == null || product.Count == 0)
                return NotFound("No products found");

            TableOrderHeader orderHeader = await _db.TableOrderHeader.FirstOrDefaultAsync(x => x.TableId == tableId);
            if (orderHeader == null)
            {
                //se entro qui significa che è il primo ordine per il tavolo quindi devo creare l'oggetto
                orderHeader = new TableOrderHeader()
                {
                    TableId = tableId,
                    TableOrderHeaderExit = orderInfo.ExitChanged,
                    TableOrderHeaderSpending = orderInfo.ProductsSpendingAmount,
                    TableOrderHeaderCouponCode = orderInfo.ShopCoupon
                };
                _db.TableOrderHeader.Add(orderHeader);
            }
            else
            {
                //Utilizzo di un solo coupon per tavolo ad ogni evento
                if (!String.IsNullOrEmpty(orderInfo.ShopCoupon))
                {
                    if (IsCouponAlreadyUsed(orderHeader))
                        return BadRequest(new GeneralReponse() { Message = "Il tavolo ha già usufruito di un coupon.", OperationSuccess = false });
                    else
                        orderHeader.TableOrderHeaderCouponCode = orderInfo.ShopCoupon;
                }

                orderHeader.TableOrderHeaderExit = orderHeader.TableOrderHeaderExit + orderInfo.ExitChanged;
                orderHeader.TableOrderHeaderSpending = orderHeader.TableOrderHeaderSpending + orderInfo.ProductsSpendingAmount;
                _db.Entry(orderHeader).State = EntityState.Modified;
            }

            List<TableOrderRow> orderRows = new List<TableOrderRow>();
            foreach (int key in orderInfo.ProductsId.Keys)
            {
                orderRows.Add(new TableOrderRow()
                {
                    ProductId = key,
                    TableOrderRowQuantity = orderInfo.ProductsId[key],
                    TableOrderHeader = orderHeader
                });
            }

            //add coupon products if client sent.
            //Set validate = false for coupon and add product rows inherited from it
            if (!String.IsNullOrEmpty(orderInfo.ShopCoupon))
            {
                var userOwner = _db.Reservation.FirstOrDefaultAsync(x => x.TableId == table.TableId && x.EventPartyId == orderInfo.EventId).Result.UserIdOwner;
                if (_db.UserProduct.Any(x => x.UserId == userOwner && x.UserProductCode == orderInfo.ShopCoupon && x.UserProductUsed == false))
                {
                    var userProduct = await _db.UserProduct.FirstOrDefaultAsync(x => x.UserProductCode == orderInfo.ShopCoupon);
                    ProductShopHeader shopProduct = await _db.ProductShopHeader.FirstOrDefaultAsync(x => x.ProductShopHeaderIdId == userProduct.ProductShopHeaderId);
                    if (shopProduct == null)
                        return BadRequest(new GeneralReponse() { OperationSuccess = false, Message = "Il prodotto non è più disponibile." });

                    var productShopRows = await _db.ProductShopRow.Where(x => x.ProductShopHeaderId == shopProduct.ProductShopHeaderIdId).ToListAsync();
                    productShopRows.ForEach(x =>
                    {
                        orderRows.Add(new TableOrderRow()
                        {
                            ProductId = x.ProductId,
                            TableOrderRowQuantity = x.ProductShopRowQuantity,
                            TableOrderHeader = orderHeader
                        });
                    });


                    userProduct.UserProductUsed = true;
                    _db.Entry(userProduct).State = EntityState.Modified;

                    SaveCouponHistory(orderInfo.EventId, orderHeader.TableId, orderInfo.ShopCoupon);
                }

            }

            _db.TableOrderRow.AddRange(orderRows);

            await _db.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/Tables/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTable(int id)
        {
            var table = await _db.Table.FindAsync(id);
            if (table == null)
            {
                return NotFound();
            }

            _db.Table.Remove(table);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private bool TableExists(int id)
        {
            return _db.Table.Any(e => e.TableId == id);
        }

        private bool IsCouponAlreadyUsed(TableOrderHeader header)
        {
            return !String.IsNullOrEmpty(header.TableOrderHeaderCouponCode);
        }

        private void SaveCouponHistory(int eventId, int tableId, string coupon)
        {
            _db.TableCouponUsed.Add(new TableCouponUsed()
            {
                TableCouponEventId = eventId,
                TableCouponTableId = tableId,
                TableCouponCouponCode = coupon
            });
        }
    }
}
