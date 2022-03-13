using ManageDisco.Context;
using ManageDisco.Helper;
using ManageDisco.Middleware;
using ManageDisco.Model;
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
using System.Text;
using System.Threading.Tasks;

namespace ManageDisco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        DiscoContext _db;
        UserManager<User> _userManager;
        IConfiguration _configuration;
        TwilioService _twilioService;
        Encryption _encryption;

        public UserController(DiscoContext db, 
            UserManager<User> userManager, 
            IConfiguration configuration,
            TwilioService twilioService,
            Encryption encryption)
        {
            _db = db;
            _userManager = userManager;
            _configuration = configuration;
            _twilioService = twilioService;
            _encryption = encryption;
        }

        [Authorize(Roles = RolesConstants.ROLE_ADMINISTRATOR, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("Roles")]
        public async Task<IActionResult> GetRoles()
        {
            if (!HttpContext.User.IsInRole(RolesConstants.ROLE_ADMINISTRATOR))
                return BadRequest();

            var roles = await _db.Roles.Where(x => x.Name != RolesConstants.ROLE_CUSTOMER).ToListAsync();

            return Ok(roles);
        }

        /// <summary>
        /// Api per recuperare le info di un utente associato al QrCode
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("General")]
        public async Task<IActionResult> GetUser([FromQuery] string userId)
        {
            if (String.IsNullOrEmpty(userId))
                return BadRequest();

            User user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return NotFound();



            return Ok(user);
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginInfo)
        {
            AuthenticationResponse response;

            if (loginInfo == null)
                return BadRequest();

            User user = await _userManager.FindByEmailAsync(loginInfo.Email);

            if (user == null)
            {
                //No user exist
                response = new AuthenticationResponse()
                {
                    Token = "",
                    RefreshToken = "",
                    Message = "Email o password errati"
                };
                return Ok(response);
            }

            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, user.Name));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            claims.Add(new Claim(ClaimTypes.Surname, user.Surname));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber != null ? user.PhoneNumber : "+39"));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, new Guid().ToString()));
            /**CUSTOM CLAIMS*/
            claims.Add(new Claim(CustomClaim.CLAIM_USERCODE, user.UserCode != null ? user.UserCode:""));
            claims.Add(new Claim(CustomClaim.CLAIM_USERNAME, user.UserName));       
            claims.Add(new Claim(CustomClaim.CLAIM_GENDER, user.Gender));       
            
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (string role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            //token
            var token = HelperMethods.GenerateJwtToken(claims, _configuration["Jwt:SecurityKey"], _configuration["Jwt:ValidIssuer"], _configuration["Jwt:ValidAudience"]);
            token = _encryption.EncryptCookie(token, "cookieAuth");
            response = new AuthenticationResponse();
            response.Token = token;
            response.RefreshToken = HelperMethods.GenerateRandomString(468);

            Response.Cookies.Append("_auth", token, new CookieOptions() { HttpOnly = true,  SameSite = SameSiteMode.Strict });
            Response.Cookies.Append("isAuth", "1", new CookieOptions() { SameSite = SameSiteMode.Strict }); 
            if (HelperMethods.UserIsPrOrAdministrator(user, (List<string>)userRoles))
            {
                Response.Cookies.Append("authorization", "True", new CookieOptions() { SameSite = SameSiteMode.Strict });
                if (HelperMethods.UserIsAdministrator((List<string>)userRoles))
                    Response.Cookies.Append("authorization_full", "True", new CookieOptions() {  SameSite = SameSiteMode.Strict });

            }else if (HelperMethods.UserIsCustomer((List<string>)userRoles))
            {
                var prId = _db.PrCustomer.FirstOrDefaultAsync(x => x.PrCustomerCustomerid == user.Id).Result.PrCustomerPrId;
                var prCode = _db.Users.FirstOrDefaultAsync(x => x.Id == prId).Result.UserCode;
                Response.Cookies.Append("pr_ref", prCode, new CookieOptions() { SameSite = SameSiteMode.Strict });
            }

            return Ok(response);
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registrationInfo)
        {
            AuthenticationResponse authenticationResponse = new AuthenticationResponse();

            if (registrationInfo == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest();           

            User newUser = new User()
            {
                Email = registrationInfo.Email,
                UserName = registrationInfo.Username,
                Name = registrationInfo.Name,
                Surname = registrationInfo.Surname,
                PhoneNumber = registrationInfo.PhoneNumber,
                Gender = registrationInfo.Gender
                
            };

            //Per la registrazione degli utenti dello staff (magazziniere, amministratori e pr) verrà creata un'apposita pagina con queryParam "type=staff" in cui verrà specificato il role
            //o in alternativa verrà chiamata a mano con i parametri che servono
            string roleName = registrationInfo.Role == 0 ? Enum.GetValues(typeof(RolesEnum)).GetValue(((int)RolesEnum.PR)).ToString() : 
                    Enum.GetValues(typeof(RolesEnum)).GetValue(registrationInfo.Role).ToString();

            if (_userManager.Users.Any(x => x.Email == registrationInfo.Email))
            {
                authenticationResponse.Token = "";
                authenticationResponse.RefreshToken = "";
                authenticationResponse.Message = "L'email è già registrata.";
                authenticationResponse.OperationSuccess = false;
                return Ok(authenticationResponse);
            }
                

            if (String.IsNullOrEmpty(registrationInfo.PrCode) && roleName == RolesConstants.ROLE_CUSTOMER)
            {
                authenticationResponse.Token = "";
                authenticationResponse.RefreshToken = "";
                authenticationResponse.Message = "Codice PR non valido.";
                authenticationResponse.OperationSuccess = false;
                return Ok(authenticationResponse);
            }              

            if (roleName == RolesConstants.ROLE_ADMINISTRATOR || roleName == RolesConstants.ROLE_PR || roleName == RolesConstants.ROLE_WAREHOUSE_WORKER)
            {
                newUser.UserCode = HelperMethods.GenerateRandomString(6, false);
            }
           
            
            var isUserCreated = _userManager.CreateAsync(newUser, registrationInfo.Password);
            if (!isUserCreated.Result.Succeeded)
            {

                StringBuilder stringBuilder = new StringBuilder();
                foreach (var error in isUserCreated.Result.Errors.ToList())
                {
                    stringBuilder.AppendLine(error.Code + " " + error.Description);
                }
                authenticationResponse.Token = "";
                authenticationResponse.RefreshToken = "";
                authenticationResponse.Message = "Registration failed: " + stringBuilder.ToString();
                authenticationResponse.OperationSuccess = false;
                return Ok(authenticationResponse);
            }
            await _userManager.AddToRoleAsync(newUser, roleName);

            if (roleName == RolesConstants.ROLE_CUSTOMER)
            {
                var prId = _db.Users.FirstOrDefaultAsync(x => x.UserCode == registrationInfo.PrCode).Result.Id; // _db.ReservationUserCode.FirstOrDefault(x => x.ReservationUserCodeValue == registrationInfo.PrCode).UserId;
                if (prId == String.Empty)
                {
                    authenticationResponse.Token = "";
                    authenticationResponse.RefreshToken = "";
                    authenticationResponse.Message = "Il codice fornito non corrisponde ad un PR.";
                    authenticationResponse.OperationSuccess = false;
                    return Ok(authenticationResponse);
                }

                _db.PrCustomer.Add(new PrCustomer()
                {
                    PrCustomerCustomerid = newUser.Id,
                    PrCustomerPrId = prId
                });
            }           

            await _db.SaveChangesAsync();

            await SendPhoneNumberConfirmation(registrationInfo.PhoneNumber);

            authenticationResponse.Message = "Registration success";
            authenticationResponse.OperationSuccess = true;
            //Change to Redirect(loginPage)
            return Ok(authenticationResponse);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("Profile")]
        public async Task<IActionResult> GetUserInfo()
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            if (userId == null)
                return BadRequest();

            User user =  _db.Users.FirstOrDefault(x => x.Id == userId);
            if (user == null)
                return NotFound();

            UserInfoView userInfoView = new UserInfoView()
            {
                UserName = user.Name,
                UserSurname = user.Surname,
                UserEmail = user.Email,
                UserPhoneNumber = user.PhoneNumber,
                IsPhoneNumberConfirmed = user.PhoneNumberConfirmed
                
            };
            var roles = await _userManager.GetRolesAsync(user);
            userInfoView.IsCustomer = !HelperMethods.UserIsInStaff(user, roles.ToList());

            if (userInfoView.IsCustomer)
            {
                //get Pr
                var prId = _db.PrCustomer.FirstOrDefaultAsync(x => x.PrCustomerCustomerid == user.Id).Result.PrCustomerPrId;
                User pr = await _db.Users.FirstOrDefaultAsync(x => x.Id == prId);
                userInfoView.PrName = pr.Name;
                userInfoView.PrSurname = pr.Surname;
                userInfoView.PrEmail = pr.Email;
                userInfoView.PrCode = pr.UserCode;
            }


            return Ok(userInfoView);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("Reservation")]
        public async Task<IActionResult> GetUserReservation()
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            if (userId == null)
                return BadRequest();

            User user = _db.Users.FirstOrDefault(x => x.Id == userId);
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            IQueryable<ReservationView> reservations = _db.Reservation
                .Select(x => new ReservationView() { 
                    ReservationId = x.ReservationId,
                    ReservationDate = x.ReservationDate,
                    ReservationName = x.ReservationTableName,
                    ReservationRealBudget = x.ReservationRealBudget,
                    ReservationExpectedBudget = x.ReservationExpectedBudget,
                    EventName = x.EventParty.Name,
                    UserId = HelperMethods.UserIsPrOrAdministrator(user, (List<string>) roles) ? x.UserId : x.UserIdOwner
                }).Where(x => x.UserId == user.Id); 


            return Ok(await reservations.ToListAsync());
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut]
        [Route("Edit")]
        public async Task<IActionResult> PutUserInfo([FromBody] User userInfo)
        {
            if (userInfo == null)
                return BadRequest();

            var userId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

            User user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return NotFound();

            user.Name = userInfo.Name;
            user.Surname = userInfo.Surname;
            user.Email = userInfo.Email;

            if (!userInfo.PhoneNumber.StartsWith("+39"))
                userInfo.PhoneNumber = $"+39{userInfo.PhoneNumber}";

            user.PhoneNumber = userInfo.PhoneNumber;

            _db.Entry(user).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            if (!user.PhoneNumberConfirmed)
                await SendPhoneNumberConfirmation(user.PhoneNumber);

            return Ok(new AuthenticationResponse() { Message = "I dati sono stati aggiornati.", OperationSuccess = true});
        }

        [HttpPost]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {

            Response.Cookies.Delete("_auth");
            Response.Cookies.Delete("isAuth");
            Response.Cookies.Delete("authorization");
            Response.Cookies.Delete("authorization_full");
            Response.Cookies.Delete("pr_ref");

            return Ok();
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("ConfirmPhoneNumber")]
        public async Task<IActionResult> ConfirmUserPhone([FromQuery] string refer)
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            User user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return BadRequest();
            if (String.IsNullOrEmpty(user.Id))
                return BadRequest();
            if (user.PhoneNumberConfirmed)
                return Ok(new GeneralReponse() { OperationSuccess = false, Message =  "Il numero di telefono è stato già confermato"});
            if (user.Id != refer)
            {
                Response.Cookies.Delete("_auth");
                Response.Cookies.Delete("isAuth");
                Response.Cookies.Delete("authorization");
                Response.Cookies.Delete("authorization_full");
                Response.Cookies.Delete("pr_ref");
                return Ok(new GeneralReponse() { OperationSuccess = false, Message = "Il numero di telefono e l'account non corrispondono" });
            }
                

            user.PhoneNumberConfirmed = true;
            _db.Entry(user).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return Ok(new GeneralReponse() { OperationSuccess = true, Message = "Il numero di telefono è stato confermato" });
        }

        private async Task SendPhoneNumberConfirmation(string phoneNumber)
        {
            if (!phoneNumber.Contains("whatsap"))
                phoneNumber = $"whatsapp:{phoneNumber}";

            Dictionary <string, string > formData = new Dictionary<string,string>();
            formData.Add(TwilioCommandResource.FIELD_FROM, "whatsapp:+14155238886");
            formData.Add(TwilioCommandResource.FIELD_TO, phoneNumber);
            formData.Add(TwilioCommandResource.FIELD_ACCOUNTSID, "AC85b726334a76001a55cd7de8ed7cd074");
            formData.Add(TwilioCommandResource.FIELD_BODY, TwilioCommandResource.SEND_PHONE_CONFIRM);
            await _twilioService.TriggerTwilio(formData);
        }
        
    }
}
