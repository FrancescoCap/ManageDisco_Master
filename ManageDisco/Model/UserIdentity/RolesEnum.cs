using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model.UserIdentity
{
    public enum RolesEnum
    {
        PR,
        WAREHOUSE_WORKER,
        ADMINISTRATOR,
        CUSTOMER
    }

    public class RolesConstants
    {
        public const string ROLE_PR = "PR";
        public const string ROLE_ADMINISTRATOR = "ADMINISTRATOR";
        public const string ROLE_WAREHOUSE_WORKER = "WAREHOUSE_WORKER";
        public const string ROLE_CUSTOMER = "CUSTOMER";
    }

    public class CustomClaim
    {
        public const string CLAIM_USERCODE = "UserCode";
        public const string CLAIM_USERNAME = "Username";
        public const string CLAIM_GENDER = "Gender";
        public const string CLAIM_USERAGENT = "UserAgent";
    }

    public class GenderCostants
    {
        public const string GENDER_MALE = "Male";
        public const string GENDER_FEMALE = "Female";        
    }

    public class ProductShopTypeCostants
    {
        public const string PRODUCT_TYPE_ENTRANCE = "INGRESSO";
        public const string PRODUCT_TYPE_TABLE = "TAVOLO";
        public const string PRODUCT_TYPE_PRODUCT = "PRODOTTO";
    }

    public class PermissionValueCostants
    {
        public const string PERMISSION_EVENT = "Gestione eventi";
        public const string PERMISSION_HOME_TEMPLATE = "Gestione template home";
        public const string PERMISSION_WAREHOUSE = "Gestione magazzino";
    }
}
