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
    public class CatalogsController : BaseController
    {
        public CatalogsController(DiscoContext db) : base(db)
        {
        }


        // GET: api/Catalogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Catalog>>> GetCatalog()
        {
            return await _db.Catalog.ToListAsync();
        }

        // GET: api/Catalogs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Catalog>> GetCatalog(int id)
        {
            var catalog = await _db.Catalog.FindAsync(id);

            if (catalog == null)
            {
                return NotFound();
            }

            return catalog;
        }

        // PUT: api/Catalogs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCatalog(int id, Catalog catalog)
        {
            if (id != catalog.CatalogId)
            {
                return BadRequest();
            }

            _db.Entry(catalog).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CatalogExists(id))
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

        // POST: api/Catalogs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> PostCatalog([FromBody] Catalog catalog)
        {
            _db.Catalog.Add(catalog);
            await _db.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/Catalogs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCatalog(int id)
        {
            var catalog = await _db.Catalog.FindAsync(id);
            if (catalog == null)
            {
                return NotFound();
            }

            _db.Catalog.Remove(catalog);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private bool CatalogExists(int id)
        {
            return _db.Catalog.Any(e => e.CatalogId == id);
        }
    }
}
