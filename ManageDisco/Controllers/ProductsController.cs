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
using Microsoft.AspNetCore.Authorization;

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
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductCatalogView>>> GetProduct([FromQuery] int catalogId, [FromQuery] bool shop)
        {
            IEnumerable<ProductCatalogView> products =  _db.Product
                .Select(x => new ProductCatalogView()
                {
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    ProductPrice = x.ProductPrice,
                    CatalogName = x.Catalog.CatalogName,
                    CatalogId = x.CatalogId,
                    ProductShopTypeId = x.ProductShopTypeId
                }).OrderBy(x => x.ProductPrice);

            //If products are required for table order have to filter them for only TABLE product
            if (!shop)
            {
                var productShopTypes = await _db.ProductShopType
                    .Where(x => x.ProductShopTypeDescription != ProductShopTypeContants.PRODUCT_SHOP_TYPE_PRODUCT && x.ProductShopTypeDescription != ProductShopTypeContants.PRODUCT_SHOP_TYPE_ENTRY)
                    .Select(x => x.ProductShopTypeId)
                    .ToListAsync();
                products = products.Where(x => productShopTypes.Contains(x.ProductShopTypeId));
            }

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
            if (product == null)
                return BadRequest(new GeneralReponse() { OperationSuccess = false, Message = "Dati non validi." });
            if (product.CatalogId < 1)
                return BadRequest(new GeneralReponse() { OperationSuccess = false, Message = "Selezionare un catalogo valido." });
            if (product.ProductName == null)
                return BadRequest(new GeneralReponse() { OperationSuccess = false, Message = "Inserire un nome prodotto." });
            if (product.ProductShopTypeId <= 0)
            {
                product.ProductShopTypeId = _db.ProductShopType.FirstOrDefault(x => x.ProductShopTypeDescription == ProductShopTypeContants.PRODUCT_SHOP_TYPE_TABLE).ProductShopTypeId;
            }

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
