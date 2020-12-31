using System.Threading.Tasks;

namespace Login.Services
{
    public interface ICryptoService
    {
        Task<byte[]> EncryptPassword(string password);
        Task<string> DecryptPassword(byte[] encryptedPassword);
    }
}
