using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{

    public class HeaderMenu
    {
        public string Header { get; set; }      
        public string Link { get; set; }
        public List<MenuChild> child { get; set; }
    }

    public class MenuChild
    {
        public string Title { get; set; }
        public string Link { get; set; }
    }

}
