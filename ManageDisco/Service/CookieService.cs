using ManageDisco.Context;
using ManageDisco.Model.UserIdentity;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    public class CookieService
    {
        public const string AUTHORIZATION_COOKIE = "c_us";
        public const string REFRESH_COOKIE = "c_r";
        public const string ISAUTH_COOKIE = "c_in";
        public const string PR_REF_COOKIE = "p_r";
        public const string AUTH_FULL_COOKIE = "a_f";
        public const string AUTH_STANDARD_COOKIE = "a_s";
        public const string CLIENT_SESSION = ".AspNetCore.Session";

        private List<string> CookiesKeyList;

        private DiscoContext _db;
        public List<Cookie> cookies { get; private set; }

        public CookieService(DiscoContext db)
        {
            _db = db;
            initCookieList();
        }

        private void initCookieList()
        {
            cookies = _db.Cookies.ToList();
        }

        public List<string> GetCookiesKeyList()
        {
            return CookiesKeyList = new List<string>()
            {
                AUTHORIZATION_COOKIE,
                REFRESH_COOKIE,
                ISAUTH_COOKIE,
                PR_REF_COOKIE,
                AUTH_FULL_COOKIE,
                AUTH_STANDARD_COOKIE,
                CLIENT_SESSION
            };
        }

        /// <summary>
        /// True if cookie is enabled for user role
        /// </summary>
        /// <returns></returns>
        public bool IsCookieEnabledForUser(string cookie, List<string> roles)
        {
            bool isAuthorized = false;

            foreach(String r in roles)
            {
                var cookieInfo = _db.Cookies.FirstOrDefault(x => x.Name == cookie);
                if (cookieInfo.Roles.Contains("ALL") || cookieInfo.Roles.Contains(r))
                {
                    isAuthorized = true;
                    break;
                }
            }

            return isAuthorized;
        }
    }
}
