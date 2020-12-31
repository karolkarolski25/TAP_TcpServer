using Login.Enums;
using Microsoft.Extensions.Logging;
using Storage.DAL;
using Storage.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Login.Services
{
    public class LoginService : ILoginService
    {
        private readonly ILogger<LoginService> _logger;
        private readonly IStorageService _storageService;
        private readonly ICryptoService _cryptoService;

        public LoginService(ILogger<LoginService> logger, IStorageService storageService,
            ICryptoService cryptoService)
        {
            _logger = logger;
            _storageService = storageService;
            _cryptoService = cryptoService;
        }

        /// <summary>
        /// Changes password
        /// </summary>
        /// <param name="data"></param>
        /// <returns>True if operation was succesfull</returns>
        public async Task<bool> ChangePassword(string login, string password)
        {
            if (!(await _storageService.GetUserDataAsync()).Any(u => u.Login == login))
            {
                return false;
            }

            try
            {
                _storageService.UpdateData(new UserData()
                {
                    Login = login,
                    Password = await _cryptoService.EncryptPassword(password)
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
            var user = (await _storageService.GetUserDataAsync()).FirstOrDefault(u => u.Login == login);

            if (user == null)
            {
                return UserLoginSettings.UserNotExists;
            }

            try
            {
                if (await _cryptoService.DecryptPassword(user.Password) == password)
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
            if ((await _storageService.GetUserDataAsync()).Any(u => u.Login == login))
            {
                return false;
            }

            try
            {
                _storageService.UpdateData(new UserData()
                {
                    Login = login,
                    Password = await _cryptoService.EncryptPassword(password)
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
