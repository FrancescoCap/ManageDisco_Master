using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    public class Cookie
    {
        [Key]
        public int CookieId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Domain { get; set; }
        public string? Path { get; set; }
        public DateTimeOffset? Expires { get; set; }
        public bool Secure { get; set; }
        public SameSiteMode SameSite { get; set; }
        public bool HttpOnly { get; set; }
        public TimeSpan? MaxAge { get; set; }      
        public bool IsEssential { get; set; }
        /// <summary>
        /// Concatenato da Pipe (|) contiene i ruoli degli utenti che riceveranno il tipo di cookie
        /// </summary>
        public string Roles { get; set; }
    }
}
