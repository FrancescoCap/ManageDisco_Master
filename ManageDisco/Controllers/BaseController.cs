using ManageDisco.Context;
using ManageDisco.Model.UserIdentity;
using ManageDisco.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ManageDisco.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class BaseController : Controller
    {
        protected DiscoContext _db;
        protected UserRoles _user;
        protected IConfiguration _configuration;
        protected TwilioService _twilioService;

        protected readonly string ftpUser;
        protected readonly string ftpPassword;
        protected readonly string ftpAddress;

        //Facendomi passare lo UserManager nel costruttore ottengo errore ogni chiamata che faccio
        public BaseController(DiscoContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;

            ftpAddress = configuration["Ftp:Address"];
            ftpUser = configuration["Ftp:User"];
            ftpPassword = configuration["Ftp:Pass"];
        }

        public BaseController(DiscoContext db, IConfiguration configuration, TwilioService twilioService)
        {
            _db = db;
            _configuration = configuration;
            _twilioService = twilioService;


            ftpAddress = configuration["Ftp:Address"];
            ftpUser = configuration["Ftp:User"];
            ftpPassword = configuration["Ftp:Pass"];
        }

        public BaseController(DiscoContext db)
        {
            _db = db;
        }

        public async override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            await GetCaller();
        }

        private async Task GetCaller()
        {
            var claims = HttpContext.User.Claims;
            _user = new UserRoles();
            if (claims.Any())
            {
                string email = claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault().Value;
                string name = claims.Where(x => x.Type == ClaimTypes.Name).FirstOrDefault().Value;
                string id = claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
                string userCode = claims.Where(x => x.Type == CustomClaim.CLAIM_USERCODE).FirstOrDefault().Value;
                string username = claims.Where(x => x.Type == CustomClaim.CLAIM_USERNAME).FirstOrDefault().Value;
                string phoneNumber = claims.Where(x => x.Type == ClaimTypes.MobilePhone).FirstOrDefault().Value;
                string gender = claims.Where(x => x.Type == CustomClaim.CLAIM_GENDER).FirstOrDefault().Value;
                string[] role = claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToArray();
                
                _user.Id = id;
                _user.Email = email;
                _user.Name = name;
                _user.Roles.AddRange(role);
                _user.UserCode = userCode;
                _user.UserName = username;
                _user.PhoneNumber = phoneNumber;
                _user.Gender = gender;
            }           
            
        }
    }
}
