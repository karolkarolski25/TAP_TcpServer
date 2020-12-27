using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StorageLibrary.Context;
using StorageLibrary.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Storage.DAL
{
    public class StorageService : IStorageService
    {
        public UserData UserDatas { get; set; }

        private readonly ILogger<StorageService> _logger;
        private readonly IUserDataContext _userDataContext;

        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public StorageService(IUserDataContext userDataContext, ILogger<StorageService> logger)
        {
            _userDataContext = userDataContext;
            _logger = logger;

            UserDatas = new UserData();
        }

        /// <summary>
        /// Add new user to database
        /// </summary>
        /// <param name="userData">new user data</param>
        public async void AddUserDataAsync(UserData userData)
        {
            _userDataContext.UserDatas.Add(userData);

            _logger.LogInformation($"Added new user: {userData.Usernane}");

            await SaveChangesAsync();
        }

        /// <summary>
        /// Edit user or add new
        /// </summary>
        public async void EditData()
        {
            var userToEdit = _userDataContext.UserDatas.FirstOrDefault(d => d.Usernane == UserDatas.Usernane);

            if (userToEdit != null)
            {
                _logger.LogInformation($"Changed password for user: {userToEdit.Usernane}");

                userToEdit.Password = UserDatas.Password;

                await SaveChangesAsync();
            }
            else
            {
                AddUserDataAsync(UserDatas);
            }
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>List containing all users</returns>
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

        /// <summary>
        /// Apply migrations
        /// </summary>
        /// <returns>Task</returns>
        public async Task MigrateAsync()
        {
            await semaphoreSlim.WaitAsync();

            try
            {
                await _userDataContext.DatabaseFacade.MigrateAsync();
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        /// <summary>
        /// Remove specified user from database
        /// </summary>
        /// <param name="userData">User to delete</param>
        public async void RemoveUserDataAsync(UserData userData)
        {
            _userDataContext.UserDatas.Remove(userData);

            _logger.LogInformation($"Removed user: {userData.Usernane}");

            await SaveChangesAsync();
        }

        /// <summary>
        /// Save pending data to database
        /// </summary>
        /// <returns>Task</returns>
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

        /// <summary>
        /// Updates specified user
        /// </summary>
        /// <param name="userData"></param>
        public void UpdateData(UserData userData)
        {
            UserDatas.Usernane = userData.Usernane;
            UserDatas.Password = UserDatas.Password;
        }
    }
}
