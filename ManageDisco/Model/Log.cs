using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    public class Log
    {
        [Key]
        public int Id { get; set; }
        public string ErrorPath { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorStacktrace { get; set; }
        public string ErrorDate { get; set; }
    }
}
