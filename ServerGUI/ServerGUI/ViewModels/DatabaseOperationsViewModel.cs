using Microsoft.Extensions.Logging;
using Prism.Commands;
using Storage.DAL;
using Storage.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ServerGUI.ViewModels
{
    public class DatabaseOperationsViewModel
    {
        private readonly ILogger<DatabaseOperationsViewModel> _logger;
        private readonly IStorageService _storageService;

        public ObservableCollection<UserData> UsersDataView { get; set; }
        public ObservableCollection<UserData> SelectedUserDetails { get; set; } = new ObservableCollection<UserData>();
        public UserData SelectedUser { get; set; }

        private DelegateCommand _changePasswordCommand;
        public DelegateCommand ChangePasswordCommand => _changePasswordCommand ??= new DelegateCommand(ChangePassword);

        private DelegateCommand _removeUserCommand;
        public DelegateCommand RemoveUserCommand => _removeUserCommand ??= new DelegateCommand(RemoveUser);

        private DelegateCommand _selectionChangedCommand;
        public DelegateCommand SelectionChangedCommand => _selectionChangedCommand ??= new DelegateCommand(SelectionChanged);

        public DatabaseOperationsViewModel(IStorageService storageService,
            ILogger<DatabaseOperationsViewModel> logger)
        {
            _storageService = storageService;
            _logger = logger;
        }

        /// <summary>
        /// Add selected user
        /// </summary>
        private void SelectionChanged()
        {
            if (SelectedUserDetails.Any())
            {
                SelectedUserDetails.Clear();
            }

            if (SelectedUser != null)
            {
                SelectedUserDetails.Add(SelectedUser);
            }
        }

        /// <summary>
        /// Update data from database
        /// </summary>
        /// <param name="users">List from database</param>
        public void SetDatabaseContent(List<UserData> users)
        {
            UsersDataView = new ObservableCollection<UserData>(users);
        }

        /// <summary>
        /// Remove selected user
        /// </summary>
        private void RemoveUser()
        {
            switch (MessageBox.Show("Delete selected user?", "Delete user", MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question))
            {
                case MessageBoxResult.Yes:

                    var userToRemove = SelectedUser;

                    if (userToRemove != null)
                    {
                        _storageService.RemoveUserDataAsync(userToRemove);

                        UsersDataView.Remove(userToRemove);

                        MessageBox.Show($"User {userToRemove.Login} sucessfully deleted", "Deletion complete",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        _logger.LogInformation($"User {userToRemove.Login} sucessfully deleted");
                    }

                    break;
                case MessageBoxResult.No:
                case MessageBoxResult.Cancel:
                default:
                    break;
            }
        }

        /// <summary>
        /// Change selected user's password
        /// </summary>
        private async void ChangePassword()
        {
            for (int i = 0; i < 10; i++)
            {
                _storageService.UpdateData(new UserData()
                {
                    Login = $"Login{i}",
                    Password = new byte[2] { (byte)i, (byte)(i + 1) },
                    FavouriteLocation = $"FavouriteLocation{i}"
                });

                await Task.Delay(250);
            }
        }
    }
}
