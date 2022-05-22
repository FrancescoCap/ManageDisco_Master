using IronBarCode;
using ManageDisco.Context;
using ManageDisco.Model;
using ManageDisco.Model.UserIdentity;
using ManageDisco.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;


namespace ManageDisco.Helper
{
    public class HelperMethods
    {
        public static readonly string NO_IMAGE_PHOTONAME = "no_image.webp";

        public static string GenerateRandomString(int length, bool withSpecialChars = true)
        {
            string chars = "";
            if (withSpecialChars)
                chars = "QWERTYUIOPLKJHGFDSAZXCVBNMqwertyuioplkjhgfdsazxcvbnmèàòùìé123456789";
            else
                chars = "QWERTYUIOPLKJHGFDSAZXCVBNMqwertyuioplkjhgfdsazxcvbnm123456789";

            StringBuilder stringBuilder = new StringBuilder();
            Random random = new Random();
            for (int c = 0; c < length; c++)
            {
                stringBuilder.Append(chars.ElementAt(random.Next(0, chars.Length)));
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
                expires: DateTime.UtcNow.AddMinutes(1),
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
                else if (value.GetType() == typeof(String))
                {
                    return "";
                }
            }
            return value;

        }

        internal static byte[] ConvertBitmapToByteArray(Bitmap imageStream)
        {
            using (var stream = new MemoryStream())
            {
                imageStream.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public static bool UserIsCustomer(UserRoles user)
        {
            return user.Roles.Any(x => x.Contains(RolesConstants.ROLE_CUSTOMER));
        }
        public static bool UserIsCustomer(List<string> roles)
        {
            return roles.Any(x => x.Contains(RolesConstants.ROLE_CUSTOMER));
        }

        public static bool UserIsAdministrator(UserRoles user)
        {
            return user.Roles.Any(x => x.Contains(RolesConstants.ROLE_ADMINISTRATOR));
        }

        public static bool UserIsAdministrator(List<string> roles)
        {
            return roles.Any(x => x.Contains(RolesConstants.ROLE_ADMINISTRATOR));
        }

        public static bool UserIsPrOrAdministrator(UserRoles user)
        {
            return user.Roles.Any(x => x.Contains(RolesConstants.ROLE_PR) || x.Contains(RolesConstants.ROLE_ADMINISTRATOR));
        }

        public static bool UserIsPrOrAdministrator(User user, List<string> roles)
        {
            return roles.Any(x => x.Contains(RolesConstants.ROLE_PR) || x.Contains(RolesConstants.ROLE_ADMINISTRATOR));
        }
        public static bool UserIsPr(User user, List<string> roles)
        {
            return roles.Any(x => x.Contains(RolesConstants.ROLE_PR));
        }

        public static bool UserIsPr(UserRoles user)
        {
            return user.Roles.Any(x => x.Contains(RolesConstants.ROLE_PR));
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

        public static bool UserIsInStaff(UserRoles user)
        {
            return user.Roles.Any(x => x.Contains(RolesConstants.ROLE_PR) ||
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

            return checkTask.Result;
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
            catch (Exception ex)
            {
                return null;
            }
        }

        public static byte[] GetBytesFromStream(Stream stream)
        {
            byte[] bytes;
            using (MemoryStream memory = new MemoryStream())
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
        public static string GetBase64DefaultNoImage(string defaultAddress, string ftpUser, string ftpPassword)
        {
            var photoStream = GetFileStreamToFtp($"{defaultAddress}/{NO_IMAGE_PHOTONAME}", ftpUser, ftpPassword);
            var photoBytes = GetBytesFromStream(photoStream);
            return Convert.ToBase64String(photoBytes);
        }
        /// <summary>
        /// Restituisce la stringa formattata in base64 di un'immagine
        /// </summary>
        /// <param name="defaultAddress">Indrizzo ftp con riferimento all'immagine</param>
        /// <returns></returns>
        public static string GetBase64Image(string defaultAddress, string ftpUser, string ftpPassword)
        {
            var photoStream = GetFileStreamToFtp($"{defaultAddress}", ftpUser, ftpPassword);
            var photoBytes = GetBytesFromStream(photoStream);
            return Convert.ToBase64String(photoBytes);
        }

        public static Task<CouponResponse> GenerateQrCodeCoupon(User user, string redirectUrl)
        {
            return Task<CouponResponse>.Run(() =>
            {
                try
                {
                    string link = redirectUrl;    //link che deve cliccare l'utente per recuperare il coupon

                    //link che deve leggere chi è incaricato alla scansione dei coupon per avere il redirect sulla pagina dell'utente per la validazione
                    GeneratedBarcode barcodeGenerator = IronBarCode.BarcodeWriter.CreateBarcode(String.Format($"{redirectUrl}&action=validate", user.Id), BarcodeEncoding.QRCode, 300, 300);
                    barcodeGenerator.AddAnnotationTextBelowBarcode(user.Name + " " + user.Surname);
                    barcodeGenerator.AddAnnotationTextAboveBarcode("Omaggio donna:");
                    barcodeGenerator.SaveAsImage(String.Format("{0}_coupon.webp", user.Id));


                    return new CouponResponse()
                    {
                        UserId = user.Id,
                        CouponValidated = false,
                        Link = link,
                        ImageStream = barcodeGenerator.ToBitmap()
                    };

                }
                catch (Exception ex)
                {
                    throw ex.InnerException;
                }
            });

        }
        public static string ParseDictionaryToFormData(Dictionary<string, string> values)
        {
            string formData = "";
            foreach (var value in values)
            {
                formData += $"{value.Key}={value.Value}&";
            }
            formData = formData.Substring(0, formData.Length - 1);
            return formData;
        }

        public async Task<AuthenticationResponse> GenerateTokens(DiscoContext db, User user, HttpContext context, UserManager<User> userManager, Encryption encryption, IConfiguration configuration)
        {           

            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, new Guid().ToString()));
            /**CUSTOM CLAIMS*/
            claims.Add(new Claim(CustomClaim.CLAIM_USERCODE, user.UserCode != null ? user.UserCode : ""));
            string userAgent = context.Request.Headers["User-Agent"];

            claims.Add(new Claim(CustomClaim.CLAIM_USERAGENT, userAgent));

            var userRoles = await userManager.GetRolesAsync(user);
            foreach (string role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            //token
            var token = HelperMethods.GenerateJwtToken(claims, configuration["Jwt:SecurityKey"], configuration["Jwt:ValidIssuer"], configuration["Jwt:ValidAudience"]);
            token = encryption.EncryptCookie(token, "cookieAuth");

            string clientSession = HelperMethods.GenerateRandomString(15, false);
            string oldClientSession = context.Session.GetString(CookieService.CLIENT_SESSION);
            string session = !String.IsNullOrEmpty(oldClientSession) ? oldClientSession : clientSession;

            var refreshToken = await GenerateRefreshTokn(db, 468, user.Id, session);

            AuthenticationResponse response = new AuthenticationResponse();
            response.Token = token;
            response.RefreshToken = refreshToken.RefreshTokenValue;
            response.ClientSession = session;

            return response;
        }

        private async Task<RefreshToken> GenerateRefreshTokn(DiscoContext db, int length, string userId, string clientSession, bool withSpecialChars = true)
        {
            RefreshToken refreshToken = new RefreshToken()
            {
                RefreshTokenValue = HelperMethods.GenerateRandomString(length, withSpecialChars),
                RefreshTokenLifetime = DateTime.Now.ToUniversalTime().AddMonths(1).Ticks,
                RefreshTokenUserId = userId,
                RefreshTokenIsValid = true,
                RefreshTokenClientSession = clientSession
            };

            db.RefreshToken.Add(refreshToken);
            await db.SaveChangesAsync();

            return refreshToken;
        }
    }
}
