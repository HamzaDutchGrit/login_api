// DepotWebAPI/Data/ApplicationUser.cs

using Microsoft.AspNetCore.Identity;

namespace DepotWebAPI.Data
{
    public class ApplicationUser : IdentityUser
    {
        public required string FullName { get; set; }  // Markeer als required
        public bool IsAdmin { get; set; } = false;     // Voeg een standaardwaarde toe
    }
}
