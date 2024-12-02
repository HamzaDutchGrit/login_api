// DepotWebAPI/Models/RegisterRequest.cs
namespace DepotWebAPI.Models
{
    public class RegisterRequest
    {
        public string Name { get; set; } = string.Empty;  // Dit zorgt voor een standaardwaarde
        public string Email { get; set; } = string.Empty; // Dit zorgt voor een standaardwaarde
        public string Password { get; set; } = string.Empty; // Dit zorgt voor een standaardwaarde
        public bool IsAdmin { get; set; } = false; // Standaard is false
    }
}
