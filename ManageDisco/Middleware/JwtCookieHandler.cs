using ManageDisco.Context;
using ManageDisco.Controllers;
using ManageDisco.Helper;
using ManageDisco.Model;
using ManageDisco.Model.UserIdentity;
using ManageDisco.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ManageDisco.Middleware
{
    public class JwtCookieHandler
    {
        private Encryption _encryption;
       // private UserManager<User> _userManager;
        private readonly RequestDelegate _next;
        private IConfiguration _configuration;

        public JwtCookieHandler(RequestDelegate next, 
            Encryption encryption, 
            IConfiguration configuration
            )
        {
            _next = next;
            _encryption = encryption;
            this._configuration = configuration;
           //this._userManager = userManager;
        }

        public async Task Invoke(HttpContext context, DiscoContext db, UserManager<User> userManager)
        {
            if (context.Request.Method != HttpMethods.Options)
            {
                if (context.Request.Path.Value.Contains("/api/User/Login"))
                {
                    context.Session.Clear();
                }          

                var cookie = context.Request.Path == "/api/User/Login" || 
                    //new customer registration
                    (context.Request.Path == "/api/User/Register" && !context.Request.Cookies.Any(x => x.Key == CookieService.AUTHORIZATION_COOKIE)) ? null : _encryption.DecryptCookie(context.Request.Cookies[CookieService.AUTHORIZATION_COOKIE]);

                if (cookie != null)
                {
                    var jwtInfo = new JwtSecurityTokenHandler().ReadJwtToken(cookie);
                    var tokenExpirationDateString = jwtInfo.Claims.FirstOrDefault(x => x.Type == CustomClaim.CLAIM_EXPIRATIONDATE).Value;
                    var tokenExpirationDateMilliseconds = double.Parse(tokenExpirationDateString);

                    //if token is used with another agent means that was copied
                    string userAgent = jwtInfo.Claims.FirstOrDefault(x => x.Type == CustomClaim.CLAIM_USERAGENT).Value;
                        string agentCaller = context.Request.Headers["User-Agent"];
                        /*
                        * RESTITUIRE UN'ECCEZIONE MIGLIORE
                        */
                        if (userAgent != agentCaller)
                            await _next.EndInvoke(null);

                        context.Request.Headers.Append("Authorization", "Bearer " + cookie);
                    //}
                    
                }
                else
                {
                    if (context.Request.Path.Value.Contains("RefreshToken"))
                    {
                        //if no cookie authorization is present and the path is RefreshToken not need to handle request. Result is always unauthorized
                        return;
                    }
                    else
                    {
                        //Se entro qui singifica che l'utente è anonimo e non loggato
                        //Rendirizzo verso un endpoint per l'accesso anonimo.
                        //Per convenzione l'API dovrà avere un punto di access con path General
                        await HandleNoValidToken(db, context);
                    }                           
                }
            }          
           
           await _next.Invoke(context);
        }

       
        private async Task<IActionResult> HandleNoValidToken(DiscoContext db, HttpContext context)
        {
            string path = context.Request.Path;
            if (String.IsNullOrEmpty(path) || path == "/")
                return new OkResult();  //home path. Doesn't need token check

            string controller = path.Split("/")[2];
            
            AnonymusAllowed anonymus = await db.AnonymusAllowed.FirstOrDefaultAsync(x => x.Path == path && x.Controller == controller);

            if (anonymus != null)
            {
                context.Request.Path = anonymus.RedirectedPath;
                context.Request.QueryString = context.Request.QueryString.Add("isAnonymus", "true");
                return new OkResult();
            }
            else
            {
                return new UnauthorizedResult();
            }
        }
    }
}
