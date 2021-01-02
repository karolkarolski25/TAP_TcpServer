using Login.Services;
using Microsoft.Extensions.Logging;
using Prism.Commands;
using Prism.Events;
using Storage.DAL;
using Storage.Events;
using Storage.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace ServerGUI.ViewModels
{
    public class DatabaseOperationsViewModel : INotifyPropertyChanged
    {
        private readonly ILogger<DatabaseOperationsViewModel> _logger;
        private readonly IStorageService _storageService;
        private readonly ICryptoService _cryptoService;
        private readonly IEventAggregator _eventAggregator;
        private bool canEditUser { get; set; } = false;

        public string NewLogin { get; set; } = string.Empty;
        public string NewFavouriteLocation { get; set; } = string.Empty;

        public Visibility NewUserVisibility { get; set; } = Visibility.Collapsed;

        public ObservableCollection<UserData> UsersDataView { get; set; }
        public ObservableCollection<UserData> SelectedUserDetails { get; set; } = new ObservableCollection<UserData>();
        public UserData SelectedUser { get; set; }

        private DelegateCommand _removeUserCommand;
        public DelegateCommand RemoveUserCommand => _removeUserCommand ??= new DelegateCommand(RemoveUser)
            .ObservesCanExecute(() => canEditUser);

        private DelegateCommand _selectionChangedCommand;
        public DelegateCommand SelectionChangedCommand => _selectionChangedCommand ??= new DelegateCommand(SelectionChanged);

        private DelegateCommand _showAddNewUserFieldsCommand;
        public DelegateCommand ShowAddNewUserFieldsCommand => _showAddNewUserFieldsCommand ??= new DelegateCommand(ShowAddNewUserFields);

        private DelegateCommand _confirmAddNewUserCommand;
        public DelegateCommand ConfirmAddNewUserCommand => _confirmAddNewUserCommand ??= new DelegateCommand(ConfirmAddNewUser);

        private DelegateCommand _exportDatabaseContentCommand;
        public DelegateCommand ExportDatabaseContentCommand => _exportDatabaseContentCommand ??= new DelegateCommand(CheckDatabaseContent);


        public event PropertyChangedEventHandler PropertyChanged;


        public DatabaseOperationsViewModel(IStorageService storageService,
            ILogger<DatabaseOperationsViewModel> logger, ICryptoService cryptoService,
            IEventAggregator eventAggregator)
        {
            _storageService = storageService;
            _logger = logger;
            _cryptoService = cryptoService;
            _eventAggregator = eventAggregator;

            _eventAggregator.GetEvent<NewUserRegistered>().Subscribe(async () =>
            {
                UsersDataView = new ObservableCollection<User>(await _storageService.GetUserDataAsync());

                OnPropertyChanged(nameof(UsersDataView));
            });
        }

        /// <summary>
        /// Check database content
        /// </summary>
        private async void CheckDatabaseContent()
        {
            var databaseContent = await _storageService.GetUserDataAsync();

            if (databaseContent.Any())
            {
                ExportDatabaseContent(databaseContent);
            }
            else
            {
                switch (MessageBox.Show("Database is empty\nDo you want to export it anyway?", "Question",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                {
                    case MessageBoxResult.Yes:
                        ExportDatabaseContent(databaseContent);
                        break;
                    case MessageBoxResult.No:
                    case MessageBoxResult.Cancel:
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Export database content to *.csv file
        /// </summary>
        /// <param name="databaseContent">databae content</param>
        private async void ExportDatabaseContent(IEnumerable<User> databaseContent)
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                FileName = "Weather server database",
                DefaultExt = ".csv",
                Filter = "CSV file (.csv)|*.csv"
            };

            if (dlg.ShowDialog() == true)
            {
                string filePath = dlg.FileName;

                using var writer = new StreamWriter(filePath);

                await writer.WriteLineAsync("ID,Login,FavouriteLocation");

                foreach (var user in databaseContent)
                {
                    await writer.WriteLineAsync($"{user.Id},{user.Login},{user.FavouriteLocation}");
                }

                MessageBox.Show($"File has been sucessfully saved\n{filePath}", "Saved",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                OpenCsvFile(filePath);
            }
        }

        /// <summary>
        /// Start csv file after saving logs
        /// </summary>
        /// <param name="filePath">database content file path</param>
        private void OpenCsvFile(string filePath)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            };

            Process.Start(processStartInfo);
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

                    UsersDataView = new ObservableCollection<UserData>(await _storageService.GetUserDataAsync());
                    OnPropertyChanged(nameof(UsersDataView));

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
                    if (SelectedUser != null)
                    {
                        _storageService.RemoveUserDataAsync(SelectedUser);

                        MessageBox.Show($"User {SelectedUser.Login} sucessfully deleted", "Deletion complete",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        _logger.LogInformation($"User {SelectedUser.Login} sucessfully deleted");

                        UsersDataView.Remove(SelectedUser);

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
        /// Update GUI content
        /// </summary>
        /// <param name="propertyName">Property to update</param>
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
