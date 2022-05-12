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
                //bool appendCookieFromClosedBrowser = context.Request.Cookies.Any(x => x.Key == CookieConstants.REFRESH_COOKIE) && !context.Request.Cookies.Any(x => x.Key == CookieConstants.AUTHORIZATION_COOKIE);
                //if (appendCookieFromClosedBrowser)
                //{
                //    var refreshToken = context.Request.Cookies.FirstOrDefault(x => x.Key == CookieConstants.REFRESH_COOKIE);
                //    AuthenticationResponse newTokens = await AttachNewTokens(db, context, userManager, _encryption, refreshToken.Value);
                //    context.Response.Cookies.Append(CookieConstants.AUTHORIZATION_COOKIE, newTokens.Token);
                //    context.Response.Cookies.Append(CookieConstants.REFRESH_COOKIE, newTokens.RefreshToken);
                //}              

                var cookie = context.Request.Path == "/api/User/Login" || 
                    //new customer registration
                    (context.Request.Path == "/api/User/Register" && !context.Request.Cookies.Any(x => x.Key == CookieConstants.AUTHORIZATION_COOKIE)) ? null : _encryption.DecryptCookie(context.Request.Cookies[CookieConstants.AUTHORIZATION_COOKIE]);

                if (cookie != null)
                {
                    var jwtInfo = new JwtSecurityTokenHandler().ReadJwtToken(cookie);
                    var tokenExpirationDateString = jwtInfo.Claims.FirstOrDefault(x => x.Type == CustomClaim.CLAIM_EXPIRATIONDATE).Value;
                    var tokenExpirationDateMilliseconds = double.Parse(tokenExpirationDateString);
                    ////if token is expired, redirect to general API
                    //if (jwtInfo.ValidTo.CompareTo(DateTime.UtcNow) < 0) //expired
                    //{
                    //    var userId = jwtInfo.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
                    //    var refreshToken = db.RefreshToken.FirstOrDefault(x => x.RefreshTokenUserId == userId && x.RefreshTokenIsValid == true).RefreshTokenLifetime;
                    //    var refreshTokenExpireDate = new DateTime(refreshToken);

                    //    if (refreshTokenExpireDate.CompareTo(DateTime.UtcNow) > 0)
                    //        context.Request.Path = "/api/User/RefreshToken";
                    //    else
                    //        await HandleNoValidToken(db, context);
                    //}
                    //else
                    //{
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
                    //Se entro qui singifica che l'utente è anonimo e non loggato
                    //Rendirizzo verso un endpoint per l'accesso anonimo.
                    //Per convenzione l'API dovrà avere un punto di access con path General
                    await HandleNoValidToken(db, context);                   
                }
            }          
           
           await _next.Invoke(context);
        }

       
        private async Task<IActionResult> HandleNoValidToken(DiscoContext db, HttpContext context)
        {
            string controller = context.Request.Path.Value.Split("/")[2];
            string path = context.Request.Path;
            AnonymusAllowed anonymus = await db.AnonymusAllowed.FirstOrDefaultAsync(x => x.Path == path && x.Controller == controller);

            if (anonymus != null)
            {
                context.Request.Path = anonymus.RedirectedPath;
                return new OkResult();
            }
            else
            {
                return new UnauthorizedResult();
            }
        }
    }
}
