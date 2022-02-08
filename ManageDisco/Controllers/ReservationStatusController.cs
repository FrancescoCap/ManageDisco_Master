using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ManageDisco.Context;
using ManageDisco.Model;

namespace ManageDisco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationStatusController : BaseController
    {
        public ReservationStatusController(DiscoContext db) : base(db)
        {
        }


        // GET: api/ReservationStatus
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservationStatus>>> GetReservationStatus()
        {
            return await _db.ReservationStatus.ToListAsync();
        }

        // GET: api/ReservationStatus/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReservationStatus>> GetReservationStatus(int id)
        {
            var reservationStatus = await _db.ReservationStatus.FindAsync(id);

            if (reservationStatus == null)
            {
                return NotFound();
            }

            return reservationStatus;
        }

        // PUT: api/ReservationStatus/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReservationStatus(int id, ReservationStatus reservationStatus)
        {
            if (id != reservationStatus.ReservationStatusId)
            {
                return BadRequest();
            }

            _db.Entry(reservationStatus).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservationStatusExists(id))
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

        // POST: api/ReservationStatus
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ReservationStatus>> PostReservationStatus(ReservationStatus reservationStatus)
        {
            _db.ReservationStatus.Add(reservationStatus);
            await _db.SaveChangesAsync();

            return CreatedAtAction("GetReservationStatus", new { id = reservationStatus.ReservationStatusId }, reservationStatus);
        }

        // DELETE: api/ReservationStatus/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservationStatus(int id)
        {
            var reservationStatus = await _db.ReservationStatus.FindAsync(id);
            if (reservationStatus == null)
            {
                return NotFound();
            }

            _db.ReservationStatus.Remove(reservationStatus);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private bool ReservationStatusExists(int id)
        {
            return _db.ReservationStatus.Any(e => e.ReservationStatusId == id);
        }
    }
}
