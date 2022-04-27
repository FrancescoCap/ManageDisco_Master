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
using Microsoft.AspNetCore.Authorization;

namespace ManageDisco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController : BaseController
    {
        public ContactsController(DiscoContext db, IConfiguration configuration) : base(db, configuration)
        {
        }
         
        // GET: api/Contacts
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetContact()
        {
            List<ContactView> contact = await _db.Contact
                .Select(x => new ContactView()
                {
                    ContactDescription = x.ContactDescription,
                    ContactTypeDescription = x.ContactType.ContactTypeDescription,
                    ContactId = x.ContactId,
                    ContactTypeId = x.ContactTypeId
                }).ToListAsync();


            return  Ok(contact);
        }
       
        // PUT: api/Contacts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutContact(int id, Contact contact)
        {
            if (id != contact.ContactId)
            {
                return BadRequest();
            }

            _db.Entry(contact).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContactExists(id))
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

        // POST: api/Contacts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Contact>> PostContact(Contact contact)
        {
            if (contact == null)
                return BadRequest();

            if (contact.ContactTypeId == 0)
                return BadRequest("Invalid contact type.");

            if (String.IsNullOrEmpty(contact.ContactDescription))
                return BadRequest("Contact could not be null.");

            _db.Contact.Add(contact);
            await _db.SaveChangesAsync();

            return CreatedAtAction("GetContact", new { id = contact.ContactId }, contact);
        }

        // DELETE: api/Contacts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContact(int id)
        {
            var contact = await _db.Contact.FindAsync(id);
            if (contact == null)
            {
                return NotFound();
            }

            _db.Contact.Remove(contact);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private bool ContactExists(int id)
        {
            return _db.Contact.Any(e => e.ContactId == id);
        }
    }
}
