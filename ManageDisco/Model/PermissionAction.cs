using ManageDisco.Model.UserIdentity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{
    public static class PermissionActionList
    {
        private static string folderName = "Resource";
        private static string fileName = "PermissionAction.txt";
        public static List<PermissionAction> permissions;

        public static List<PermissionAction> GetToInsertPermissions(List<PermissionAction> existingPermission)
        {
            if (permissions == null)
                permissions = new List<PermissionAction>();
          
            FileStream fileStream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), folderName, fileName), FileMode.Open);
            if (fileStream == null)
                return null;

           using (var sr = new StreamReader(fileStream))
           {
                List<string> lines = new List<string>();
                string line = "";
                while((line = sr.ReadLine()) != null)
                {
                    lines.Add(line);
                }

                foreach (string permission in lines)
                {
                    var values = permission.Split("/");
                    if (values == null || values.Length == 0)
                        continue;

                    if (existingPermission.Any(x => x.PermissionActionDescription == values[0] && x.Path == values[2]))
                        continue;

                    permissions.Add(new PermissionAction()
                    {
                        PermissionActionDescription = values[0],
                        Methods = values[1],
                        Path = values[2]
                    });
                    
                }
           }
                     

            return permissions;
        }
    }

    public class PermissionAction
    {
        [Key]
        public int PermissionActionId { get; set; }
        public string PermissionActionDescription { get; set; }
        /// <summary>
        /// Path per cui è richiesto un particolare permesso
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Metodi del path per cui è richiesto un determinato permesso
        /// </summary>
        public string Methods { get; set; }
    }

}
