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

namespace ManageDisco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentOverviewsController : BaseController
    {
        public PaymentOverviewsController(DiscoContext db) : base(db)
        {
        }


        // GET: api/PaymentOverviews
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentOverview>>> GetPaymentOverview([FromQuery] string userId)
        {
            if (userId == "Seleziona collaboratore")
                return Ok(new List<ReservationPaymentView>());


            IQueryable<ReservationPaymentView> payments = _db.PaymentOverview
              .Select(x => new ReservationPaymentView()
              {
                  PaymentId = x.PaymentOverviewId,
                  Name = x.User.Name,
                  Surname = x.User.Surname,
                  TotalIncoming = x.TotalIncoming,
                  TotalPayed = x.TotalCreditPayed,
                  ResumeCredit = x.TotalCreditResume,
                  UserId = x.UserId
              });

            if (!String.IsNullOrEmpty(userId))
                payments = payments.Where(x => x.UserId == userId);

            if (_user.Roles.Contains(RolesConstants.ROLE_PR))
                payments = payments.Where(x => x.UserId == _user.Id);


            return Ok(payments.ToList());

        }  

        // POST: api/PaymentOverviews
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PaymentOverview>> PostPaymentOverview(PaymentOverview paymentOverview)
        {
            _db.PaymentOverview.Add(paymentOverview);
            await _db.SaveChangesAsync();

            return CreatedAtAction("GetPaymentOverview", new { id = paymentOverview.PaymentOverviewId }, paymentOverview);
        }

        // DELETE: api/PaymentOverviews/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaymentOverview(int id)
        {
            var paymentOverview = await _db.PaymentOverview.FindAsync(id);
            if (paymentOverview == null)
            {
                return NotFound();
            }

            _db.PaymentOverview.Remove(paymentOverview);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
