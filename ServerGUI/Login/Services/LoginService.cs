using Login.Enums;
using Microsoft.Extensions.Logging;
using Storage.DAL;
using Storage.Models;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Login.Services
{
    public class LoginService : ILoginService
    {
        private readonly Aes aes;
        private readonly CryptoConfiguration _cryptoConfiguration;

        private readonly ILogger<LoginService> _logger;
        private readonly IStorageService _storageService;

        public LoginService(CryptoConfiguration cryptoConfiguration, ILogger<LoginService> logger,
            IStorageService storageService)
        {
            _logger = logger;
            _cryptoConfiguration = cryptoConfiguration;
            _storageService = storageService;

            aes = Aes.Create();
        }

        /// <summary>
        /// Changes password
        /// </summary>
        /// <param name="data"></param>
        /// <returns>True if operation was succesfull</returns>
        public async Task<bool> ChangePassword(string login, string password)
        {
            byte[] encryptedPassword;

            if (!(await _storageService.GetUserDataAsync()).Any(u => u.Login == login))
            {
                return false;
            }

            try
            {
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, aes.CreateEncryptor(_cryptoConfiguration.Key, 
                        _cryptoConfiguration.IV), CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(password);
                        }
                        encryptedPassword = msEncrypt.ToArray();
                    }
                }

                _storageService.UpdateData(new UserData()
                {
                    Login = login,
                    Password = encryptedPassword
                });
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if user is already in database
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Information if user is in database</returns>
        public async Task<UserLoginSettings> CheckData(string login, string password)
        {
            string decryptedPassword;

            var user = (await _storageService.GetUserDataAsync()).FirstOrDefault(u => u.Login == login);

            if (user == null)
            {
                return UserLoginSettings.UserNotExists;
            }

            try
            {
                using (MemoryStream msDecrypt = new MemoryStream(user.Password))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, aes.CreateDecryptor(_cryptoConfiguration.Key,
                        _cryptoConfiguration.IV), CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            decryptedPassword = srDecrypt.ReadToEnd();
                        }
                    }
                }

                if (decryptedPassword == password)
                {
                    return UserLoginSettings.LoggedIn;
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
            }

            return UserLoginSettings.BadPassword;
        }

        /// <summary>
        /// Adds user to database
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Status of registration</returns>
        public async Task<bool> RegisterAccount(string login, string password)
        {
            byte[] encryptedPassword;

            if ((await _storageService.GetUserDataAsync()).Any(u => u.Login == login))
            {
                return false;
            }

            try
            {
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, aes.CreateEncryptor(_cryptoConfiguration.Key, 
                        _cryptoConfiguration.IV), CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(password);
                        }
                        encryptedPassword = msEncrypt.ToArray();
                    }
                }

                _storageService.UpdateData(new UserData()
                {
                    Login = login,
                    Password = encryptedPassword
                });
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);

                return false;
            }

            return true;
        }
    }
}
