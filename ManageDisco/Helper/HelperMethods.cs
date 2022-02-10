using ManageDisco.Model;
using ManageDisco.Model.UserIdentity;
using Microsoft.Extensions.Configuration;
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
        public static readonly string NO_IMAGE_PHOTONAME = "no_image.webp";

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

        public static Object HandleNullValue<T>(T value)
        {
            if (value == null)
            {
                if (value.GetType() == typeof(Int16) ||
                    value.GetType() == typeof(Int32) ||
                    value.GetType() == typeof(Int64))
                {
                    return 0;
                }
                else if(value.GetType() == typeof(String))
                {
                    return "";
                }
            }
            return value;

        }

        public static bool UserIdCustomer(UserRoles user)
        {
            return user.Roles.Any(x => x.Contains(RolesConstants.ROLE_CUSTOMER));
        }
        public static bool UserIsAdministrator(UserRoles user)
        {
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
        /// <summary>
        /// True se l'utente è un membro dello staff (pr, admin, magazziniere....)
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public static bool UserIsInStaff(User user, List<string> roles)
        {
            return roles.Any(x => x.Contains(RolesConstants.ROLE_PR) ||
                x.Contains(RolesConstants.ROLE_ADMINISTRATOR) ||
                x.Contains(RolesConstants.ROLE_WAREHOUSE_WORKER));
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

        public static Task DeleteFileFromFtp(string address, string user, string password, string filename)
        {
            return Task.Run(() =>
            {
                try
                {
                    FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create($"{address}/{filename}");
                    ftpWebRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                    ftpWebRequest.Credentials = new NetworkCredential(user, password);

                    var response = ftpWebRequest.GetResponse();

                }
                catch (Exception ex)
                {

                }
            });

        }

        public static bool CheckFileFromFtp(string address, string user, string password, string filename)
        {
            Task<bool> checkTask = Task.Run(() =>
              {
                  bool exist = false;
                  try
                  {
                      FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create($"{address}");
                      ftpWebRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                      ftpWebRequest.Credentials = new NetworkCredential(user, password);

                      var responseStream = ftpWebRequest.GetResponse().GetResponseStream();
                      StreamReader reader = new StreamReader(responseStream);
                      string resultString = reader.ReadToEnd();
                      exist = resultString.Contains(filename);
                      
                  }
                  catch (Exception ex)
                  {
                     
                  }
                  return exist;
              });

            return  checkTask.Result;
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

            }
            catch(Exception ex)
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
        /// <summary>
        /// Restituisce la stringa formattata in base64 dell'immagine no_image.webp
        /// </summary>
        /// <returns></returns>
        public static string GetBase64NoImage(string defaultAddress, string ftpUser, string ftpPassword)
        {
            var photoStream = GetFileStreamToFtp($"{defaultAddress}/{NO_IMAGE_PHOTONAME}", ftpUser, ftpPassword);
            var photoBytes = GetBytesFromStream(photoStream);
            return Convert.ToBase64String(photoBytes);
        }
    }
}
