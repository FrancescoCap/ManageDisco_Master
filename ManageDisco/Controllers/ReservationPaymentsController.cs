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
    public class ReservationPaymentsController : BaseController
    {     

        public ReservationPaymentsController(DiscoContext db) : base(db)
        {
        }



        // GET: api/ReservationPayments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservationPayment>>> GetReservationPayment()
        {

            return Ok();

        }

        // GET: api/ReservationPayments/5
        [HttpGet("User")]
        public async Task<ActionResult<List<ReservationPayment>>> GetReservationPayment([FromQuery]string userId)
        {
            List<ReservationPayment> reservationPayment = null;

            if (_user.Roles.Contains(RolesConstants.ROLE_ADMINISTRATOR))
                reservationPayment = await _db.ReservationPayment.Where(x => x.UserId == userId).OrderBy(x => x.ReservationPaymentDate).ToListAsync();
            else if (_user.Roles.Contains(RolesConstants.ROLE_PR))
                reservationPayment = await _db.ReservationPayment.Where(x => x.UserId == _user.Id).OrderBy(x => x.ReservationPaymentDate).ToListAsync();

            if (reservationPayment == null)
            {
                return NotFound();
            }

            return reservationPayment;
        }

        // PUT: api/ReservationPayments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReservationPayment(int id, ReservationPayment reservationPayment)
        {
            if (id != reservationPayment.ReservationPaymentId)
            {
                return BadRequest();
            }

            _db.Entry(reservationPayment).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservationPaymentExists(id))
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

        // POST: api/ReservationPayments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> PostReservationPayment([FromBody]ReservationPayment reservationPayment)
        {
            if (reservationPayment == null)
                return BadRequest("Invalid data");
            if (reservationPayment.UserId == string.Empty)
                return BadRequest("Invalid user");

            _db.ReservationPayment.Add(reservationPayment);
            await _db.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/ReservationPayments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservationPayment(int id)
        {
            var reservationPayment = await _db.ReservationPayment.FindAsync(id);
            if (reservationPayment == null)
            {
                return NotFound();
            }

            _db.ReservationPayment.Remove(reservationPayment);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private bool ReservationPaymentExists(int id)
        {
            return _db.ReservationPayment.Any(e => e.ReservationPaymentId == id);
        }
    }
}
