using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StorageLibrary.Context;
using StorageLibrary.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StorageLibrary.Services
{
    public class StorageService : IStorageService
    {
        public UserData UserData { get; set; }

        private readonly ILogger<StorageService> _logger;
        private readonly IUserDataContext _userDataContext;

        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public StorageService(IUserDataContext userDataContext, ILogger<StorageService> logger)
        {
            _userDataContext = userDataContext;
            _logger = logger;

            UserData = new UserData();
        }

        public async void AddUserDataAsync(UserData userData)
        {
            _userDataContext.UserDatas.Add(userData);

            _logger.LogInformation($"Added new user: {userData.Usernane}");

            await SaveChangesAsync();
        }

        public async void EditData()
        {
            var userToEdit = _userDataContext.UserDatas.FirstOrDefault(d => d.Usernane == UserData.Usernane);

            if (userToEdit != null)
            {
                _logger.LogInformation($"Changed password for user: {userToEdit.Usernane}");

                userToEdit.Password = UserData.Password;

                await SaveChangesAsync();
            }
            else
            {
                AddUserDataAsync(UserData);
            }
        }

        public async Task<List<UserData>> GetUserDataAsync()
        {
            await semaphoreSlim.WaitAsync();

            try
            {
                await _userDataContext.UserDatas.LoadAsync();
            }
            finally
            {
                semaphoreSlim.Release();
            }

            return _userDataContext.UserDatas.Local.ToList();
        }

        public async Task MigrateAsync()
        {
            await semaphoreSlim.WaitAsync();

            try
            {
                await _userDataContext.Database.MigrateAsync();
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async void RemoveUserDataAsynv(UserData userData)
        {
            _userDataContext.UserDatas.Remove(userData);

            _logger.LogInformation($"Removed user: {userData.Usernane}");

            await SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await semaphoreSlim.WaitAsync();

            try
            {
                _logger.LogInformation("Data saved");

                await _userDataContext.SaveChangesAsync();
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public void UpdateData(UserData userData)
        {
            UserData.Usernane = userData.Usernane;
            UserData.Password = UserData.Password;
        }
    }
}
