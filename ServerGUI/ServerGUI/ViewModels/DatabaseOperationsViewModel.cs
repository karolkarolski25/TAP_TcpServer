using Login.Services;
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
    public class DatabaseOperationsViewModel : INotifyPropertyChanged
    {
        private readonly ILogger<DatabaseOperationsViewModel> _logger;
        private readonly IStorageService _storageService;
        private readonly ICryptoService _cryptoService;

        private bool canEditUser { get; set; } = false;

        public string NewLogin { get; set; } = string.Empty;
        public string NewFavouriteLocation { get; set; } = string.Empty;

        public Visibility NewUserVisibility { get; set; } = Visibility.Collapsed;

        public ObservableCollection<UserData> UsersDataView { get; set; }
        public ObservableCollection<UserData> SelectedUserDetails { get; set; } = new ObservableCollection<UserData>();
        public UserData SelectedUser { get; set; }

        private DelegateCommand _changePasswordCommand;
        public DelegateCommand ChangePasswordCommand => _changePasswordCommand ??= new DelegateCommand(ChangePassword)
            .ObservesCanExecute(() => canEditUser);

        private DelegateCommand _removeUserCommand;
        public DelegateCommand RemoveUserCommand => _removeUserCommand ??= new DelegateCommand(RemoveUser)
            .ObservesCanExecute(() => canEditUser);

        private DelegateCommand _selectionChangedCommand;
        public DelegateCommand SelectionChangedCommand => _selectionChangedCommand ??= new DelegateCommand(SelectionChanged);

        private DelegateCommand _showAddNewUserFieldsCommand;
        public DelegateCommand ShowAddNewUserFieldsCommand => _showAddNewUserFieldsCommand ??= new DelegateCommand(ShowAddNewUserFields);

        private DelegateCommand _confirmAddNewUserCommand;
        public DelegateCommand ConfirmAddNewUserCommand => _confirmAddNewUserCommand ??= new DelegateCommand(ConfirmAddNewUser);

        public event PropertyChangedEventHandler PropertyChanged;


        public DatabaseOperationsViewModel(IStorageService storageService,
            ILogger<DatabaseOperationsViewModel> logger, ICryptoService cryptoService)
        {
            _storageService = storageService;
            _logger = logger;
            _cryptoService = cryptoService;
        }

        /// <summary>
        /// Show fileds to add new user
        /// </summary>
        private void ShowAddNewUserFields()
        {
            if (NewUserVisibility == Visibility.Collapsed)
            {
                NewUserVisibility = Visibility.Visible;
            }
            else
            {
                NewUserVisibility = Visibility.Collapsed;
            }

            OnPropertyChanged(nameof(NewUserVisibility));
        }

        /// <summary>
        /// Add new user to database
        /// </summary>
        private async void ConfirmAddNewUser()
        {
            if (!string.IsNullOrWhiteSpace(NewLogin))
            {
                if (!(await _storageService.GetUserDataAsync()).Any(u => u.Login == NewLogin))
                {
                    var newUser = new UserData()
                    {
                        Login = NewLogin,
                        Password = await _cryptoService.EncryptPassword("1234"),
                        FavouriteLocation = NewFavouriteLocation
                    };

                    _storageService.UpdateData(newUser);

                    MessageBox.Show($"User {NewLogin} added with default password: 1234", "New user added",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    _logger.LogInformation($"User {NewLogin} added with default password: 1234");

                    UsersDataView.Add(newUser);

                    NewUserVisibility = Visibility.Collapsed;
                    OnPropertyChanged(nameof(NewUserVisibility));
                }
                else
                {
                    MessageBox.Show("User alredy registered\r\nTry again", "Login conflict",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Login cannot be empty\r\nTry again", "Login error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

                canEditUser = true;
                OnPropertyChanged(nameof(canEditUser));
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

                        canEditUser = false;
                        OnPropertyChanged(nameof(canEditUser));
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
        private void ChangePassword() //TODO
        {

        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
