public class RefreshToken
{
    public string Id { get; set; } = Guid.NewGuid().ToString(); // Zorg dat Id wordt geïnitialiseerd

    // Voeg 'required' toe aan de Token-property
    public required string Token { get; set; }

    public DateTime Expires { get; set; }
    public bool IsExpired => DateTime.UtcNow >= Expires;
}
