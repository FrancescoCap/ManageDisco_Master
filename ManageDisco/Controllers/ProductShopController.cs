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
using ManageDisco.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ManageDisco.Model.UserIdentity;

namespace ManageDisco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductShopController : BaseController
    {
     
        public ProductShopController(DiscoContext db, IConfiguration configuration) : base(db, configuration)
        {
        }

       /// <summary>
       /// GET ProductShop ONLY headers. It is used for shop page view
       /// </summary>
       /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetProductShop()
        {
            List<ProductShopView> header = await _db.ProductShopHeader
                .Select(x => new ProductShopView()
                {
                    ProductShopHeaderIdId = x.ProductShopHeaderIdId,
                    ProductShopHeaderName = x.ProductShopHeaderName,
                    ProductShopHeaderDescription = x.ProductShopHeaderDescription,
                    ProductShopHeaderPrice = x.ProductShopHeaderPrice,
                    ProductShopImagePath = x.ProductShopImagePath
                }).ToListAsync();

            header.ForEach(x =>
            {
                x.productShopBase64Image = !String.IsNullOrEmpty(x.ProductShopImagePath) ? HelperMethods.GetBase64Image(x.ProductShopImagePath, ftpUser, ftpPassword) : HelperMethods.GetBase64DefaultNoImage(ftpAddress, ftpUser, ftpPassword);
            });

            return Ok(header);
        }

        [HttpGet("UserAwards")]
        public async Task<ActionResult<ProductShopHeader>> GetUserAwards()
        {

            List<UserProduct> userProducts = await _db.UserProduct
                .Where(x => x.UserId == _user.Id)
                .Select(x => new UserProduct()
                {
                    ProductShopHeaderId = x.ProductShopHeaderId,
                    UserProductCode = x.UserProductCode,
                    UserProductUsed = x.UserProductUsed,
                    ProductShopHeader = new ProductShopHeader()
                    {
                        ProductShopHeaderName = x.ProductShopHeader.ProductShopHeaderName,
                        ProductShopHeaderDescription = x.ProductShopHeader.ProductShopHeaderDescription,
                        ProductShopHeaderPrice = x.ProductShopHeader.ProductShopHeaderPrice
                    }
                }).ToListAsync();

            return Ok(userProducts);
        }


        [Authorize(Roles = RolesConstants.ROLE_ADMINISTRATOR)]
        [HttpPost]
        public async Task<IActionResult> PostProductShop([FromBody] ProductShopPost productShop)
        {
            if (productShop == null)
                return BadRequest();
            if (String.IsNullOrEmpty(productShop.ProductShopHeaderDescription))
                return BadRequest(new GeneralReponse() { OperationSuccess = false, Message = "Inserire una descrizione per l'articolo."});
            if (String.IsNullOrEmpty(productShop.ProductShopHeaderName))
                return BadRequest(new GeneralReponse() { OperationSuccess = false, Message = "Inserire un nome per l'articolo." });
            if (productShop.ProductShopTypeId < 1)
                return BadRequest(new GeneralReponse() { OperationSuccess = false, Message = "Selezionare una tipolgia del prodotto." });
            if (productShop.ProductShopHeaderPrice < 1)
                return BadRequest(new GeneralReponse() { OperationSuccess = false, Message = "Indicare un prezzo per l'articolo." });
            if (productShop.Rows == null || !productShop.Rows.Any())
                return BadRequest(new GeneralReponse() { OperationSuccess = false, Message = "Selezionare almeno un prodotto." });

            string imageExtension = "webp";
            string fileName = $"{productShop.ProductShopHeaderName.Replace(" ", "_")}_{productShop.ProductShopHeaderPrice}.{imageExtension}";
            ProductShopHeader productShopHeader = new ProductShopHeader()
            {
                ProductShopHeaderDescription = productShop.ProductShopHeaderDescription,
                ProductShopHeaderName = productShop.ProductShopHeaderName,
                ProductShopHeaderPrice = productShop.ProductShopHeaderPrice,
                ProductShopTypeId = productShop.ProductShopTypeId,
                ProductShopImagePath = String.IsNullOrEmpty(productShop.ProductShopBase64Image) ? $"{ftpAddress}/{fileName}" : ""
            };
           
            _db.Entry(productShopHeader).State = EntityState.Added;
            List<ProductShopRow> productShopRow = new List<ProductShopRow>();
            productShop.Rows.ForEach(x =>
            {
                productShopRow.Add(new ProductShopRow()
                {
                    ProductShopHeader = productShopHeader,
                    ProductId = x.ProductId,
                    ProductShopRowQuantity = x.ProductShopRowQuantity
                });
            });
            _db.ProductShopRow.AddRange(productShopRow);


            await _db.SaveChangesAsync();
            //if(!String.IsNullOrEmpty(productShop.ProductShopBase64Image))
            //    await HelperMethods.UploadFileToFtp(ftpAddress, ftpUser, ftpPassword, fileName, Convert.FromBase64String(productShop.ProductShopBase64Image.Split(",").Last()));

            return Ok();
        }

        [Authorize(Roles = RolesConstants.ROLE_CUSTOMER + "," + RolesConstants.ROLE_PR)]
        [HttpPost]
        [Route("Purchase")]
        public async Task<IActionResult> PurchaseProduct([FromQuery] int productId)
        {
            if (productId == 0)
                return BadRequest(new GeneralReponse() { OperationSuccess = false, Message = "Il prodotto non è valido" });

            if (string.IsNullOrEmpty(_user.Id))
                return BadRequest();    //TODO Handle not logged user request (postman case)

            //Only for check purposes
            ProductShopRow product = await _db.ProductShopRow.Include(pt => pt.Product.ProductShopType).Include(i => i.ProductShopHeader).FirstOrDefaultAsync(x => x.ProductShopHeaderId == productId);
            if (product == null)
                return NotFound(new GeneralReponse() { OperationSuccess = false, Message = "Il prodotto non è più disponibile." });

            if (_user.Points < product.ProductShopHeader.ProductShopHeaderPrice)
                return BadRequest(new GeneralReponse() { OperationSuccess = false, Message = "Saldo punti insufficiente." });

            

            bool okCode = false;
            int count = 0;
            //Handling ENTRY Award. Suppose that free entry is valid for 3 times i have to sent 3 different code to use them in different event
            while (!okCode)
            {                
                string code = HelperMethods.GenerateRandomString(6);
                if (!_db.UserProduct.Any(x => x.UserProductCode == code))
                {
                    UserProduct userProduct = new UserProduct()
                    {
                        UserId = _user.Id,
                        ProductShopHeaderId = productId,
                        UserProductUsed = false
                    };
                    userProduct.UserProductCode = code;
                    count++;
                    if (product.Product.ProductShopType.ProductShopTypeDescription == ProductShopTypeContants.PRODUCT_SHOP_TYPE_ENTRY)
                        okCode = true && count == product.ProductShopRowQuantity;
                    else
                        okCode = true;
                    _db.UserProduct.Add(userProduct);
                }
            }
           
            await _db.SaveChangesAsync();

            return Ok();
        }

        //// DELETE: api/ProductShop/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteProductShop(int id)
        //{
        //    var productShop = await _db.ProductShopHeader.FindAsync(id);
        //    if (productShop == null)
        //    {
        //        return NotFound();
        //    }

        //    _db.ProductShopHeader.Remove(productShop);
        //    await _db.SaveChangesAsync();

        //    return NoContent();
        //}

        private bool ProductShopExists(int id)
        {
            return false;// _db.ProductShopHeader.Any(e => e.ProductShopId == id);
        }
    }
}
