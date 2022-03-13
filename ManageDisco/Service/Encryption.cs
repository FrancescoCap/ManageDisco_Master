using ManageDisco.Model;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ManageDisco.Service
{
    public class Encryption
    {
        IConfiguration _conf;
        private IDataProtector _protector;

        public Encryption(IConfiguration configuration, IDataProtectionProvider protector)
        {
            _protector = (IDataProtector) protector;
            _conf = configuration;
        }

        public string EncryptCookie(string valueToEncrypt, string scope)
        {
            if (String.IsNullOrEmpty(valueToEncrypt))
                throw new NullReferenceException();

            _protector.CreateProtector(scope);
            string valueEncrypted = _protector.Protect(valueToEncrypt);
            return valueEncrypted;

        }
    }
}
