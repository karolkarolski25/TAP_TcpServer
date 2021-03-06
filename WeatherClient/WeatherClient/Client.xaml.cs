﻿using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WeatherClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Client : Window
    {
        private string ipAddress;
        private int port;
        private TcpClient client;
        private byte[] buffer;
        private NetworkStream stream;
        private bool connected = false;

        public Client()
        {
            InitializeComponent();

#if DEBUG
            textBoxIPAddress.Text = "127.0.0.1";
            textBoxPort.Text = "2048";

            textBoxLogin.Text = "qwe";
            textBoxPassword.Password = "qwe";

            textBoxLocation.Text = "Warsaw,Berlin,Szczecin";
            textBoxDate.Text = "3";
#endif
        }

        /// <summary>
        /// Connects to server and receives first message from server
        /// </summary>
        private async void ConnectToServer()
        {
            ipAddress = textBoxIPAddress.Text;

            if (!int.TryParse(textBoxPort.Text, out port) || (port < 1024 || port > 65535))
            {
                MessageBox.Show("Wrong port number, try again", "Port error",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            try
            {
                client = new TcpClient();

                await client.ConnectAsync(ipAddress, port);
            }
            catch
            {
                MessageBox.Show("Cannot connect to server", "Connection error",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            stream = client.GetStream();

            buffer = new byte[85];
            stream.Read(buffer, 0, 85);

            if (Encoding.ASCII.GetString(buffer).Replace("\0", "") == "Login: ")
            {
                ClientLogTextBox.Text = "Log in";
                LoginButton.IsEnabled = true;
                ConnectButton.Content = "Disconnect";
                connected = true;
            }
        }

        /// <summary>
        /// Disconnects from server
        /// </summary>
        private void DisconnectFromServer()
        {
            buffer = Encoding.ASCII.GetBytes("exit");
            stream.Write(buffer, 0, buffer.Length);

            client.Close();
            ConnectButton.Content = "Connect";
            connected = false;
            LoginButton.IsEnabled = false;
            SaveWeatherButton.IsEnabled = true;
            ChangePasswordButton.IsEnabled = false;
            SaveFavouriteLocationButton.IsEnabled = false;
            GetWeatherButton.IsEnabled = true;
            buffer = new byte[85];
            ClientLogTextBox.Text = "";
            textBoxLocation.Text = "";
            

#if RELEASE
            textBoxLogin.Clear();
            textBoxPassword.Clear();
            textBoxLocation.Clear();
            textBoxDate.Clear();
#endif
        }

        /// <summary>
        /// Sends login and password to server
        /// </summary>
        private async void HandleLogin()
        {
            buffer = Encoding.ASCII.GetBytes(textBoxLogin.Text);

            await stream.WriteAsync(buffer, 0, buffer.Length);
            await stream.WriteAsync(buffer, 0, 2);

            buffer = new byte[85];

            await stream.ReadAsync(buffer, 0, 85);

            string data = Encoding.ASCII.GetString(buffer).Replace("\0", "");

            if (Encoding.ASCII.GetString(buffer).Replace("\0", "") == "Password: ")
            {
                buffer = Encoding.ASCII.GetBytes(textBoxPassword.Password);

                await stream.WriteAsync(buffer, 0, buffer.Length);
                await stream.WriteAsync(buffer, 0, 2);

                buffer = new byte[85];

                await stream.ReadAsync(buffer, 0, buffer.Length);

                string message = Encoding.ASCII.GetString(buffer).Replace("\0", "");

                if (message.Contains("fav"))
                {
                    var weatherPreferences = message.Substring(3).Split(';');

                    textBoxLocation.Text = weatherPreferences[0];
                    textBoxDate.Text = weatherPreferences[1];

                    buffer = new byte[1024];
                }
                else if (message == "Account not found, do you want to create new account? (Y/N):")
                {
                    if (!await HandleRegistration())
                    {
                        return;
                    }
                }
                else if (message == "Bad password, try again")
                {
                    MessageBox.Show("Bad password, try again", "Bad password", MessageBoxButton.OK, MessageBoxImage.Error);
                    buffer = new byte[85];
                    await stream.ReadAsync(buffer, 0, 85);
                    return;
                }

                ClientLogTextBox.Text = "Enter Location and number of days or date";

                GetWeatherButton.IsEnabled = true;
                SaveWeatherButton.IsEnabled = true;
                LoginButton.IsEnabled = false;
                ChangePasswordButton.IsEnabled = true;
                SaveFavouriteLocationButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Shows popup message if user wants to register account
        /// </summary>
        /// <returns>True if user wants to register, false if user don't want to register</returns>
        private async Task<bool> HandleRegistration()
        {
            switch (MessageBox.Show("Do you want to create new account", "Account not found",
                MessageBoxButton.YesNo, MessageBoxImage.Question))
            {
                case MessageBoxResult.Yes:
                    buffer = Encoding.ASCII.GetBytes("Y");
                    await stream.WriteAsync(buffer, 0, buffer.Length);
                    buffer = new byte[85];
                    await stream.ReadAsync(buffer, 0, buffer.Length);
                    return true;
                case MessageBoxResult.No:
                    buffer = Encoding.ASCII.GetBytes("N");
                    await stream.WriteAsync(buffer, 0, buffer.Length);
                    buffer = new byte[2];
                    await stream.WriteAsync(buffer, 0, 2);
                    buffer = new byte[85];
                    await stream.ReadAsync(buffer, 0, 85);
                    return false;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Sends location and date to server
        /// </summary>
        private async void SendLocationAndDate()
        {
            string location = textBoxLocation.Text;
            string daysPeriod = textBoxDate.Text;

            if (string.IsNullOrWhiteSpace(location) || string.IsNullOrWhiteSpace(daysPeriod))
            {
                MessageBox.Show("Weather location and days period cannot be empty", "Empty weather data",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                buffer = Encoding.ASCII.GetBytes(location);

                await stream.WriteAsync(buffer, 0, buffer.Length);

                buffer = new byte[1024];

                buffer = Encoding.ASCII.GetBytes(daysPeriod);

                await stream.WriteAsync(buffer, 0, buffer.Length);

                buffer = new byte[2048];

                string data = "";
                int locationsCount = location.Where(c => c == ',').Count() + 1;

                ClientLogTextBox.Clear();

                for (int i = 0; i < locationsCount; i++)
                {
                    do
                    {
                        await stream.ReadAsync(buffer, 0, buffer.Length);
                        data = Encoding.ASCII.GetString(buffer).Replace("\0", "");

                        if (data.Contains("Incorrect weather period, try again"))
                        {
                            MessageBox.Show("Incorrect weather period, try different formatting", "Format error",
                                MessageBoxButton.OK, MessageBoxImage.Error);

                            return;
                        }
                    } while (!data.Contains("Temperature"));

                    ClientLogTextBox.Text += data;

                    Array.Clear(buffer, 0, buffer.Length);
                }
            }
        }

        /// <summary>
        /// Shows new window to change password and sends data to server
        /// </summary>
        private async void ChangePassword()
        {
            ChangePassword cp = new ChangePassword();

            cp.ShowDialog();

            if (cp.DialogResult.Value)
            {
                textBoxPassword.Password = cp.textBoxNewPassword.Password;

                buffer = Encoding.ASCII.GetBytes("change");
                await stream.WriteAsync(buffer, 0, buffer.Length);

                buffer = new byte[85];

                buffer = Encoding.ASCII.GetBytes(textBoxLogin.Text + ";" + textBoxPassword.Password);
                await stream.WriteAsync(buffer, 0, buffer.Length);

                buffer = new byte[1024];

                await stream.ReadAsync(buffer, 0, buffer.Length);
                string data = Encoding.ASCII.GetString(buffer).Replace("\0", "");

                if (data.Contains("Error"))
                {
                    MessageBox.Show("Can't change password, try again", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("Password Changed", "Password Changed",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        /// <summary>
        /// Saves weather data to file
        /// </summary>
        private async void SaveWeather()
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog
                {
                    FileName = "Weather forecast",
                    DefaultExt = ".text",
                    Filter = "Text documents (.txt)|*.txt"
                };

                dlg.ShowDialog();

                string filePath = dlg.FileName;

                using var writer = new StreamWriter(filePath);

                await writer.WriteAsync(ClientLogTextBox.Text);

                MessageBox.Show($"File has been sucessfully saved\n{filePath}", "Saved",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                OpenTxtFile(filePath);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during saving file\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Start txt file after saving logs
        /// </summary>
        /// <param name="filePath">Log file path</param>
        private static void OpenTxtFile(string filePath)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            };

            Process.Start(processStartInfo);
        }

        /// <summary>
        /// Quits client application
        /// </summary>
        /// <param name="sender">button object</param>
        /// <param name="e">button event</param>
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            switch (MessageBox.Show("Are you sure?", "Closing application",
                MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
            {
                case MessageBoxResult.Yes:
                    if (connected)
                    {
                        client.Close();
                    }
                    Close();
                    break;
                case MessageBoxResult.No:
                case MessageBoxResult.Cancel:
                default:
                    break;
            }
        }

        /// <summary>
        /// Check if weather period is a date or days number
        /// </summary>
        private void CheckDateForSaving()
        {
            var weatherPeriod = textBoxDate.Text;

            if (int.TryParse(weatherPeriod, out _) || Regex.IsMatch(weatherPeriod, @"((0[1-9]|[12]\d|3[01])-(0[1-9]|1[0-2])-([12]\d{3}))")
                || string.IsNullOrEmpty(weatherPeriod))
            {
                if (Regex.IsMatch(weatherPeriod, @"((0[1-9]|[12]\d|3[01])-(0[1-9]|1[0-2])-([12]\d{3}))"))
                {
                    switch (MessageBox.Show("Weather period is a date\nIt will expire\nWould you like to save it anyway?",
                        "Potential date issue", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                    {
                        case MessageBoxResult.Yes:
                            SaveLocationAndTime(weatherPeriod);
                            break;
                        case MessageBoxResult.No:
                        case MessageBoxResult.Cancel:
                        default:
                            break;
                    }
                }
                else
                {
                    SaveLocationAndTime(weatherPeriod);
                }
            }
            else
            {
                MessageBox.Show("Incorrect weather period\nEnter days number or date (eg. DD-MM-YYYY)", "Weather period error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Save date and favourite location na server's database
        /// </summary>
        /// <param name="weatherPeriod"></param>
        private async void SaveLocationAndTime(string weatherPeriod)
        {
            buffer = Encoding.ASCII.GetBytes("favourite");
            await stream.WriteAsync(buffer, 0, buffer.Length);

            buffer = new byte[85];

            buffer = Encoding.ASCII.GetBytes($"{textBoxLogin.Text};{textBoxLocation.Text};{weatherPeriod}");
            await stream.WriteAsync(buffer, 0, buffer.Length);

            buffer = new byte[1024];

            await stream.ReadAsync(buffer, 0, buffer.Length);
            string data = Encoding.ASCII.GetString(buffer).Replace("\0", "");

            if (data.Contains("Error"))
            {
                MessageBox.Show("Can't save favourite location, try again", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Favourite location saved", "Location saved",
                    MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (connected)
            {
                DisconnectFromServer();

                return;
            }
            else
            {
                ConnectToServer();
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            HandleLogin();
        }

        private void GetWeatherButton_Click(object sender, RoutedEventArgs e)
        {
            SendLocationAndDate();
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePassword();
        }

        private void SaveWeatherButton_Click(object sender, RoutedEventArgs e)
        {
            SaveWeather();
        }

        private void SaveFavouriteLocationButton_Click(object sender, RoutedEventArgs e)
        {
            CheckDateForSaving();
        }

        private void ClearWeatherForecast_Click(object sender, RoutedEventArgs e)
        {
            ClientLogTextBox.Clear();
        }
    }
}
