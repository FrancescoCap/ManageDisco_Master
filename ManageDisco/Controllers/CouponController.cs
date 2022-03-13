﻿using ManageDisco.Context;
using ManageDisco.Helper;
using ManageDisco.Model;
using ManageDisco.Model.UserIdentity;
using ManageDisco.Resource;
using ManageDisco.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ManageDisco.Controllers
{
    [Route("api/[controller]")]    
    [ApiController]
    public class CouponController : BaseController
    {
        public CouponController(DiscoContext db, IConfiguration configuration, TwilioService twilioService) : base(db, configuration, twilioService)
        {
        }

        [HttpGet]
        public IActionResult GetCoupon([FromQuery] string refer)
        {
            refer = refer.Replace("\"", ""); //Perchè quando riceve il parametro aggiunge in automatico " in fondo?
            return Ok(new { value = HelperMethods.GetBase64Image($"{ftpAddress}/Coupons/{refer}_coupon.webp", ftpUser, ftpPassword) });
        }
       
        [HttpPost]
        [Route("Validate")]
        public async Task<IActionResult> ValidateCoupon([FromQuery] string couponUserId)
        {
            if (String.IsNullOrEmpty(couponUserId))
                return BadRequest();

           
            if (_user.Id == null)
                return BadRequest();

            Coupon newCoupon = await _db.Coupon.FirstOrDefaultAsync(x => x.UserId == couponUserId && x.CouponValidated == false);
            if (newCoupon == null)
                return Ok(new GeneralReponse() { OperationSuccess = false, Message = "Coupon già convalidato" });

            if (newCoupon.CouponValidated)
                return Ok(new GeneralReponse() { OperationSuccess = false, Message = "Coupon già convalidato" });

            newCoupon.CouponValidated = true;
            _db.Entry(newCoupon).State = EntityState.Modified;
            

            //get user information
            User userOwner = await GetUserInfoFromCoupon(couponUserId);
            if (userOwner == null)
                return BadRequest("Coupon già validato");

            await _db.SaveChangesAsync();

            return Ok(userOwner);
        }

        [HttpGet]
        [Route("Request")]
        public async Task<IActionResult> GetFreeEntrance([FromQuery] int eventId)
        {
            if (_user == null || _user.Id == null)
                return BadRequest();
            if (eventId == 0)
                return BadRequest();
            if (_user.Gender != GenderCostants.GENDER_FEMALE)
                return BadRequest();
            if (_db.Coupon.Any(x => x.UserId == _user.Id && x.EventId == eventId))
                return BadRequest(new GeneralReponse() { Message = "Hai già ricevuto un coupon per questo evento.", OperationSuccess = false});

            //TODO Fare tutti i controlli del caso
            //1. controllo che l'utente non abbia già un coupon
            //Coupon couponExist = await _db.Coupon.FirstOrDefaultAsync(x => x.UserId == _user.Id && x.EventId == eventId);
            //if (couponExist != null)
            //    return BadRequest();

            Coupon coupon = new Coupon()
            {
                UserId = _user.Id,
                EventId = eventId,
                CouponValidated = false
            };

            await _db.Coupon.AddAsync(coupon);
            await _db.SaveChangesAsync();

            await TriggerTwilio (_user.PhoneNumber);

            return Ok();
        }

        private async Task<User> GetUserInfoFromCoupon(string userId)
        {
            return await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
        }

        private async Task TriggerTwilio(string phoneNumber)
        {
            Dictionary<string, string> formValues = new Dictionary<string, string>();
            formValues.Add(TwilioCommandResource.FIELD_TO, $"whatsapp:{phoneNumber}");
            formValues.Add(TwilioCommandResource.FIELD_FROM, "whatsapp:+14155238886");
            formValues.Add(TwilioCommandResource.FIELD_BODY, TwilioCommandResource.SEND_COUPON);
            //formValues.Add(TwilioCommandResource.FIELD_ACCOUNTSID, "AC85b726334a76001a55cd7de8ed7cd074");

            await _twilioService.TriggerTwilio(formValues);
        }
    }
}
