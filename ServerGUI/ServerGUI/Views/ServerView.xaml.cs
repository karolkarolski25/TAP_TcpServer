using ServerGUI.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ServerGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(ServerViewModel mainWindowViewModel)
        {
            DataContext = mainWindowViewModel;

            InitializeComponent();
        }

        /// <summary>
        /// Quits server application
        /// </summary>
        /// <param name="sender">button object</param>
        /// <param name="e">button event</param>
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            switch (MessageBox.Show("Are you sure?", "Closing application", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
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
        /// Minimizes application
        /// </summary>
        /// <param name="sender">button object</param>
        /// <param name="e">button event</param>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
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
