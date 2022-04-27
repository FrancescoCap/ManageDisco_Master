using ManageDisco.Context;
using ManageDisco.Model.UserIdentity;
using ManageDisco.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
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
        protected UserManager<User> _userManager;

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
        public BaseController(DiscoContext db, IConfiguration configuration, TwilioService twilioService, UserManager<User> userManager)
        {
            _db = db;
            _configuration = configuration;
            _twilioService = twilioService;
            _userManager = userManager;

            ftpAddress = configuration["Ftp:Address"];
            ftpUser = configuration["Ftp:User"];
            ftpPassword = configuration["Ftp:Pass"];
        }

        public BaseController(DiscoContext db, IConfiguration configuration, UserManager<User> userManager)
        {
            _db = db;
            _configuration = configuration;
            _userManager = userManager;

            ftpAddress = configuration["Ftp:Address"];
            ftpUser = configuration["Ftp:User"];
            ftpPassword = configuration["Ftp:Pass"];
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
                string id = claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
                string userCode = claims.Where(x => x.Type == CustomClaim.CLAIM_USERCODE).FirstOrDefault().Value;                
                string[] role = claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToArray();

                /** SENSIBLE DATA **/
                string name = GetSensibleUserInfo(id).Name;
                string username = GetSensibleUserInfo(id).UserName;
                string phoneNumber = GetSensibleUserInfo(id).PhoneNumber;
                string gender = GetSensibleUserInfo(id).Gender;

                /** OTHER DATA */
                decimal points = GetSensibleUserInfo(id).Points;

                _user.Id = id;
                _user.Email = email;
                _user.Name = name;
                _user.Roles.AddRange(role);
                _user.UserCode = userCode;
                _user.UserName = username;
                _user.PhoneNumber = phoneNumber;
                _user.Gender = gender;
                _user.Points = points;
                _user.UserCanHandleEvents = _db.UserPermission.Any(x => x.UserId == id && 
                    x.PermissionAction.PermissionActionDescription == PermissionValueCostants.PERMISSION_EVENT &&
                    x.PermissionActionAllowed == true);
                _user.UserCanHandleHomeTemplate = _db.UserPermission.Any(x => x.UserId == id && 
                    x.PermissionAction.PermissionActionDescription == PermissionValueCostants.PERMISSION_HOME_TEMPLATE &&
                    x.PermissionActionAllowed == true);
                _user.UserCanHandleWarehouse = _db.UserPermission.Any(x => x.UserId == id && 
                    x.PermissionAction.PermissionActionDescription == PermissionValueCostants.PERMISSION_WAREHOUSE &&
                    x.PermissionActionAllowed == true);
            }           
            
        }

        private User GetSensibleUserInfo(string id)
        {
            return _db.Users.FirstOrDefault(x => x.Id == id);
        }
    }
}
