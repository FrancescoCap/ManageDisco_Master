using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ManageDisco.Context;
using ManageDisco.Model;
using Microsoft.Extensions.Configuration;

namespace ManageDisco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductShopTypesController : BaseController
    {
       
        public ProductShopTypesController(DiscoContext db, IConfiguration configuration) : base(db, configuration)
        {
        }

        // GET: api/ProductShopTypes
        [HttpGet]
        public async Task<IActionResult> GetProductShopType()
        {
            return Ok(await _db.ProductShopType.ToListAsync());
        }       

        // PUT: api/ProductShopTypes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductShopType(int id, ProductShopType productShopType)
        {
            if (id != productShopType.ProductShopTypeId)
            {
                return BadRequest();
            }

            _db.Entry(productShopType).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductShopTypeExists(id))
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

        // POST: api/ProductShopTypes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ProductShopType>> PostProductShopType(ProductShopType productShopType)
        {
            _db.ProductShopType.Add(productShopType);
            await _db.SaveChangesAsync();

            return CreatedAtAction("GetProductShopType", new { id = productShopType.ProductShopTypeId }, productShopType);
        }

        // DELETE: api/ProductShopTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductShopType(int id)
        {
            var productShopType = await _db.ProductShopType.FindAsync(id);
            if (productShopType == null)
            {
                return NotFound();
            }

            _db.ProductShopType.Remove(productShopType);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductShopTypeExists(int id)
        {
            return _db.ProductShopType.Any(e => e.ProductShopTypeId == id);
        }
    }
}
