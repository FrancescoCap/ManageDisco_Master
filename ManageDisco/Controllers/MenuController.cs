using ManageDisco.Context;
using ManageDisco.Helper;
using ManageDisco.Model;
using ManageDisco.Model.UserIdentity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : BaseController
    {
        public MenuController(DiscoContext db) : base(db)
        {
        }

        [HttpGet]
        //[Authorize(Roles = RolesManager.ROLE_ADMINISTRATOR + "," + RolesManager.ROLE_PR + "," + )]
        public async Task<IActionResult> GetMenu()
        {
            List<HeaderMenu> menu = new List<HeaderMenu>();
            #region commonMenu
            HeaderMenu home = new HeaderMenu()
            {
                Header = "Home",
                Link = "/Home"
            };
            HeaderMenu events = new HeaderMenu()
            {
                Header = "Eventi",
                Link = "/Events",
                child = new List<MenuChild>()
                {
                    new MenuChild()
                    {
                        Title = "Programmazione eventi",
                        Link = "/Events"
                    }
                }
            };

            HeaderMenu profile = new HeaderMenu()
            {
                Header = "Profilo",
                Link = "MyProfile",
                child = new List<MenuChild>()
                {
                    new MenuChild()
                    {
                        Title ="I miei dati",
                        Link = "MyProfile"
                    },
                    new MenuChild()
                    {
                         Title = "Le mie prenotazioni",
                         Link = "MyProfile/Reservation"
                    }
                }
            };

            HeaderMenu catalog = new HeaderMenu()
            {
                Header = "Listino",
                Link = "#",
                child = new List<MenuChild>()
                {
                    new MenuChild()
                    {
                        Title = "Bottiglie",
                        Link = "/Product"
                    }
                }
            };
            menu.Add(home);
            menu.Add(events);
            menu.Add(catalog);
            #endregion

            #region PR & Administrator menu
            if (_user.Roles.Contains(RolesConstants.ROLE_ADMINISTRATOR) || _user.Roles.Contains(RolesConstants.ROLE_PR))
            {
                HeaderMenu m2 = new HeaderMenu()
                {
                    Header = "Prenotazioni",
                    Link = "/Reservation",
                    child = new List<MenuChild>()
                    {
                        new MenuChild()
                        {
                            Title = "Elenco prenotazioni",
                            Link = "/Reservation"
                        },
                        new MenuChild()
                        {
                            Title = "Gestione tavoli",
                            Link = "/Table"
                        }
                    }
                };
                menu.Add(m2);

                HeaderMenu m3 = new HeaderMenu()
                {
                    Header = "Gestione generale",
                    Link = "/Payments",
                    child = new List<MenuChild>()
                    {
                        new MenuChild()
                        {
                            Title = "Pagamenti",
                            Link = "/Payments"
                        }
                    }
                };
                if (HelperMethods.UserIsAdministrator(_user))
                {
                    m3.child.Add(new MenuChild()
                    {
                        Title = "Home",
                        Link = "/HomeSettings"
                    });
                    m3.child.Add(new MenuChild()
                    {
                        Title = "Magazzino-bottiglie",
                        Link = "/Warehouse"
                    });
                }
                menu.Add(m3);
            }
            #endregion

            #region WAREHOUSEWORKER menu
            if (_user.Roles.Contains(RolesConstants.ROLE_WAREHOUSE_WORKER))
            {
                HeaderMenu order = new HeaderMenu()
                {
                    Header = "Ordini",
                    Link = "#",
                    child = new List<MenuChild>()
                {
                    new MenuChild()
                    {
                        Title = "Tavoli-bottiglie",
                        Link = "/TableOrder"
                    }
                }
                };
                HeaderMenu warehouse = new HeaderMenu()
                {
                    Header = "Magazzino",
                    Link = "/Warehouse"
                };
                menu.Add(order);
                menu.Add(warehouse);
            }

            #endregion


            menu.Add(profile);

            return Ok(menu);
        }
    }
}
