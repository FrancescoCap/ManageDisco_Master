using ManageDisco.Context;
using ManageDisco.Helper;
using ManageDisco.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ManageDisco.Middleware
{
    public class ExceptionHandlerMiddleware : IMiddleware
    {
        private readonly DiscoContext db;

        public ExceptionHandlerMiddleware(DiscoContext db)
        {
            this.db = db;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (Exception e)
            {
                Log rowLog = new Log()
                {
                    ErrorMessage = e.Message,
                    ErrorStacktrace = e.StackTrace,
                    ErrorPath = context.Request.Path,
                    ErrorDate = DateTime.Now.ToString()
                };
                await db.Log.AddAsync(rowLog);
                await db.SaveChangesAsync();
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync("Errore interno del server.");
            }
        }
    }
}
