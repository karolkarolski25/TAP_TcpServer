using System;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;

namespace WeatherClient
{
    public partial class ClientWindow : Form
    {
        private string ipAddress;
        private int port;
        private TcpClient klient;
        private byte[] buffer;
        private NetworkStream stream;
        private bool connected = false;

        public ClientWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Function that connects to server and receives first message from server
        /// </summary>
        private void ConnectToServer()
        {
            ipAddress = textBoxIPAddress.Text;

            if (!int.TryParse(textBoxPort.Text, out port) || (port < 1024 || port > 65535))
            {
                MessageBox.Show("Wrong port number, try again", "Port error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            try
            {
                klient = new TcpClient(ipAddress, port);
            }
            catch
            {
                MessageBox.Show("Cannot connect to server", "Connection error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            stream = klient.GetStream();

            buffer = new byte[85];
            stream.Read(buffer, 0, 85);

            if (Encoding.ASCII.GetString(buffer).Replace("\0", "") == "Login: ")
            {
                textBox1.Text = "Log in";
                buttonLogin.Enabled = true;
                buttonConnect.Text = "Disconnect";
                connected = true;
            }
        }

        /// <summary>
        /// Function that disconnects from server
        /// </summary>
        private void DisconnectFromServer()
        {
            klient.Close();
            buttonConnect.Text = "Connect";
            connected = false;
            buttonLogin.Enabled = false;
            buttonGetWeather.Enabled = false;
            buttonChangePassword.Enabled = false;
            buffer = new byte[85];
            textBox1.Text = "";
        }

        /// <summary>
        /// Function that sends login and password to server
        /// </summary>
        private void HandleLogin()
        {
            buffer = Encoding.ASCII.GetBytes(textBoxLogin.Text);

            stream.Write(buffer, 0, buffer.Length);
            stream.Write(buffer, 0, 2);

            buffer = new byte[85];

            stream.Read(buffer, 0, 85);

            string data = Encoding.ASCII.GetString(buffer).Replace("\0", "");

            if (Encoding.ASCII.GetString(buffer).Replace("\0", "") == "Password: ")
            {
                buffer = Encoding.ASCII.GetBytes(textBoxPassword.Text);

                stream.Write(buffer, 0, buffer.Length);
                stream.Write(buffer, 0, 2);

                buffer = new byte[85];

                stream.Read(buffer, 0, buffer.Length);

                string message = Encoding.ASCII.GetString(buffer).Replace("\0", "");

                if (message == "Account not found, do you want to create new account? (Y/N): ")
                {
                    if(!HandleRegistration())
                    {
                        return;
                    }
                }

                buffer = new byte[1024];
                stream.Read(buffer, 0, buffer.Length);

                textBox1.Text = "Enter Location and number of days or date";

                buttonGetWeather.Enabled = true;
                buttonLogin.Enabled = false;
                buttonChangePassword.Enabled = true;
            }
        }

        /// <summary>
        /// Function that shows popup message if user wants to register account
        /// </summary>
        /// <returns>True if user wants to register, false if user don't want to register</returns>
        private bool HandleRegistration()
        {
            switch(MessageBox.Show("Do you want to create new account", "Account not found", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                case DialogResult.Yes:
                    buffer = Encoding.ASCII.GetBytes("Y");
                    stream.Write(buffer, 0, buffer.Length);
                    buffer = new byte[85];
                    stream.Read(buffer, 0, buffer.Length);
                    return true;
                case DialogResult.No:
                    buffer = Encoding.ASCII.GetBytes("N");
                    stream.Write(buffer, 0, buffer.Length);
                    buffer = new byte[2];
                    stream.Write(buffer, 0, 2);
                    buffer = new byte[85];
                    stream.Read(buffer, 0, 85);
                    return false;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Function that sends location and data to server
        /// </summary>
        private void SendLocationAndData()
        {
            string location = textBoxLocation.Text;
            string daysPeriod = textBoxDate.Text;

            if (string.IsNullOrWhiteSpace(location) || string.IsNullOrWhiteSpace(daysPeriod))
            {
                MessageBox.Show("Weather location and days period cannot be empty", "Empty weather data", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                buffer = Encoding.ASCII.GetBytes(textBoxLocation.Text);

                stream.Write(buffer, 0, buffer.Length);

                buffer = new byte[1024];
                stream.Read(buffer, 0, buffer.Length);

                buffer = Encoding.ASCII.GetBytes(daysPeriod);

                stream.Write(buffer, 0, buffer.Length);

                buffer = new byte[2048];

                string data = "";

                do
                {
                    stream.Read(buffer, 0, buffer.Length);
                    data = Encoding.ASCII.GetString(buffer).Replace("\0", "");

                    if (data.Contains("Incorrect weather period, try again"))
                    {
                        MessageBox.Show("Incorrect weather period, try different formatting", "Format error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);

                        return;
                    }
                } while (!data.Contains("Temperature"));

                textBox1.Text = data;
            }
        }

        private void ChangePassword()
        {
            ChangePassword cp = new ChangePassword();

            if(cp.ShowDialog(this) == DialogResult.OK)
            {
                textBoxPassword.Text = cp.textBoxNewPassword.Text;

                buffer = Encoding.ASCII.GetBytes("change");
                stream.Write(buffer, 0, buffer.Length);

                buffer = new byte[85];

                buffer = Encoding.ASCII.GetBytes(textBoxLogin.Text + ";" + textBoxPassword.Text);
                stream.Write(buffer, 0, buffer.Length);

                buffer = new byte[1024];

                stream.Read(buffer, 0, buffer.Length);
                string data = Encoding.ASCII.GetString(buffer).Replace("\0", "");

                if (data.Contains("Error"))
                {
                    MessageBox.Show("Can't change password, try again", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Password Changed", "Password Changed", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            cp.Dispose();
        }

        private void ConnectButton_Click(object sender, EventArgs e)
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

        private void LoginButton_Click(object sender, EventArgs e)
        {
            HandleLogin();
        }

        private void GetWeatherButton_Click(object sender, EventArgs e)
        {
            SendLocationAndData();
        }

        private void ChangePasswordButton_Click(object sender, EventArgs e)
        {
            ChangePassword();
        }
    }
}
