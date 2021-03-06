﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prism.Events;
using Storage.Context;
using Storage.DAL.Events;
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
        private readonly IEventAggregator _eventAggregator;

        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public StorageService(IUserContext userDataContext, ILogger<StorageService> logger, 
            IEventAggregator eventAggregator)
        {
            _userDataContext = userDataContext;
            _logger = logger;
            _eventAggregator = eventAggregator;

            Users = new User();
        }

        /// <summary>
        /// Add new user to database
        /// </summary>
        /// <param name="userData">new user data</param>
        public async void AddUserDataAsync(User userData)
        {
            userData.Id = userData.Id == 0 ? userData.Id : 0;

            _userDataContext.Users.Add(userData);

            _logger.LogInformation($"Added new user: {userData.Login}");

            await SaveChangesAsync();

            _eventAggregator.GetEvent<NewUserRegisteredEvent>().Publish();
        }

        /// <summary>
        /// Edit user or add new
        /// </summary>
        public async void EditData()
        {
            var userToEdit = _userDataContext.Users.FirstOrDefault(d => d.Login == Users.Login);

            if (userToEdit != null)
            {
                _logger.LogInformation($"Updated user: {userToEdit.Login}");

                userToEdit.Password = Users.Password ?? userToEdit.Password;
                userToEdit.FavouriteLocations = Users.FavouriteLocations ?? userToEdit.FavouriteLocations;
                userToEdit.PreferredWeatherPeriod = Users.PreferredWeatherPeriod ?? userToEdit.PreferredWeatherPeriod;

                await SaveChangesAsync();

                _eventAggregator.GetEvent<DatabaseContentChangedEvent>().Publish();
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
            Users.Login = userData.Login;
            Users.Password = userData.Password ?? Users.Password;
            Users.FavouriteLocations = userData.FavouriteLocations;
            Users.PreferredWeatherPeriod = userData.PreferredWeatherPeriod;

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

            return _userDataContext.Users.Local.ToList();
        }

        public async Task<string> GetFavouriteLocations(string login)
        {
            var user = (await GetUserDataAsync()).FirstOrDefault(d => d.Login == login);

            return $"{user.FavouriteLocations};{user.PreferredWeatherPeriod}";
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
