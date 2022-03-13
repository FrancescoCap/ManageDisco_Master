using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Service
{
    public interface IEmailService
    {
        public void SendEmail();
        public void SetParams();
    }

    public class EmailService: IEmailService
    {
        string _domain = "";
        string _username = "";
        string _password = "";
        string _port = "";
        IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public void SendEmail()
        {
            throw new NotImplementedException();
        }

        public void SetParams()
        {
            throw new NotImplementedException();
        }
    }


}
