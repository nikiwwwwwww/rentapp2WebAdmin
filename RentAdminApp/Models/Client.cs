using System;
using System.Collections.Generic;

namespace RentAdminApp.Models
{
    public partial class Client
    {

        public int IdClient { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public bool? TypeClient { get; set; }
        public string? PasswordHash { get; set; }
        public string? Photo { get; set; }
        public string? Role { get; set; }

    }
}
