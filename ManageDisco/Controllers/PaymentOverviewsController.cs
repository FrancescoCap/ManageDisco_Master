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
using Microsoft.AspNetCore.Identity;

namespace ManageDisco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentOverviewsController : BaseController
    {
       
        public PaymentOverviewsController(DiscoContext db,
            UserManager<User> userManager) : base(db, userManager)
        {
            
        }

        /// <summary>
        /// Returns data useful for get payments to collaborator. Api used for mobile and desktop
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<PaymentsOverviewFull>> GetPaymentsOverview([FromQuery] string userId)
        {
            PaymentsOverviewFull paymentsData = new PaymentsOverviewFull();
            List<Task> tasks = new List<Task>()
            {
                Task.Run(async () => {

                    if (!HelperMethods.UserIsAdministrator(_user))
                        return;

                    paymentsData.Collaborators = (List<User>) await _userManager.GetUsersInRoleAsync(RolesConstants.ROLE_ADMINISTRATOR);
                    paymentsData.Collaborators.AddRange((List<User>) await _userManager.GetUsersInRoleAsync(RolesConstants.ROLE_PR));
                    paymentsData.Collaborators.AddRange((List<User>) await _userManager.GetUsersInRoleAsync(RolesConstants.ROLE_WAREHOUSE_WORKER));
                    //clean from unnecessary data
                    paymentsData.Collaborators.ForEach(x =>
                    {
                        x.Email = "";
                        x.PasswordHash = "";
                        x.PhoneNumber = null;
                        x.NormalizedEmail = "";
                        x.UserName = "";
                        x.NormalizedUserName = "";
                        x.Gender = "";
                        x.UserCode = "";
                        x.SecurityStamp = "";
                        x.ConcurrencyStamp = "";
                    });

                }),
                Task.Run(async () => {
                    
                        var tempList = _db.PaymentOverview
                        .Select(x => new ReservationPaymentView()
                        {
                            PaymentId = x.PaymentOverviewId,
                            TotalIncoming = x.TotalIncoming,
                            TotalPayed = x.TotalCreditPayed,
                            ResumeCredit = x.TotalCreditResume,
                            Name = x.User.Name,
                            Surname = x.User.Surname,
                            UserId = x.UserId                           
                        });

                        if (HelperMethods.UserIsAdministrator(_user))
                        {
                            if (!String.IsNullOrEmpty(userId))
                                tempList = tempList.Where(x => x.UserId == userId);
                        }else if (HelperMethods.UserIsPrOrAdministrator(_user))
                        {
                            tempList = tempList.Where(x => x.UserId == _user.Id);
                        }

                        paymentsData.PaymentsOverview =  await tempList.ToListAsync();
                })/*,
                Task.Run(() => {
                    
                        var tempList = _db.ReservationPayment
                        .Select(x => new ReservationPayment()
                        {
                            ReservationPaymentId = x.ReservationPaymentId,
                            ReservationPaymentAmount = x.ReservationPaymentAmount,
                            ReservationPaymentDescription = x.ReservationPaymentDescription,
                            ReservationPaymentDate = x.ReservationPaymentDate
                        });

                        if (HelperMethods.UserIsAdministrator(_user))
                        {
                            tempList = tempList.Where(x => x.UserId == userId);
                        }else if (HelperMethods.UserIsPrOrAdministrator(_user))
                        {
                            tempList = tempList.Where(x => x.UserId == _user.Id);
                        }                   
                
                })*/
            };

            tasks.ForEach(t =>
            {
                t.Wait(); 
            });

            return Ok(paymentsData);
        }

        // POST: api/PaymentOverviews
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PaymentOverview>> PostPaymentOverview(PaymentOverview paymentOverview)
        {
            _db.PaymentOverview.Add(paymentOverview);
            await _db.SaveChangesAsync();

            return CreatedAtAction("GetPaymentOverview", new { id = paymentOverview.PaymentOverviewId }, paymentOverview);
        }

        // DELETE: api/PaymentOverviews/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaymentOverview(int id)
        {
            var paymentOverview = await _db.PaymentOverview.FindAsync(id);
            if (paymentOverview == null)
            {
                return NotFound();
            }

            _db.PaymentOverview.Remove(paymentOverview);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
