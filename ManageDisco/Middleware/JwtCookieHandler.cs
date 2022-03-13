using ManageDisco.Model.UserIdentity;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ManageDisco.Middleware
{
    public class JwtCookieHandler
    {

        private readonly RequestDelegate _next;

        public JwtCookieHandler(RequestDelegate next)
        {
            _next = next;

        }

        public async Task Invoke(HttpContext context)
        {
            var cookie = context.Request.Cookies["_auth"];
            if (cookie != null)
            {
               
                context.Request.Headers.Append("Authorization", "Bearer " + cookie);
            }
            else
            {
                //Se entro qui singifica che l'utente è anonimo e non loggato
                //Rendirizzo verso un endpoint per l'accesso anonimo.
                //Per convenzione l'API dovrà avere un punto di access con path General

                //Per API Login e Register non devo reindirizzare il path
                bool isLoginRequest = context.Request.Path.Value.Contains("Login") || context.Request.Path.Value.Contains("Register");
                bool isWhatsappRequest = context.Request.Path.Value.Contains("Whatsapp");
                bool isCouponRequest = context.Request.Path.Value.Contains("Coupon");
                context.Request.Path = !isLoginRequest && !isWhatsappRequest && !isCouponRequest ? $"{context.Request.Path}/General": context.Request.Path;
            }           

           await _next.Invoke(context);
        }
    }
}
