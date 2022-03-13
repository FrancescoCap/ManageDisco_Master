using ManageDisco.Service;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ManageDisco.Middleware
{
    public class EncryptionMiddleware
    {
        private readonly RequestDelegate _next;
        Encryption _encryption;

        public EncryptionMiddleware(RequestDelegate next, Encryption encryption)
        {
            _next = next;
            _encryption = encryption;
        }

        public async Task Invoke(HttpContext context)
        {
            
            
           


            await _next.Invoke(context);
        }
    }
}
