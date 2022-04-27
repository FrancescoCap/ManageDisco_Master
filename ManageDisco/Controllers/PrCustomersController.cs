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
using ManageDisco.Helper;

namespace ManageDisco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrCustomersController : BaseController
    {
        public PrCustomersController(DiscoContext db) : base(db)
        {
        }


        // GET: api/PrCustomers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PrCustomerView>>> GetPrCustomer()
        {
            List<PrCustomer> prCustomers = await _db.PrCustomer.Where(x => x.PrCustomerPrId == _user.Id).ToListAsync();

            List<PrCustomerView> prCustomerViews = new List<PrCustomerView>();
            foreach(PrCustomer row in prCustomers)
            {
                PrCustomerView prCustomerView = await _db.Users
                     .Where(x => x.Id == row.PrCustomerCustomerid)
                     .Select(x => new PrCustomerView()
                     {
                         CustomerId = x.Id,
                         Name = x.Name,
                         Surname = x.Surname
                     }).FirstOrDefaultAsync();                

                prCustomerViews.Add(prCustomerView);
            }

            if (HelperMethods.UserIsInStaff(_user))
            {
                prCustomerViews.Add(new PrCustomerView()
                {
                    CustomerId = _user.Id,
                    Name = "Me stesso"
                });
            }          

            return prCustomerViews;
        }

        // GET: api/PrCustomers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PrCustomer>> GetPrCustomer(int id)
        {
            var prCustomer = await _db.PrCustomer.FindAsync(id);

            if (prCustomer == null)
            {
                return NotFound();
            }

            return prCustomer;
        }

        // PUT: api/PrCustomers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        public async Task<IActionResult> PutPrCustomer([FromQuery]string prCode)
        {
            if (prCode == "")
                return BadRequest();

            //get customer
            PrCustomer prCustomer = await _db.PrCustomer.FirstOrDefaultAsync(x => x.PrCustomerCustomerid == _user.Id);
            if (prCustomer == null)
                return BadRequest();

            //Get pr associated with new code
            User pr = await _db.Users.FirstOrDefaultAsync(x => x.UserCode == prCode);
            if (pr == null)
                return BadRequest(new GeneralReponse() { OperationSuccess = false, Message = "Nessun pr trovato con il codice fornito." });

            prCustomer.PrCustomerPrId = pr.Id;
            _db.Entry(prCustomer).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/PrCustomers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PrCustomer>> PostPrCustomer(PrCustomer prCustomer)
        {
            _db.PrCustomer.Add(prCustomer);
            await _db.SaveChangesAsync();

            return CreatedAtAction("GetPrCustomer", new { id = prCustomer.PrCustomerId }, prCustomer);
        }
    }
}
