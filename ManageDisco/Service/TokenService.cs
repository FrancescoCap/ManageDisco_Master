using ManageDisco.Context;
using ManageDisco.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Service
{
    public class TokenService : ITokenService
    {
        DiscoContext _db;

        public TokenService(DiscoContext db)
        {
            _db = db;
        }

        public bool isTokenValid(string value)
        {          


            return false;            
        }

        public async Task<bool> ValidateRefreshToken(string value, string sessionId)
        {
            if (value == "")
                return false;

            RefreshToken refreshTokenOld = await _db.RefreshToken.FirstOrDefaultAsync(x => x.RefreshTokenValue == value);

            if (refreshTokenOld == null)
                return false;

            if (sessionId != refreshTokenOld.RefreshTokenClientSession)
                return false;

            return true;
        }
    }
}
