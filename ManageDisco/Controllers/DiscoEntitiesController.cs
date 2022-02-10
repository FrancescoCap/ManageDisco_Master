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
    public class DiscoEntitiesController : ControllerBase
    {
        private readonly DiscoContext _context;

        public DiscoEntitiesController(DiscoContext context)
        {
            _context = context;
        }

        // GET: api/DiscoEntities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DiscoEntity>>> GetDiscoEntity()
        {
            return await _context.DiscoEntity.ToListAsync();
        }

        // GET: api/DiscoEntities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DiscoEntity>> GetDiscoEntity(string id)
        {
            var discoEntity = await _context.DiscoEntity.FindAsync(id);

            if (discoEntity == null)
            {
                return NotFound();
            }

            return discoEntity;
        }

        // PUT: api/DiscoEntities/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDiscoEntity(string id, DiscoEntity discoEntity)
        {
            if (id != discoEntity.DiscoId)
            {
                return BadRequest();
            }

            _context.Entry(discoEntity).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DiscoEntityExists(id))
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

        // POST: api/DiscoEntities
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<DiscoEntity>> PostDiscoEntity(DiscoEntity discoEntity)
        {
            _context.DiscoEntity.Add(discoEntity);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (DiscoEntityExists(discoEntity.DiscoId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetDiscoEntity", new { id = discoEntity.DiscoId }, discoEntity);
        }

        // DELETE: api/DiscoEntities/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDiscoEntity(string id)
        {
            var discoEntity = await _context.DiscoEntity.FindAsync(id);
            if (discoEntity == null)
            {
                return NotFound();
            }

            _context.DiscoEntity.Remove(discoEntity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DiscoEntityExists(string id)
        {
            return _context.DiscoEntity.Any(e => e.DiscoId == id);
        }
    }
}
