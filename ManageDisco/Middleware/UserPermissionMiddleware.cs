using ManageDisco.Context;
using ManageDisco.Model;
using ManageDisco.Model.UserIdentity;
using ManageDisco.Service;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ManageDisco.Middleware
{
    public class UserPermissionMiddleware
    {
        RequestDelegate _next;
        Encryption _encryption;
        public UserPermissionMiddleware(RequestDelegate next, Encryption encryption)
        {
            _next = next;
            _encryption = encryption;
        }

        public async Task Invoke(HttpContext context, DiscoContext db)
        {
            string endpoint = context.Request.Path.Value.Split("/").Last();
            if (!context.Request.Path.Value.Contains("Login") &&
                !context.Request.Path.Value.Contains("Register") &&
                IsPathToCheck(endpoint, db))
            {
                PermissionAction action = GetPermissionForAction(endpoint, context.Request.Method, db);
                if (action == null)
                {
                    await _next.Invoke(context);
                }
                else
                {
                    var cookie = context.Request.Path == "/api/User/Login" ||
                   //new customer registration
                   (context.Request.Path == "/api/User/Register" && !context.Request.Cookies.Any(x => x.Key == CookieService.AUTHORIZATION_COOKIE)) ? null : _encryption.DecryptCookie(context.Request.Cookies[CookieService.AUTHORIZATION_COOKIE]);
                    
                    var jwtInfo = new JwtSecurityTokenHandler().ReadJwtToken(cookie);

                    string userId = jwtInfo.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
                    if (!IsUserEnabledForAction(userId, action.PermissionActionId, db))
                        await _next.EndInvoke(null);
                    else
                        await _next.Invoke(context);
                }
            }
            else
            {
                await _next.Invoke(context);
            }
           
        }

        private PermissionAction GetPermissionForAction(string path, string method, DiscoContext db)
        {
            PermissionAction permission = db.PermissionAction.FirstOrDefault(x => x.Path.Contains(path));           
            if (permission == null)
                return null;
            if (!permission.Methods.Split("|").Contains(method))
                return null;                        
           

            return permission;
        }

        private bool IsUserEnabledForAction(string userId, int permissionId, DiscoContext db)
        {
            if (String.IsNullOrEmpty(userId) || permissionId == 0)
                return false;

            return db.UserPermission.Any(x => x.PermissionActionId == permissionId && x.UserId == userId && x.PermissionActionAllowed == true);
        }

        private bool IsPathToCheck(string path, DiscoContext db)
        {
            return db.PermissionAction.Any(x => x.Path.Contains(path));
        }
    }
}
