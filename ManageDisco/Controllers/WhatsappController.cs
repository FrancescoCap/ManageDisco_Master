using ManageDisco.Context;
using ManageDisco.Helper;
using ManageDisco.Model.UserIdentity;
using ManageDisco.Resource;
using ManageDisco.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Twilio;
using Twilio.AspNet.Common;
using Twilio.AspNet.Core;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;
using Twilio.TwiML.Messaging;
using Twilio.Types;

namespace ManageDisco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class WhatsappController : TwilioController
    {
        DiscoContext _db;
        UserManager<User> _userManager;
        IConfiguration _conf;
        TwilioService _twilioService;

        public WhatsappController(DiscoContext db, UserManager<User> userManager, IConfiguration conf, TwilioService twilioService)
        {
            _db = db;
            _userManager = userManager;
            _conf = conf;
            _twilioService = twilioService;
        }

        [HttpPost]
        public TwiMLResult Index([FromForm] SmsRequest incomingMessage)
        {
           // var userPhone = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.MobilePhone).Value;
            
            var response = new MessagingResponse();
            
            if (!String.IsNullOrEmpty(incomingMessage.Body)){
                switch (incomingMessage.Body)
                {
                    case TwilioCommandResource.SEND_COUPON:
                        SendCoupon(incomingMessage.From, incomingMessage.To);
                        break;
                    case TwilioCommandResource.SEND_PHONE_CONFIRM:
                        SendPhoneNumberConfirmation(incomingMessage.From, incomingMessage.To);
                        break;
                    default:
                        break;
                }
            }
            
         
            return TwiML(response);
        }
        
        public void SendCoupon(string from, string to)
        {
            if (from == null || to == null)
                return;
             //await Task.Run(async () => {
                System.Threading.Thread.Sleep(1000);

                var number = to.Replace("whatsapp:", "");

                var user = _db.Users.FirstOrDefault(x => x.PhoneNumber == number);

                string couponUrl = $"{_conf["NgRok:Client"]}/Coupon?refer={user.Id}";
                var couponTask = HelperMethods.GenerateQrCodeCoupon(user,couponUrl).Result;

                var couponGenerationResponse = couponTask;

                var ftpAddress = _conf["Ftp:Address"];
                var ftpUsername = _conf["Ftp:User"];
                var ftpPassword = _conf["Ftp:Pass"];

                //HelperMethods.UploadFileToFtp(
                //    $"{ftpAddress}/Coupons",
                //    ftpUsername, ftpPassword,
                //    $"{couponGenerationResponse.UserId}_coupon.webp",
                //    HelperMethods.ConvertBitmapToByteArray(couponGenerationResponse.ImageStream));
            /*
             * L'inivio di immagini funziona. Questo è un URL di un'immagine casuale.
             * Quando andrò in produzione dovrò sostituirlo con l'indirizzo FTP associato al coupon
             */
            List<Uri> media = new List<Uri>()
            {
                new Uri("https://images.unsplash.com/photo-1545093149-618ce3bcf49d?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=crop&w=668&q=80")
            };
            //_twilioService.StartTwilioResponse(from, to, $"Ciao {user.Name} {user.Surname} sono Opium! Puoi trovare il coupon al seguente link: {couponGenerationResponse.Link.Replace("http://","")}");
            _twilioService.StartTwilioResponse(from, to, $"Ciao {user.Name} {user.Surname} sono Opium! Puoi trovare il coupon al seguente link: {couponGenerationResponse.Link.Replace("http://","")}", media);
          
        }

        private void SendPhoneNumberConfirmation(string from, string to)
        {
            var number = to.Replace("whatsapp:", "");
            var user = _db.Users.FirstOrDefault(x => x.PhoneNumber == number);
            string body = $"Ciao, clicca sul seguente link per confermare il tuo nr. di cellulare!\nlocalhost:4200/PhoneNumber?refer={user.Id}&action=phoneConfirm";
            
            _twilioService.StartTwilioResponse(from, to, body);
        }

       
    }
}
