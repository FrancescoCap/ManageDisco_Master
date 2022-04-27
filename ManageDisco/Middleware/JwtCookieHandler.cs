using ManageDisco.Context;
using ManageDisco.Model;
using ManageDisco.Model.UserIdentity;
using ManageDisco.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ManageDisco.Middleware
{
    public class JwtCookieHandler
    {
        private Encryption _encryption;
        private readonly RequestDelegate _next;
        private DiscoContext _db;

        public JwtCookieHandler(RequestDelegate next, Encryption encryption)
        {
            _next = next;
            _encryption = encryption;
            //_db = db;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Method != HttpMethods.Options)
            {
                if (context.Request.Path == "/api/User/RefreshToken")
                {
                    //bool rftValid = await isRefreshTokenValid(context.Request.Cookies.FirstOrDefault(c => c.Key == CookieConstants.REFRESH_COOKIE).Value, context.Session.GetString(CookieConstants.CLIENT_SESSION));
                    //if (rftValid)
                    //{                        
                        
                        await _next.Invoke(context);
                    //}
                }                   

                var cookie = context.Request.Path == "/api/User/Login" || 
                    //new customer registration
                    (context.Request.Path == "/api/User/Register" && !context.Request.Cookies.Any(x => x.Key == CookieConstants.AUTHORIZATION_COOKIE)) ? null : _encryption.DecryptCookie(context.Request.Cookies[CookieConstants.AUTHORIZATION_COOKIE]);

                if (cookie != null)
                {
                    var jwtInfo = new JwtSecurityTokenHandler().ReadJwtToken(cookie);
                    //if token is used with another agent means that was copied 
                  
                    string userAgent = jwtInfo.Claims.FirstOrDefault(x => x.Type == CustomClaim.CLAIM_USERAGENT).Value;
                    string agentCaller = context.Request.Headers["User-Agent"];
                   /*
                   * RESTITUIRE UN'ECCEZIONE MIGLIORE
                   */
                    if (userAgent != agentCaller)
                        await _next.EndInvoke(null);

                    context.Request.Headers.Append("Authorization", "Bearer " + cookie);
                }
                else
                {
                    //Se entro qui singifica che l'utente è anonimo e non loggato
                    //Rendirizzo verso un endpoint per l'accesso anonimo.
                    //Per convenzione l'API dovrà avere un punto di access con path General

                    //Per alcune API non devo impostare il redirect
                    bool isLoginRequest = context.Request.Path.Value.Contains("Login") || context.Request.Path.Value.Contains("Register");
                    bool isWhatsappRequest = context.Request.Path.Value.Contains("Whatsapp");
                    bool isCouponRequest = context.Request.Path.Value.Contains("Coupon");
                    bool isContactRequest = context.Request.Path.Value.Contains("Info");
                    context.Request.Path = 
                        (!isLoginRequest &&     //need redirect
                        !isWhatsappRequest && 
                        !isCouponRequest &&
                        !isContactRequest) ? $"{context.Request.Path}/General" : context.Request.Path;
                }
            }          
           
           await _next.Invoke(context);
        }

        //public async Task<bool> isRefreshTokenValid(string refreshTokenValue, string sessionTime)
        //{
        //    //RefreshToken refreshToken = await _db.RefreshToken.FirstOrDefaultAsync(x => x.RefreshTokenValue == refreshTokenValue);
        //    //if (refreshToken == null)
        //    //    return false;

        //    long nowTicks = DateTime.Now.ToUniversalTime().Ticks;

        //    if ((nowTicks - refreshToken.RefreshTokenLifetime.ToUniversalTime().Ticks) == (nowTicks - long.Parse(sessionTime)))
        //        return true;

        //    return false;
        //}
    }
}
