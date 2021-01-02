namespace Storage.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Login { get; set; }
        public byte[] Password { get; set; }
        public string? FavouriteLocations { get; set; }
        public string? PreferredWeatherPeriod { get; set; }
    }
}
