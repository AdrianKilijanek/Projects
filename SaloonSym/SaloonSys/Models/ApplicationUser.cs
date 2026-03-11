using Microsoft.AspNetCore.Identity;

namespace SaloonSys.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumberPersonal { get; set; }
    }
}
