using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    /// <summary>
    /// Table which contains all allowed anonymus path
    /// </summary>
    public class AnonymusAllowed
    {
        public int Id { get; set; }
        public string Controller { get; set; }
        public string Path { get; set; }
        public string RedirectedPath { get; set; }

        public Dictionary<string, List<string>> GetAnonymusPaths()
        {
            Dictionary<string, List<string>> controllers = new Dictionary<string, List<string>>();
            controllers.Add("Home", new List<string>{ "/api/Home/Info/General"});
            controllers.Add("Menu", new List<string> { "/api/Menu/General" });
            controllers.Add("ProductShop", new List<string> { "/api/ProductShop" });
            controllers.Add("ProductShopTypes", new List<string> { "/api/ProductShopTypes" });
            controllers.Add("EventParties", new List<string> { "/api/EventParties", "/api/EventParties/Details/General" });
            controllers.Add("Products", new List<string> { "/api/Products" });
            controllers.Add("Catalogs", new List<string> { "/api/Catalogs" });
            controllers.Add("User", new List<string> { "/api/User/Login" });
            

            return controllers;
        }

    }

}
