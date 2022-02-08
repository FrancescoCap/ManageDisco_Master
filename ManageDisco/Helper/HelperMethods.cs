using ManageDisco.Model;
using ManageDisco.Model.UserIdentity;
using Microsoft.IdentityModel.Tokens;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ManageDisco.Helper
{
    public class HelperMethods
    {
        public static string GenerateRandomString(int length)
        {
            string chars = "QWERTYUIOPLKJHGFDSAZXCVBNMqwertyuioplkjhgfdsazxcvbnmèàòùìé123456789";
            StringBuilder stringBuilder = new StringBuilder();
            Random random = new Random();
            for(int c = 0; c < length; c++)
            {
                stringBuilder.Append(chars.ElementAt(random.Next(0,chars.Length)));
            }
            return stringBuilder.ToString();
        }

        public static string GenerateJwtToken(List<Claim> claims, string securityKey, string issuer, string audience)
        {
            var signingCredentials = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
            var credentials = new SigningCredentials(signingCredentials, SecurityAlgorithms.HmacSha256);
            
            var token = new JwtSecurityToken(                
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: null,
                expires: DateTime.Now.AddMinutes(59),
                signingCredentials: credentials);


            return new JwtSecurityTokenHandler().WriteToken(token);
            
        }

        public static Dictionary<string, List<MenuChild>> GetCommonMenu()
        {
            Dictionary<string, List<MenuChild>> menu = new Dictionary<string, List<MenuChild>>();
            menu.Add("Settings", new List<MenuChild>() {
                new MenuChild(){ Title="Account", Link =""},
                new MenuChild(){ Title="Logout", Link =""}
            });
            return menu;
        }

        public static Object HandleNullValue<T>(T value)
        {
            if (value == null)
            {
                if (value.GetType() == typeof(Int16) ||
                    value.GetType() == typeof(Int32) ||
                    value.GetType() == typeof(Int64))
                {
                    return 0;
                }else if(value.GetType() == typeof(String)){
                    return "";
                }
            }
            return value;
                
        }

        public static bool UserIdCustomer(UserRoles user)
        {
            return user.Roles.Any(x => x.Contains(RolesConstants.ROLE_CUSTOMER));
        }
        public static bool UserIsAdministrator(UserRoles user) {
            return user.Roles.Any(x => x.Contains(RolesConstants.ROLE_ADMINISTRATOR));
        }

        public static bool UserIsPrOrAdministrator(UserRoles user)
        {
            return user.Roles.Any(x => x.Contains(RolesConstants.ROLE_PR) || x.Contains(RolesConstants.ROLE_ADMINISTRATOR));
        }

        public static bool UserIsPrOrAdministrator(User user, List<string> roles)
        {
            return roles.Any(x => x.Contains(RolesConstants.ROLE_PR) || x.Contains(RolesConstants.ROLE_ADMINISTRATOR));
        }

        public static Task UploadFileToFtp(string address, string user, string password, string fileName, byte[] fileContent)
        {
            return Task.Run((() =>
            {
                try
                {
                    FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create($"{address}/{fileName}");
                    ftpWebRequest.UseBinary = true;
                    ftpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;
                    ftpWebRequest.Credentials = new NetworkCredential(user, password);

                    Stream ftpStream = ftpWebRequest.GetRequestStream();
                    ftpStream.Write(fileContent, 0, fileContent.Length);
                    ftpStream.Close();

                }
                catch (Exception ex)
                {

                }
            }));          
            
        }

        public static Stream GetFileStreamToFtp(string address, string user, string password)
        {
            try
            {
                FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(address);
                ftpWebRequest.UseBinary = true;
                ftpWebRequest.Credentials = new NetworkCredential(user, password);
                ftpWebRequest.Method = WebRequestMethods.Ftp.DownloadFile;

                return ftpWebRequest.GetResponse().GetResponseStream();
                
            }catch(Exception ex)
            {
                return null;
            }
        }

        public static byte[] GetBytesFromStream(Stream stream)
        {
            byte[] bytes;
            using(MemoryStream memory = new MemoryStream())
            {
                stream.CopyTo(memory);
                bytes = memory.ToArray();
            }

            return bytes;
        }
    }
}
