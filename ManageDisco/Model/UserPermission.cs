using ManageDisco.Model.UserIdentity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    public class UserPermission
    {
        public int UserPermissionId { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public int PermissionActionId { get; set; }
        public PermissionAction PermissionAction { get; set; }        
        public bool PermissionActionAllowed { get; set; }
    }

    public class UserPermissionPut
    {
        public string UserId { get; set; }
        public int PermissionId { get; set; }
    }

    /****************** START TABLE OBJECTS *************************/
    public class UserPermissionTable
    {
        public List<string> UserPermissionTableHeaderCol { get; set; }
        public List<string> UserPermissionTableHeaderRow { get; set; }
        public List<UserPermissionRow> Rows { get; set; } = new List<UserPermissionRow>();
    }

    public class UserPermissionRow
    {
        public string User { get; set; }
        public List<UserPermissionCell> UserPermissionTableCell { get; set; } = new List<UserPermissionCell>();
    }

    public class UserPermissionCell
    {
        public string UserId { get; set; }
        public string UserIdentity { get; set; }
        public int PermissionId { get; set; }
        public bool PermissionState { get; set; }
    }

    /****************** END TABLE OBJECTS *************************/
}
