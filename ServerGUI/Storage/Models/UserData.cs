using System.ComponentModel.DataAnnotations;

namespace Storage.Models
{
    public class UserData
    {
        public long Id { get; set; }
        public string Login { get; set; }
        public byte[] Password { get; set; }
    }
}
