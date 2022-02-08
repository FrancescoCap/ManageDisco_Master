using ManageDisco.Context;
using ManageDisco.Helper;
using ManageDisco.Model;
using ManageDisco.Model.UserIdentity;
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

        public UserController(DiscoContext db, 
            UserManager<User> userManager, 
            IConfiguration configuration)
        {
            _db = db;
            _userManager = userManager;
            _configuration = configuration;
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
                    Message = "No user found"
                };
                return Ok(response);
            }

            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, user.Name));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            claims.Add(new Claim(ClaimTypes.Surname, user.Surname));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, new Guid().ToString()));
            /**CUSTOM CLAIMS*/
            claims.Add(new Claim(CustomClaim.CLAIM_USERCODE, user.UserCode != null ? user.UserCode:""));
            claims.Add(new Claim(CustomClaim.CLAIM_USERNAME, user.UserName));

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (string role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            //token
            var token = HelperMethods.GenerateJwtToken(claims, _configuration["Jwt:SecurityKey"], _configuration["Jwt:ValidIssuer"], _configuration["Jwt:ValidAudience"]);

            response = new AuthenticationResponse();
            response.Token = token;
            response.RefreshToken = HelperMethods.GenerateRandomString(468);

            return Ok(response);

        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registrationInfo)
        {
            AuthenticationResponse authenticationResponse = new AuthenticationResponse();

            if (registrationInfo == null)
                return BadRequest();

            User newUser = new User()
            {
                Email = registrationInfo.Email,
                UserName = registrationInfo.Username,
                Name = registrationInfo.Name,
                Surname = registrationInfo.Surname
            };
            //Per la registrazione degli utenti dello staff (magazziniere, amministratori e pr) verrà creata un'apposita pagina con queryParam "type=staff" in cui verrà specificato il role
            //o in alternativa verrà chiamata a mano con i parametri che servono
            string roleName = registrationInfo.Role == 0 ? Enum.GetValues(typeof(RolesEnum)).GetValue(((int)RolesEnum.PR)).ToString() : 
                    Enum.GetValues(typeof(RolesEnum)).GetValue(registrationInfo.Role).ToString();

            if (_userManager.Users.Any(x => x.Email == registrationInfo.Email))
                return Ok("Email is already used.");

            if (registrationInfo.PrCode == null)
                return BadRequest("Insert Pr code.");

            newUser.UserCode = registrationInfo.PrCode;
            
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
                return Ok(authenticationResponse);
            }
            await _userManager.AddToRoleAsync(newUser, roleName);

            if (roleName == RolesConstants.ROLE_CUSTOMER)
            {
                string prId = _db.ReservationUserCode.FirstOrDefault(x => x.ReservationUserCodeValue == registrationInfo.PrCode).UserId;
                if (prId == String.Empty)
                    return BadRequest("No pr found for code.");

                _db.PrCustomer.Add(new PrCustomer()
                {
                    PrCustomerCustomerid = newUser.Id,
                    PrCustomerPrId = prId
                });
            }           

            await _db.SaveChangesAsync();

            //if (prId == String.Empty)
            //    return BadRequest("No PR found.");

            authenticationResponse.Message = "Registration success";

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
                UserEmail = user.Email
            };
            var roles = await _userManager.GetRolesAsync(user);
            userInfoView.IsCustomer = !HelperMethods.UserIsPrOrAdministrator(user, (List<string>)roles);

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

            _db.Entry(user).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return Ok();
        }
        
    }
}
