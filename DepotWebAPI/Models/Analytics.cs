// DepotWebAPI/Models/Analytics.cs
namespace DepotWebAPI.Models
{
    public class Analytics
    {
        public int Id { get; set; } // Primair sleutel
        public int Home { get; set; }
        public int Store { get; set; }
        public int Implementation { get; set; }
        public int AboutUs { get; set; }
        public int Contact { get; set; }
    }
}
