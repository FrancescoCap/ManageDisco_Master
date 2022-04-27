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
using Microsoft.Extensions.Primitives;
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
        SignInManager<User> _signManager;
        IConfiguration _configuration;
        TwilioService _twilioService;
        Encryption _encryption;

        public UserController(DiscoContext db, 
            UserManager<User> userManager, 
            IConfiguration configuration,
            TwilioService twilioService,
            Encryption encryption,
            SignInManager<User> sign)
        {
            _db = db;
            _userManager = userManager;
            _configuration = configuration;
            _twilioService = twilioService;
            _encryption = encryption;
            _signManager = sign;
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

        [Authorize(Roles = RolesConstants.ROLE_ADMINISTRATOR, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("Collaborators")]
        public async Task<IActionResult> GetCollaborators()
        {
           var users = _userManager.GetUsersInRoleAsync(RolesConstants.ROLE_PR).Result.ToList();
            users.AddRange(_userManager.GetUsersInRoleAsync(RolesConstants.ROLE_WAREHOUSE_WORKER).Result.ToList());
            users.AddRange(_userManager.GetUsersInRoleAsync(RolesConstants.ROLE_ADMINISTRATOR).Result.ToList());

            return Ok(users);
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
                return BadRequest(response);
            }

            var passwordCheck = await _signManager.CheckPasswordSignInAsync(user, loginInfo.Password, true);
            if (!passwordCheck.Succeeded)
            {
                //Wrong password
                response = new AuthenticationResponse()
                {
                    Token = "",
                    RefreshToken = "",
                    Message = "Email o password errati"
                };
                return BadRequest(response);
            }

            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, new Guid().ToString()));
            /**CUSTOM CLAIMS*/
            claims.Add(new Claim(CustomClaim.CLAIM_USERCODE, user.UserCode != null ? user.UserCode:""));
            
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (string role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            //token
            //var token = HelperMethods.GenerateJwtToken(claims, _configuration["Jwt:SecurityKey"], _configuration["Jwt:ValidIssuer"], _configuration["Jwt:ValidAudience"]);
            //token = _encryption.EncryptCookie(token, "cookieAuth");      

            response = await GenerateTokens(user);

            //delete old cookie if user login wihout logout operation           
            areCookiesToAdd(false);
            areCookiesToAdd(true, response.Token, response.RefreshToken);
            if (HelperMethods.UserIsPrOrAdministrator(user, (List<string>)userRoles))
            {
                Response.Cookies.Append(CookieConstants.AUTH_STANDARD_COOKIE, "True", new CookieOptions() { SameSite = SameSiteMode.Strict });
                if (HelperMethods.UserIsAdministrator((List<string>)userRoles))
                    Response.Cookies.Append(CookieConstants.AUTH_FULL_COOKIE, "True", new CookieOptions() {  SameSite = SameSiteMode.Strict });

            }else if (HelperMethods.UserIsCustomer((List<string>)userRoles))
            {
                var prId = _db.PrCustomer.FirstOrDefaultAsync(x => x.PrCustomerCustomerid == user.Id).Result.PrCustomerPrId;
                var prCode = _db.Users.FirstOrDefaultAsync(x => x.Id == prId).Result.UserCode;
                Response.Cookies.Append(CookieConstants.PR_REF_COOKIE, prCode, new CookieOptions() { SameSite = SameSiteMode.Strict });
            }
           
            HttpContext.Session.SetString(CookieConstants.CLIENT_SESSION, response.ClientSession);

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
            /* IF ROLE IS NULL MEANS THAT IS A CUSTOMER REGISTRATION */
            string roleName = String.IsNullOrEmpty(registrationInfo.Role) ? _db.Roles.Where(x => x.Name == RolesConstants.ROLE_CUSTOMER).FirstOrDefault().Name : _db.Roles.Find(registrationInfo.Role).Name;

            if (_userManager.Users.Any(x => x.Email == registrationInfo.Email))
            {
                authenticationResponse.Token = "";
                authenticationResponse.RefreshToken = "";
                authenticationResponse.Message = "L'email è già registrata.";
                authenticationResponse.OperationSuccess = false;
                return BadRequest(authenticationResponse);
            }
            //if (_userManager.Users.Any(x => x.PhoneNumber.Replace("+39","") == registrationInfo.PhoneNumber))
            //{
            //    authenticationResponse.Token = "";
            //    authenticationResponse.RefreshToken = "";
            //    authenticationResponse.Message = "E' già presente un account associato al numero indicato.";
            //    authenticationResponse.OperationSuccess = false;
            //    return BadRequest(authenticationResponse);
            //}
                

            if (String.IsNullOrEmpty(registrationInfo.PrCode) && roleName == RolesConstants.ROLE_CUSTOMER)
            {
                authenticationResponse.Token = "";
                authenticationResponse.RefreshToken = "";
                authenticationResponse.Message = "Codice PR non valido.";
                authenticationResponse.OperationSuccess = false;
                return BadRequest(authenticationResponse);
            }              

            if (roleName == RolesConstants.ROLE_ADMINISTRATOR || roleName == RolesConstants.ROLE_PR || roleName == RolesConstants.ROLE_WAREHOUSE_WORKER)
            {
                newUser.UserCode = HelperMethods.GenerateRandomString(6, false);
            }

            if (roleName == RolesConstants.ROLE_CUSTOMER)
            {
                var prId = await _db.Users.FirstOrDefaultAsync(x => x.UserCode == registrationInfo.PrCode);

                if (prId == null || String.IsNullOrEmpty(prId.Id))
                {
                    authenticationResponse.Token = "";
                    authenticationResponse.RefreshToken = "";
                    authenticationResponse.Message = "Il codice fornito non corrisponde ad un PR.";
                    authenticationResponse.OperationSuccess = false;
                    return BadRequest(authenticationResponse);
                }

                _db.PrCustomer.Add(new PrCustomer()
                {
                    PrCustomerCustomerid = newUser.Id,
                    PrCustomerPrId = prId.Id
                });
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
                authenticationResponse.Message = "Registrazione fallita " + stringBuilder.ToString();
                authenticationResponse.OperationSuccess = false;
                return Ok(authenticationResponse);
            }

            await _userManager.AddToRoleAsync(newUser, roleName);                               

            await _db.SaveChangesAsync();

            await SendPhoneNumberConfirmation(registrationInfo.PhoneNumber);

            authenticationResponse.Message = "Registratione completata";
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
                UserPoints = user.Points,
                IsPhoneNumberConfirmed = user.PhoneNumberConfirmed

            };

            var roles = await _userManager.GetRolesAsync(user);
            userInfoView.IsCustomer = !HelperMethods.UserIsInStaff(user, roles.ToList());

            if (!userInfoView.IsCustomer) 
                userInfoView.InvitationLink = $"{_configuration["NgRok:Client"]}/SignUp?code={user.UserCode}";

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

            areCookiesToAdd(false);
            return Ok();
        }

        [HttpGet]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromQuery]string userId)
        {
            var refreshCookie = HttpContext.Request.Cookies.FirstOrDefault(x => x.Key == CookieConstants.REFRESH_COOKIE);
            var refreshTokenValue = refreshCookie.Value;
            var clientSession = Encoding.UTF8.GetString(HttpContext.Session.Get(CookieConstants.CLIENT_SESSION));
            //To avoid multiple refresh token generation check if it was already provided
            bool refreshTokenIsAlreadyProvided = _db.RefreshToken.Where(x => x.RefreshTokenClientSession == clientSession).Any(x => x.RefreshTokenIsValid == true)
                && _db.RefreshToken.Where(x => x.RefreshTokenClientSession == clientSession).Count(x => x.RefreshTokenIsValid == true) > 1;
            RefreshToken refreshTokenOld = await _db.RefreshToken.FirstOrDefaultAsync(x => x.RefreshTokenValue == refreshTokenValue);
            
            if (!refreshTokenIsAlreadyProvided && refreshTokenOld.RefreshTokenIsValid)
            {
                //check if refresh token exist 
                

                if (refreshTokenOld == null)
                    return BadRequest();


                //var lifetime = HttpContext.Session.GetString(CookieConstants.CLIENT_SESSION);
                //if (long.Parse(lifetime) != refreshTokenOld.RefreshTokenLifetime)
                //    return BadRequest();

                User user = await _userManager.FindByIdAsync(refreshTokenOld.RefreshTokenUserId);
                if (user == null)
                    return BadRequest();

                refreshTokenOld.RefreshTokenIsValid = false;
                _db.Entry(refreshTokenOld).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                var tokens = await GenerateTokens(user);
                areCookiesToAdd(true, tokens.Token, tokens.RefreshToken);
            }
            

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
        
        private void areCookiesToAdd(bool add, params string[] args)
        {
            //handle only global Cookies
            if (add)
            {
                Response.Cookies.Append(CookieConstants.AUTHORIZATION_COOKIE, args[0], new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.Strict });
                Response.Cookies.Append(CookieConstants.ISAUTH_COOKIE, "1", new CookieOptions() { SameSite = SameSiteMode.Strict });
                Response.Cookies.Append(CookieConstants.REFRESH_COOKIE, args[1], new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.Strict});
                //Response.Cookies.Append(CookieConstants.CLIENT_SESSION, args[2]);
            }
            else
            {
                Response.Cookies.Delete(CookieConstants.AUTHORIZATION_COOKIE);
                Response.Cookies.Delete(CookieConstants.ISAUTH_COOKIE);
                Response.Cookies.Delete(CookieConstants.AUTH_STANDARD_COOKIE);
                Response.Cookies.Delete(CookieConstants.AUTH_FULL_COOKIE);
                Response.Cookies.Delete(CookieConstants.PR_REF_COOKIE);
                Response.Cookies.Delete(CookieConstants.REFRESH_COOKIE);
                Response.Cookies.Delete(CookieConstants.CLIENT_SESSION);
            }
        }
        public async Task<RefreshToken> GenerateRefreshTokn(int length, string userId, string clientSession, bool withSpecialChars = true )
        {
            RefreshToken refreshToken = new RefreshToken()
            {
                RefreshTokenValue = HelperMethods.GenerateRandomString(length, withSpecialChars),
                RefreshTokenLifetime = DateTime.Now.ToUniversalTime().Ticks,
                RefreshTokenUserId = userId,
                RefreshTokenIsValid = true,
                RefreshTokenClientSession = clientSession
            };

            _db.RefreshToken.Add(refreshToken);
            await _db.SaveChangesAsync();

            return refreshToken;

        }      
        
        private async Task<AuthenticationResponse> GenerateTokens(User user)
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, new Guid().ToString()));            
            /**CUSTOM CLAIMS*/
            claims.Add(new Claim(CustomClaim.CLAIM_USERCODE, user.UserCode != null ? user.UserCode : ""));
            string userAgent = HttpContext.Request.Headers["User-Agent"];
            
            claims.Add(new Claim(CustomClaim.CLAIM_USERAGENT, userAgent));

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (string role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            //token
            var token = HelperMethods.GenerateJwtToken(claims, _configuration["Jwt:SecurityKey"], _configuration["Jwt:ValidIssuer"], _configuration["Jwt:ValidAudience"]);
            token = _encryption.EncryptCookie(token, "cookieAuth");

            string clientSession = HelperMethods.GenerateRandomString(15, false);
            string oldClientSession = HttpContext.Session.GetString(CookieConstants.CLIENT_SESSION); 
            string session = !String.IsNullOrEmpty(oldClientSession) ? oldClientSession : clientSession;

            var refreshToken = await GenerateRefreshTokn(468, user.Id, session);

            AuthenticationResponse response = new AuthenticationResponse();
            response.Token = token;
            response.RefreshToken = refreshToken.RefreshTokenValue;
            response.ClientSession = session;

            return response;
        }
    }
}
