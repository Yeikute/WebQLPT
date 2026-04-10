using Microsoft.AspNetCore.Identity;

namespace WebQLPT.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Add additional profile fields here if needed
        public string FullName { get; set; }
    }
}
