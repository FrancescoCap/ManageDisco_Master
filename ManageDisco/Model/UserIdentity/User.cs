using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model.UserIdentity
{
    public class User:IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }  
        /// <summary>
        /// Il codice associato all'utente di tipo PR per permettere di collegare le prenotazioni
        /// </summary>
        public string UserCode { get; set; } 
        public string DiscoEntityId { get; set; }
        public DiscoEntity DiscoEntity { get; set; }
        public string Gender { get; set; }
        public decimal Points { get; set; }
    }

    public class UserRoles: User
    {
        public bool UserCanHandleEvents { get; set; }
        public bool UserCanHandleWarehouse { get; set; }
        public bool UserCanHandleHomeTemplate { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public bool TokenEncrypted { get; set; }
    }

    public class RegisterRequest: LoginRequest
    {
        [Required]        
        public string Username { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Surname { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
       
        public string Role { get; set; }
        
        public string PrCode { get; set; }
        [Required]
        public string Gender { get; set; }
    }

    public class AuthenticationResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string ClientSession { get; set; }
        public string Message { get; set; }
        public bool OperationSuccess { get; set; }
    }

    public class CollaboratorView
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string ReservationCode { get; set; }
        public int ReservationCount { get; set; }
        /// <summary>
        /// Totale incasso da prenotazioni
        /// </summary>
        public decimal TotalReservationBudget { get; set; }
        /// <summary>
        /// Totale generale di quello che dovrà incassare il PR
        /// </summary>
        public decimal ResumeCredit { get; set; }
        /// <summary>
        /// Totale di ciò che ha incassato il PR
        /// </summary>
        public decimal PayedCredit { get; set; }
    }

    public class UserInfoView
    {
        public string UserName { get; set; }
        public string UserSurname { get; set; }
        public string UserEmail { get; set; }
        public string UserPhoneNumber { get; set; }
        public bool IsCustomer { get; set; }
        public bool IsPhoneNumberConfirmed { get; set; }
        public string Gender { get; set; }
        public decimal UserPoints { get; set; }
        //PR INFO
        public string PrName { get; set; }
        public string PrSurname { get; set; }
        public string PrEmail { get; set; }
        public string PrCode { get; set; }      
        public string InvitationLink { get; set; }
    }
}
