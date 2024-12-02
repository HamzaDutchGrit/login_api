// DepotWebAPI/Models/LoginRequest.cs
namespace DepotWebAPI.Models
{
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty; // Dit zorgt voor een standaardwaarde
        public string Password { get; set; } = string.Empty; // Dit zorgt voor een standaardwaarde
        public bool IsAdmin { get; set; } = false;     // Voeg een standaardwaarde toe
    }
}
