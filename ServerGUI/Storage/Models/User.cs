namespace Storage.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Login { get; set; }
        public byte[] Password { get; set; }
        public string? FavouriteLocation { get; set; }
    }
}
