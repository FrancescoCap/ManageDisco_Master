using Microsoft.Extensions.Configuration;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Twilio.Rest.Api.V2010.Account;

namespace ManageDisco.Service
{
    public class TwilioService
    {
        IConfiguration _conf;
        string endpoint = "";

        public TwilioService(IConfiguration config)
        {
            _conf = config;
            endpoint = $"{_conf["Twilio:UrlCallback"]}/api/Whatsapp";
        }
        /// <summary>
        /// Starts an auto-call for Twilio endpoint emulating message receveing from user
        /// </summary>
        private Task EmulateMessageReceived(Dictionary<string, string> formData)
        {
            return Task.Run(async() =>
            {
                if (formData == null)
                    throw new NullReferenceException();

                HttpClient client = new HttpClient();
                HttpContent body = new FormUrlEncodedContent(formData);
                await body.LoadIntoBufferAsync();
                var postOperation = await client.PostAsync(endpoint, body);
                return postOperation;
            });              
            
        }

        public async Task TriggerTwilio(Dictionary<string, string> formData)
        {           
            await EmulateMessageReceived(formData);
        }

        public void StartTwilioResponse(string from, string to, string body)
        {
            Twilio.TwilioClient.Init(_conf["Twilio:AccountId"], _conf["Twilio:Token"]);

            var message = MessageResource.Create(                
                from: new Twilio.Types.PhoneNumber(SanitizeNumber(from)),
                to: new Twilio.Types.PhoneNumber(SanitizeNumber(to)),
                body: body);           
        }

        public void StartTwilioResponse(string from, string to, string body, List<Uri> mediaUrl)
        {
            Twilio.TwilioClient.Init(_conf["Twilio:AccountId"], _conf["Twilio:Token"]);

            var message = MessageResource.Create(
                mediaUrl: mediaUrl,
                from: new Twilio.Types.PhoneNumber(SanitizeNumber(from)),
                to: new Twilio.Types.PhoneNumber(SanitizeNumber(to)),
                body: body);
        }

        private string SanitizeNumber(string value)
        {            
            string sanitizedNumber = !value.Contains("+") ? $"whatsapp:+39{value.Replace("whatsapp:", "")}" : value;
            return sanitizedNumber;
        }

    }
}
