using ManageDisco.Model.Error;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Helper
{
    public class ExceptionHelper : DiscoException
    {
        private const string FTP_UPLAOD_ERROR = "Si è verificato un errore durante il carimento dell'immagine sul server";

        public override string OnFtpUploadError(string message)
        {
            return FTP_UPLAOD_ERROR;
        }
    }
}
