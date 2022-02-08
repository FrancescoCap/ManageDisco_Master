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
        public async Task<ActionResult<IEnumerable<PaymentOverview>>> GetPaymentOverview()
        {
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

            if (_user.Roles.Contains(RolesConstants.ROLE_PR))
                payments = payments.Where(x => x.UserId == _user.Id);
          

            return Ok(payments.ToList());
        }

        // GET: api/PaymentOverviews/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentOverview>> GetPaymentOverview(int id)
        {
            var paymentOverview = await _db.PaymentOverview.FindAsync(id);

            if (paymentOverview == null)
            {
                return NotFound();
            }

            return paymentOverview;
        }

        // PUT: api/PaymentOverviews/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPaymentOverview(int id, PaymentOverview paymentOverview)
        {
            if (id != paymentOverview.PaymentOverviewId)
            {
                return BadRequest();
            }

            _db.Entry(paymentOverview).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentOverviewExists(id))
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

        private bool PaymentOverviewExists(int id)
        {
            return _db.PaymentOverview.Any(e => e.PaymentOverviewId == id);
        }
    }
}
