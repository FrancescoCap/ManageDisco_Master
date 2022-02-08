using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ManageDisco.Context;
using ManageDisco.Model;
using System.Text.Json.Serialization;

namespace ManageDisco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : BaseController
    {
        public ProductsController(DiscoContext db) : base(db)
        {
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductCatalogView>>> GetProduct([FromQuery] int catalogId)
        {
            IEnumerable<ProductCatalogView> products =  _db.Product                
                .Select(x => new ProductCatalogView()
                {
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    ProductPrice = x.ProductPrice,
                    CatalogName = x.Catalog.CatalogName,
                    CatalogId = x.CatalogId
                }).OrderBy(x => x.ProductPrice);

            if (catalogId > 0)
            {
                products = products.Where(x => x.CatalogId == catalogId);
            }

            return products.ToList();
        }



        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.ProductId)
            {
                return BadRequest();
            }

            _db.Entry(product).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
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

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]        
        public async Task<ActionResult<Product>> PostProduct([FromBody] Product product)
        {
            _db.Product.Add(product);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Products/5
        [HttpDelete]
        public async Task<IActionResult> DeleteProduct([FromQuery]int productId)
        {
            var product = await _db.Product.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            _db.Product.Remove(product);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _db.Product.Any(e => e.ProductId == id);
        }
    }
}
