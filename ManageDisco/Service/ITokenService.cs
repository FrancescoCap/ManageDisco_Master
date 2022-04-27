using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Service
{
    interface ITokenService
    {
        public bool isTokenValid(string value);
        public Task<bool> ValidateRefreshToken(string value, string sessionId);
    }
}
