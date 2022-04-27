using ManageDisco.Context;
using ManageDisco.Model;
using ManageDisco.Model.UserIdentity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ManageDisco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserPermissionController : BaseController
    {
        public UserPermissionController(DiscoContext db, IConfiguration configuration) : base(db, configuration)
        {
        }

        [HttpGet]
        [Route("PermissionAction")]
        public async Task<IActionResult> GetPermissionAction()
        {
            List<PermissionAction> permissionAction = await _db.PermissionAction.ToListAsync();


            return Ok(permissionAction);
        }

        // GET: api/<UserPermission>
        [HttpGet]
        [Route("UserPermissionView")]
        public async Task<IActionResult> GetUserPermission()
        {
            List<PermissionAction> permissions = await _db.PermissionAction.ToListAsync();
            List<string> staffRoles = await _db.Roles
                .Where(x => x.Name == RolesConstants.ROLE_ADMINISTRATOR || x.Name == RolesConstants.ROLE_PR || x.Name == RolesConstants.ROLE_WAREHOUSE_WORKER)
                .Select(x => x.Id)
                .ToListAsync();

            var users = await _db.UserRoles.ToListAsync();

            var staff = users
                .Join(staffRoles, collaborator => collaborator.RoleId, role => role, (collaborator, role) => collaborator).ToList();

            UserPermissionTable userPermissionTable = new UserPermissionTable();
            userPermissionTable.UserPermissionTableHeaderCol = new List<string>();

            List<Task> tasks = new List<Task>()
            {
                new Task(() => {
                    permissions.ForEach(x =>
                    {
                        userPermissionTable.UserPermissionTableHeaderCol.Add(x.PermissionActionDescription);
                    });
                }),
                new Task(() =>
                {
                    staff.ForEach(x => {
                        UserPermissionRow row = new UserPermissionRow();
                        var member = _db.Users.FirstOrDefault(u => u.Id == x.UserId);
                        row.User = member.Name +  " " + member.Surname;

                        permissions.ForEach(p => {
                            row.UserPermissionTableCell.Add(new UserPermissionCell(){
                                PermissionState =  _db.UserPermission.Any(x => x.PermissionActionAllowed == true && x.PermissionActionId == p.PermissionActionId && x.UserId == member.Id),
                                PermissionId = p.PermissionActionId,
                                UserId = member.Id
                            });
                        });
                        userPermissionTable.Rows.Add(row);
                    });
                })
            };

            tasks.ForEach(x => { x.Start(); x.Wait(); });

            return Ok(userPermissionTable);                
        }

        [Authorize(Roles = RolesConstants.ROLE_ADMINISTRATOR)]
        [HttpPut]
        public async Task<IActionResult> PostPermissionUser([FromBody] UserPermissionPut userPermissionInfo)
        {
            if (userPermissionInfo == null)
                return BadRequest("Dati non validi.");
            if (userPermissionInfo.PermissionId == 0)
                return BadRequest("Permesso non valido.");
            if (String.IsNullOrEmpty(userPermissionInfo.UserId))
                return BadRequest("Utente non valido.");

            UserPermission userPermission = await _db.UserPermission.FirstOrDefaultAsync(x => x.UserId == userPermissionInfo.UserId && x.PermissionActionId == userPermissionInfo.PermissionId);
            if (userPermission == null)
                return NotFound("Riga di permesso per l'utente non trovata");

            userPermission.PermissionActionAllowed = !userPermission.PermissionActionAllowed;
            _db.Entry(userPermission).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return Ok();
        }
    }
}
