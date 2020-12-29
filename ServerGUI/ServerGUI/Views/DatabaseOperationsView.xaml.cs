using ServerGUI.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ServerGUI.Views
{
    /// <summary>
    /// Interaction logic for DatabaseOperationsView.xaml
    /// </summary>
    public partial class DatabaseOperationsView : Window
    {
        public DatabaseOperationsView(DatabaseOperationsViewModel databaseOperationsViewModel)
        {
            DataContext = databaseOperationsViewModel;

            InitializeComponent();
        }

        /// <summary>
        /// Quits database content window
        /// </summary>
        /// <param name="sender">button object</param>
        /// <param name="e">button event</param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            switch (MessageBox.Show("Are you sure?", "Closing database window",
                MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
            {
                case MessageBoxResult.Yes:
                    Close();
                    break;
                case MessageBoxResult.No:
                case MessageBoxResult.Cancel:
                default:
                    break;
            }
        }

        /// <summary>
        /// Event occurs after pressing left mouse button
        /// </summary>
        /// <param name="sender">Mouse object</param>
        /// <param name="e">Mouse event event</param>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch { }
        }
    }
}
