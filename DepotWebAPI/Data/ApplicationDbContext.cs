// DepotWebAPI/Data/ApplicationDbContext.cs
using DepotWebAPI.Models; // Zorg ervoor dat je de juiste namespace gebruikt voor de Analytics klasse
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DepotWebAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Voeg deze DbSet toe om toegang te krijgen tot de Analytics entiteit
        public DbSet<Analytics> Analytics { get; set; }
    }
}
