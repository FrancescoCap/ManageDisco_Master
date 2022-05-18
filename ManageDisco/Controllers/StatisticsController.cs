using ManageDisco.Context;
using ManageDisco.Model;
using ManageDisco.Model.UserIdentity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = RolesConstants.ROLE_ADMINISTRATOR)]
    [ApiController]
    public class StatisticsController : BaseController
    {
        public StatisticsController(DiscoContext db, IConfiguration configuration) : base(db, configuration)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistcs([FromQuery] int eventId)
        {
            Statistics statistics = new Statistics();
            List<Task> tasks = new List<Task>()
            {
                new Task(() => {
                    statistics.FreeEntrance = new FreeEntrancePercentage();
                    statistics.EventTable = new EventTableOrder();
                    statistics.FreeEntrance.CouponList = _db.Coupon
                        .Where(x => x.EventId == eventId)
                        .Select(x => new FreeEntrance()
                        {
                             Name = _db.Users.FirstOrDefault(u => u.Id == x.UserId).Name,
                             Surname = _db.Users.FirstOrDefault(u => u.Id == x.UserId).Surname,
                             Validated = x.CouponValidated
                        }).ToList();

                    statistics.FreeEntrance.CouponSent = statistics.FreeEntrance.CouponList.Count;
                    if (statistics.FreeEntrance.CouponSent > 0)
                        statistics.FreeEntrance.CouponValidated = statistics.FreeEntrance.CouponList.Where(x => x.Validated == true).ToList().Count * 100 / statistics.FreeEntrance.CouponSent;
                    
                    var eventReservation = _db.Reservation.Include(x => x.Table).Where(x => x.EventPartyId == eventId).ToList();
                    statistics.EventTable.TableCount = eventReservation.Count();                    

                    var eventTables = eventReservation.Select(x => x.Table).ToList();
                    eventTables.ForEach(t =>
                    {
                        if (t != null)
                        {
                            statistics.EventTable.TotalOrderTable += _db.TableOrderHeader.Where(x => x.TableId == t.TableId).Sum(s => s.TableOrderHeaderSpending);
                            statistics.EventTable.PeopleCountFromTable += _db.TableOrderHeader.Where(x => x.TableId == t.TableId).Sum(s => s.TableOrderHeaderExit);
                        }                       
                    });
                    statistics.EventTable.TableCoupons = new List<EventTableOrderCoupon>();
                    var tableCouponUsed = _db.TableCouponUsed.ToList();
                    tableCouponUsed.ForEach(x =>
                    {
                        var tableName = _db.Reservation.FirstOrDefault(t => t.TableId == x.TableCouponTableId).ReservationTableName;
                        var couponDescription = _db.UserProduct.Include(i => i.ProductShopHeader).FirstOrDefault(c => c.UserProductCode == x.TableCouponCouponCode).ProductShopHeader.ProductShopHeaderName;
                        statistics.EventTable.TableCoupons.Add(new EventTableOrderCoupon()
                        {
                            TableName = tableName,
                            CouponCode = x.TableCouponCouponCode,
                            CouponDescription = couponDescription
                        });
                    });
                })
            };

            tasks.ForEach(x =>
            {
                x.Start();
                x.Wait();
            });

            return Ok(statistics);
        }
    }
}
