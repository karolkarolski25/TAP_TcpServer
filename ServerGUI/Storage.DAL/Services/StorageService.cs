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
        public User Users { get; set; }

        private readonly ILogger<StorageService> _logger;
        private readonly IUserContext _userDataContext;

        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public StorageService(IUserContext userDataContext, ILogger<StorageService> logger)
        {
            _userDataContext = userDataContext;
            _logger = logger;

            Users = new User();
        }

        /// <summary>
        /// Add new user to database
        /// </summary>
        /// <param name="userData">new user data</param>
        public async void AddUserDataAsync(User userData)
        {
            _userDataContext.Users.Add(userData);

            _logger.LogInformation($"Added new user: {userData.Login}");

            await SaveChangesAsync();
        }

        /// <summary>
        /// Edit user or add new
        /// </summary>
        public async void EditData()
        {
            var userToEdit = _userDataContext.Users.FirstOrDefault(d => d.Login == Users.Login);

            if (userToEdit != null)
            {
                _logger.LogInformation($"Changed password or favourite location for user: {userToEdit.Login}");

                userToEdit.Password = Users.Password;
                userToEdit.FavouriteLocation = Users.FavouriteLocation;

                await SaveChangesAsync();
            }
            else
            {
                AddUserDataAsync(Users);
            }
        }

        /// <summary>
        /// Updates specified user
        /// </summary>
        /// <param name="userData"></param>
        public void UpdateData(User userData)
        {
            //Users.Id = 0;
            Users.Login = userData.Login;
            Users.Password ??= userData.Password;
            Users.FavouriteLocation = userData.FavouriteLocation;

            EditData();
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>List containing all users</returns>
        public async Task<List<User>> GetUserDataAsync()
        {
            await semaphoreSlim.WaitAsync();

            try
            {
                await _userDataContext.Users.LoadAsync();
            }
            finally
            {
                semaphoreSlim.Release();
            }

            return _userDataContext.Users.ToList();
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
        public async void RemoveUserDataAsync(User userData)
        {
            _userDataContext.Users.Remove(userData);

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
