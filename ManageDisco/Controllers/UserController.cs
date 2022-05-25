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
        CookieService _cookieService;

        public UserController(DiscoContext db,
            UserManager<User> userManager,
            IConfiguration configuration,
            TwilioService twilioService,
            Encryption encryption,
            SignInManager<User> sign,
            CookieService cookieService)
        {
            _db = db;
            _userManager = userManager;
            _configuration = configuration;
            _twilioService = twilioService;
            _encryption = encryption;
            _signManager = sign;
            _cookieService = cookieService;
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
            claims.Add(new Claim(CustomClaim.CLAIM_USERCODE, user.UserCode != null ? user.UserCode : ""));

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (string role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            } 

            response = await new HelperMethods().GenerateTokens(_db, user, HttpContext, _userManager, _encryption, _configuration);
            response.UserPoints = user.Points;
            response.UserNameSurname = String.Format("{0} {1}", user.Name, user.Surname);

            if (HelperMethods.UserIsCustomer((List<string>)userRoles))
            {
                string linkedPrId = _db.PrCustomer.FirstOrDefault(x => x.PrCustomerCustomerid == user.Id).PrCustomerPrId;
                var findPrTask = await _db.Users.FirstOrDefaultAsync(x => x.Id == linkedPrId);
                var prCode = findPrTask.UserCode;
                response.PrCode = prCode;
            }

            //delete old cookie if user login wihout logout operation           
            areCookiesToAdd(false, (List<string>)userRoles);

            string pr_ref = "";
            bool auth_type_full = false;
            bool auth_type_standard = false;
            if (HelperMethods.UserIsPrOrAdministrator(user, (List<string>)userRoles))
            {

                auth_type_standard = true;
                if (HelperMethods.UserIsAdministrator((List<string>)userRoles))
                {
                    auth_type_full = true;
                }

            }
            else if (HelperMethods.UserIsCustomer((List<string>)userRoles))
            {
                var prId = _db.PrCustomer.FirstOrDefaultAsync(x => x.PrCustomerCustomerid == user.Id).Result.PrCustomerPrId;
                pr_ref = _db.Users.FirstOrDefaultAsync(x => x.Id == prId).Result.UserCode;

            }

            Dictionary<string, string> cookiesValues = new Dictionary<string, string>();
            cookiesValues.Add(CookieService.AUTHORIZATION_COOKIE, response.Token);
            cookiesValues.Add(CookieService.REFRESH_COOKIE, response.RefreshToken);
            cookiesValues.Add(CookieService.AUTH_FULL_COOKIE, auth_type_full ? "1" : "0");
            cookiesValues.Add(CookieService.PR_REF_COOKIE, pr_ref);
            cookiesValues.Add(CookieService.AUTH_STANDARD_COOKIE, auth_type_standard ? "1" : "0");
            cookiesValues.Add(CookieService.CLIENT_SESSION, response.ClientSession);
            cookiesValues.Add(CookieService.ISAUTH_COOKIE, "1");

            areCookiesToAdd(true, (List<string>)userRoles, cookiesValues);

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
                
        [HttpPost]
        [Route("Register/Admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterRequest registerRequest)
        {
            if (registerRequest == null ||
                registerRequest.Email == null || 
                registerRequest.Password == null ||
                registerRequest.PhoneNumber == null)
                return BadRequest();

            User newUser = new User() { 
                Name = registerRequest.Name,
                Surname = registerRequest.Surname,
                Email = registerRequest.Email,
                Gender = registerRequest.Gender,
                PhoneNumber = registerRequest.PhoneNumber,
                UserName = registerRequest.Username
            };
            

            var registrationResult = _userManager.CreateAsync(newUser, registerRequest.Password);
            if (!registrationResult.Result.Succeeded)
                return BadRequest(registrationResult.Result.Errors);

           await _userManager.AddToRoleAsync(newUser, RolesConstants.ROLE_ADMINISTRATOR);


            return Ok();
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("Profile")]
        public async Task<IActionResult> GetUserInfo()
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            if (userId == null)
                return BadRequest();

            User user = _db.Users.FirstOrDefault(x => x.Id == userId);
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

        /// <summary>
        /// Handle profile page view. Client have to know if is customer to hide other menu options
        /// </summary>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("ProfilePageView")]
        public async Task<IActionResult> GetPageTypeView()
        {
            var user = await _userManager.FindByIdAsync(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);          
            var userRoles = _userManager.GetRolesAsync(user).Result;

            return Ok(HelperMethods.UserIsCustomer((List<string>) userRoles));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("NewCollaboratorInfo")]
        public async Task<IActionResult> GetAddingCollabortorInfo()
        {
            var user = await _userManager.FindByIdAsync(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
            bool isApiAllowed = _db.UserPermission.Any(x => x.UserId == user.Id &&
                     x.PermissionAction.PermissionActionDescription == PermissionValueCostants.PERMISSION_NEW_COLLABORATOR &&
                     x.PermissionActionAllowed == true);

            var userRoles = _userManager.GetRolesAsync(user).Result;
            //Roles which user can add collaborator
            var enabledNewCollabatorRoles = await _db.Roles.Where(x => x.Name != RolesConstants.ROLE_CUSTOMER).ToListAsync();
            //filter roles
            if (HelperMethods.UserIsPr((List<string>)userRoles))
            {
                enabledNewCollabatorRoles = enabledNewCollabatorRoles.Where(x => x.Name == RolesConstants.ROLE_PR).ToList();
            }
              

            if (!isApiAllowed && !HelperMethods.UserIsAdministrator((List<string>) userRoles))
                return Forbid();

            NewCollaboratorInfo newCollaboratorInfo = new NewCollaboratorInfo()
            {
                UserCanAddCollaborator = isApiAllowed,
                Roles = enabledNewCollabatorRoles
            };

            return Ok(newCollaboratorInfo);
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
                .Select(x => new ReservationView()
                {
                    ReservationId = x.ReservationId,
                    ReservationDate = x.ReservationDate,
                    ReservationName = x.ReservationTableName,
                    ReservationRealBudget = x.ReservationRealBudget,
                    ReservationExpectedBudget = x.ReservationExpectedBudget,
                    EventName = x.EventParty.Name,
                    UserId = HelperMethods.UserIsPrOrAdministrator(user, (List<string>)roles) ? x.UserId : x.UserIdOwner
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
            user.NormalizedEmail = userInfo.Email.ToUpper();

            if (!userInfo.PhoneNumber.StartsWith("+39"))
                userInfo.PhoneNumber = $"+39{userInfo.PhoneNumber}";

            bool sendConfirmationNumberMessage = false;
            if (user.PhoneNumber != userInfo.PhoneNumber)
            {
                sendConfirmationNumberMessage = true;
                user.PhoneNumber = userInfo.PhoneNumber;
            }


            _db.Entry(user).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            if (!user.PhoneNumberConfirmed && sendConfirmationNumberMessage)
                await SendPhoneNumberConfirmation(user.PhoneNumber);

            return Ok(new AuthenticationResponse() { Message = "I dati sono stati aggiornati.", OperationSuccess = true });
        }

        [HttpPost]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            var clientSession = HttpContext.Request.Cookies.FirstOrDefault(x => x.Key == CookieService.CLIENT_SESSION).Value;
            var userSession = await _db.RefreshToken.FirstOrDefaultAsync(x => x.RefreshTokenClientSession == clientSession);
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userSession.RefreshTokenUserId); ;


            areCookiesToAdd(false, (List<string>)await _userManager.GetRolesAsync(user));
            return Ok();
        }

        [HttpGet]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromQuery] string userId)
        {
            var refreshCookie = HttpContext.Request.Cookies.FirstOrDefault(x => x.Key == CookieService.REFRESH_COOKIE);
            var refreshTokenValue = refreshCookie.Value;
            if (String.IsNullOrEmpty(refreshTokenValue))
                return Unauthorized();

            string clientSession = "";
            if (_db.RefreshToken.Any(x => x.RefreshTokenValue == refreshTokenValue && x.RefreshTokenIsValid == true))
                clientSession = _db.RefreshToken.FirstOrDefault(x => x.RefreshTokenValue == refreshTokenValue && x.RefreshTokenIsValid == true).RefreshTokenClientSession;
            //Se entro qui significa che il client non mi ha restituito il client session --> qualcosa non va: blocco la chiamata
            //if (String.IsNullOrEmpty(clientSession))
            //    return BadRequest();

            //To avoid multiple refresh token generation check if it was already provided
            bool refreshTokenIsAlreadyProvided = _db.RefreshToken.Any(x => x.RefreshTokenClientSession == clientSession && x.RefreshTokenIsValid == true)
                && _db.RefreshToken.Where(x => x.RefreshTokenClientSession == clientSession).Count(x => x.RefreshTokenIsValid == true) > 1;
            RefreshToken refreshTokenOld = await _db.RefreshToken.FirstOrDefaultAsync(x => x.RefreshTokenValue == refreshTokenValue);

            if (!refreshTokenIsAlreadyProvided && refreshTokenOld.RefreshTokenIsValid)
            {
                //check if refresh token old exist 
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

                var tokens = await new HelperMethods().GenerateTokens(_db, user, HttpContext, _userManager, _encryption, _configuration);
                if (tokens == null)
                    return Ok();

                string pr_ref = HttpContext.Request.Cookies.Where(x => x.Key == CookieService.PR_REF_COOKIE).FirstOrDefault().Value;

                string auth_standard = HttpContext.Request.Cookies.Where(x => x.Key == CookieService.AUTH_STANDARD_COOKIE).FirstOrDefault().Value;

                var userRoles = await _userManager.GetRolesAsync(user);

                Dictionary<string, string> cookiesValues = new Dictionary<string, string>();
                cookiesValues.Add(CookieService.AUTHORIZATION_COOKIE, tokens.Token);
                cookiesValues.Add(CookieService.REFRESH_COOKIE, tokens.RefreshToken);
                cookiesValues.Add(CookieService.AUTH_FULL_COOKIE,
                    _cookieService.IsCookieEnabledForUser(CookieService.AUTH_FULL_COOKIE, (List<string>)userRoles) ? HttpContext.Request.Cookies.FirstOrDefault(x => x.Key == CookieService.AUTH_FULL_COOKIE).Value : "");
                cookiesValues.Add(CookieService.PR_REF_COOKIE, pr_ref);
                cookiesValues.Add(CookieService.AUTH_STANDARD_COOKIE,
                     _cookieService.IsCookieEnabledForUser(CookieService.AUTH_STANDARD_COOKIE, (List<string>)userRoles) ? HttpContext.Request.Cookies.FirstOrDefault(x => x.Key == CookieService.AUTH_STANDARD_COOKIE).Value : "");

                cookiesValues.Add(CookieService.CLIENT_SESSION, tokens.ClientSession);
                cookiesValues.Add(CookieService.ISAUTH_COOKIE, "1");

                areCookiesToAdd(true, (List<string>)userRoles, cookiesValues);
                return Ok();
            }


            return Unauthorized();
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
                return Ok(new GeneralReponse() { OperationSuccess = false, Message = "Il numero di telefono è stato già confermato" });
            if (user.Id != refer)
            {
                areCookiesToAdd(false, (List<string>)await _userManager.GetRolesAsync(user));
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

            Dictionary<string, string> formData = new Dictionary<string, string>();
            formData.Add(TwilioCommandResource.FIELD_FROM, "whatsapp:+14155238886");
            formData.Add(TwilioCommandResource.FIELD_TO, phoneNumber);
            formData.Add(TwilioCommandResource.FIELD_ACCOUNTSID, "AC85b726334a76001a55cd7de8ed7cd074");
            formData.Add(TwilioCommandResource.FIELD_BODY, TwilioCommandResource.SEND_PHONE_CONFIRM);
            await _twilioService.TriggerTwilio(formData);
        }

        private void areCookiesToAdd(bool add, List<string> roles, Dictionary<string, string> values = null)
        {

            foreach (Cookie cookie in _cookieService.cookies)
            {
                if (_cookieService.IsCookieEnabledForUser(cookie.Name, roles))
                {
                    if (add)
                    {
                        if (values != null)
                        {
                            if (values[cookie.Name] == null)
                                continue;

                            Response.Cookies.Append(cookie.Name, values[cookie.Name], new CookieOptions()
                            {
                                HttpOnly = cookie.HttpOnly,
                                SameSite = cookie.SameSite,
                                Secure = cookie.Secure,
                                Expires = cookie.Expires,
                                Domain = cookie.Domain
                            });
                        }
                    }
                    else
                        Response.Cookies.Delete(cookie.Name);
                }
            }
        }

    }
}
