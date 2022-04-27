using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model.Error
{
    public abstract class DiscoException:Exception
    {       
        public abstract string OnFtpUploadError(string message);
    }
}
