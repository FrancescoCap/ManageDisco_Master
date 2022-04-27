using ManageDisco.Context;
using ManageDisco.Model;
using ManageDisco.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManageDisco.Middleware
{
    public class Security
    {
        RequestDelegate _next;
        Encryption _encrypter;

        public Security(RequestDelegate next, Encryption encrypter)
        {
            _next = next;
            _encrypter = encrypter;
        }

        public async Task Invoke(HttpContext context, TokenService tokenService, DiscoContext db)
        {
            //var tokenValue = context.Request.Cookies.FirstOrDefault(x => x.Key == CookieConstants.AUTHORIZATION_COOKIE).Value;
            //var refreshTokenValue = context.Request.Cookies.FirstOrDefault(x => x.Key == CookieConstants.REFRESH_COOKIE).Value;
            //if (tokenValue != null && refreshTokenValue != null)
            //{
                
            //}



            //if (!context.Request.Path.Value.Contains("Login"))
            //{
            //    try
            //    {
            //        var tokenValue = context.Request.Cookies.FirstOrDefault(x => x.Key == CookieConstants.AUTHORIZATION_COOKIE).Value;
            //        var refreshTokenValue = context.Request.Cookies.FirstOrDefault(x => x.Key == CookieConstants.REFRESH_COOKIE).Value;

            //        var tokenInfoBase64 = _encrypter.DecryptCookie(tokenValue);

            //        var tokenVal = new JwtSecurityTokenHandler().ReadJwtToken(tokenInfoBase64);
            //        int expirationInSeconds = tokenVal.Payload.Exp.Value;
            //        DateTime expirationDate = new DateTime(1970, 1, 1, 0, 0, 0, 0); //parto dalla data da cui parte la UNIX epoch
            //        expirationDate = expirationDate.AddSeconds(expirationInSeconds);    //aggiungo i secondi passati che corrispondono alla data di scadenza del token
            //        var date = expirationDate.ToLocalTime(); //metto in chiaro la data

            //        if (DateTime.Now.CompareTo(expirationDate) > 0)
            //        {
            //            if (DateTime.Now.Day > expirationDate.Day ||
            //                DateTime.Now.Hour > expirationDate.Hour ||
            //                (DateTime.Now.Minute - 1) >= expirationDate.Minute)
            //            {
            //                RefreshToken refreshToken = await db.RefreshToken.FirstOrDefaultAsync(x => x.RefreshTokenValue == refreshTokenValue);

            //                if (refreshToken != null)
            //                {
            //                    var sessionId = !String.IsNullOrEmpty(refreshToken.RefreshTokenClientSession) ?
            //                   refreshToken.RefreshTokenClientSession :
            //                   context.Session.GetString(CookieConstants.CLIENT_SESSION);

            //                    if (await tokenService.ValidateRefreshToken(refreshTokenValue, sessionId))
            //                    {
            //                        context.Request.Path = "/api/User/RefreshToken";
            //                    }
            //                    else
            //                    {
            //                        await _next.EndInvoke(null);
            //                    }
            //                }

            //            }

            //        }

            //    }
            //    catch (Exception ex)
            //    {
            //        string s = ex.Message;
            //    }
            //}                       

            await _next.Invoke(context);
        }
    }
}
