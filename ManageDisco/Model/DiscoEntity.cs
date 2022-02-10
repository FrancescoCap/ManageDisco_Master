using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    /// <summary>
    /// Rappresenta la discoteca
    /// </summary>
    public class DiscoEntity
    {
        [Key]
        public string DiscoId { get; set; }
        public string DiscoName { get; set; }
        public string DiscoVatCode { get; set; }
        public string DiscoAddress { get; set; }
        public string DiscoCity { get; set; }
        public string DiscoProvince { get; set; }
        public string DiscoCityCap { get; set; }
        public string DiscoOpeningTime { get; set; }
        public string DiscoClosingTime { get; set; }

        
    }
}
