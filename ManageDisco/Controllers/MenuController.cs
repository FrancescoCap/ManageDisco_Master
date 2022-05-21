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
using System.Security.Claims;
using System.Threading.Tasks;

namespace ManageDisco.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : BaseController
    {
        public MenuController(DiscoContext db) : base(db)
        {
        }        
       
     
        [HttpGet]
        public async Task<IActionResult> GetMenu([FromQuery]bool isAnonymus)
        {
            List<HeaderMenu> menu = new List<HeaderMenu>();
            #region commonMenu
            HeaderMenu home = new HeaderMenu()
            {
                Header = "Home",
                Link = "/Home",
                Icon = "fa fa-home"
            };
            HeaderMenu events = new HeaderMenu()
            {
                Header = "Eventi",
                Link = "/Events",
                Icon = "far fa-calendar-alt",
                child = new List<MenuChild>()
                {
                    new MenuChild()
                    {
                        Title = "Programmazione eventi",
                        Link = "/Events",
                        Icon = "far fa-calendar-alt"
                    }
                }                
            };

            HeaderMenu profile = new HeaderMenu()
            {
                Header = "Gestione generale",
                Link = "/MyProfile",
                Icon = "fas fa-cogs"                    
            };

            HeaderMenu catalog = new HeaderMenu()
            {
                Header = "Listino",
                Link = "/Product",
                Icon = "fas fa-list",
                child = new List<MenuChild>()
                {
                    new MenuChild()
                    {
                        Title = "Bottiglie",
                        Link = "/Product",
                        Icon = "fas fa-wine-bottle"
                    }
                }
            };

            HeaderMenu shop = new HeaderMenu()
            {
                Header = "Shop",
                Link = "Shop",
                Icon = "fa fa-shopping-cart"
            };

            menu.Add(home);
            menu.Add(events);
            menu.Add(shop);
            menu.Add(catalog);

            //if is an anonymus user, menu above is enough
            if (isAnonymus)
                return Ok(menu);

            #region Customer only option
            if (!HelperMethods.UserIsInStaff(_user))
            {
                HeaderMenu customerReservations = new HeaderMenu()
                {
                    Header = "Prenotazioni",
                    Link = "/Reservation",
                    Icon = "fas fa-receipt"
                };
                menu.Add(customerReservations);
            }
            #endregion
          
            #endregion

            #region PR & Administrator menu
            if (_user.Roles.Contains(RolesConstants.ROLE_ADMINISTRATOR) || _user.Roles.Contains(RolesConstants.ROLE_PR))
            {
                HeaderMenu m2 = new HeaderMenu()
                {
                    Header = "Prenotazioni",
                    Link = "/Reservation",
                    Icon = "fas fa-receipt",
                    child = new List<MenuChild>()
                    {
                        new MenuChild()
                        {
                            Title = "Elenco prenotazioni",
                            Link = "/Reservation",
                            Icon = "fas fa-receipt"
                        },
                        new MenuChild()
                        {
                            Title = "Gestione tavoli",
                            Link = "/Table",
                            Icon = "fas fa-clipboard"
                        }
                    }
                };
                menu.Add(m2);

                HeaderMenu m3 = new HeaderMenu()
                {
                    Header = "Strumenti",
                    Link = "/Payments",
                    Icon = "fas fa-toolbox",
                    child = new List<MenuChild>()
                    {
                        new MenuChild()
                        {
                            Title = "Pagamenti",
                            Link = "/Payments",
                            Icon = "fas fa-cash-register"
                        }
                    }
                };
                if (HelperMethods.UserIsAdministrator(_user))
                {
                    m3.child.Add(new MenuChild()
                    {
                        Title = "Home",
                        Link = "/HomeSettings",
                        Icon = "fas fa-hammer"
                    });                   
                }
                menu.Add(m3);
            }
            #endregion

            #region WAREHOUSEWORKER menu
            if (_user.Roles.Contains(RolesConstants.ROLE_WAREHOUSE_WORKER) || _user.UserCanHandleWarehouse)
            {
                HeaderMenu order = new HeaderMenu()
                {
                    Header = "Ordini",
                    Link = "#",
                    Icon = "fa fa-list",
                    child = new List<MenuChild>()
                {
                    new MenuChild()
                    {
                        Title = "Tavoli-bottiglie",
                        Link = "/TableOrder",
                        Icon = "fas fa-wine-bottle"
                    }
                }
                };
                HeaderMenu warehouse = new HeaderMenu()
                {
                    Header = "Magazzino",
                    Link = "/Warehouse",
                    Icon = "fa fa-exchange",
                    child = new List<MenuChild>()
                    {
                        new MenuChild()
                        {
                            Title = "Magazzino-bottiglie",
                            Link = "/Warehouse",
                            Icon = "fas fa-boxes"
                        }
                    }
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
