using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    public class RefreshToken
    {
        public int RefreshTokenId { get; set; }
        public string RefreshTokenValue { get; set; }
        public string RefreshTokenUserId { get; set; }
        public string RefreshTokenClientSession { get; set; }
        public bool RefreshTokenIsValid { get; set; }
        public Int64 RefreshTokenLifetime { get; set; }
        public string RefreskTokenUserAgent { get; set; }
    }
}
