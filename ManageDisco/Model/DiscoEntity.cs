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
        
    }
}
