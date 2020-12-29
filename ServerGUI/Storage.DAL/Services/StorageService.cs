using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Storage.Context;
using Storage.Models;
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

            _logger.LogInformation($"Added new user: {userData.Login}");

            await SaveChangesAsync();
        }

        /// <summary>
        /// Edit user or add new
        /// </summary>
        public async void EditData()
        {
            var userToEdit = _userDataContext.UserDatas.FirstOrDefault(d => d.Login == UserDatas.Login);

            if (userToEdit != null)
            {
                _logger.LogInformation($"Changed password or favourite location for user: {userToEdit.Login}");

                userToEdit.Password = UserDatas.Password;

                await SaveChangesAsync();
            }
            else
            {
                AddUserDataAsync(UserDatas);
            }
        }

        /// <summary>
        /// Updates specified user
        /// </summary>
        /// <param name="userData"></param>
        public void UpdateData(UserData userData)
        {
            UserDatas.Login = userData.Login;
            UserDatas.Password = userData.Password;
            UserDatas.FavouriteLocation = userData.FavouriteLocation;

            EditData();
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
                await _userDataContext.Database.MigrateAsync();
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

            _logger.LogInformation($"Removed user: {userData.Login}");

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
    }
}
