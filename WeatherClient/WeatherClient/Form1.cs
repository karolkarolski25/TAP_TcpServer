using System;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;

namespace WeatherClient
{
    public partial class Form1 : Form
    {
        private string ipAddress;
        private int port;
        TcpClient klient;
        byte[] buffer;
        NetworkStream stream;
        bool connected = false;
        public Form1()
        {
            InitializeComponent();
            buffer = new byte[85];
            textBoxIPAddress.Text = "127.0.0.1";
            textBoxPort.Text = "2048";
            textBoxLogin.Text = "test";
            textBoxPassword.Text = "123";
            textBoxLocation.Text = "Warsaw";
            textBoxDate.Text = "2";
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (connected)
            {
                klient.Close();
                buttonConnect.Text = "Connect";
                connected = false;
                buttonLogin.Enabled = false;
                buttonGetWeather.Enabled = false;
                buffer = new byte[85];
                textBox1.Text = "";
                return;
            }
            ipAddress = textBoxIPAddress.Text;
            try
            {
                port = int.Parse(textBoxPort.Text);
            }
            catch
            {
                MessageBox.Show("Wrong IP Address");
                return;
            }

            if (port < 1024 || port > 65535)
            {
                MessageBox.Show("Wrong port number");
                return;
            }

            try
            {
                klient = new TcpClient(ipAddress, port);
            }
            catch
            {
                MessageBox.Show("Can't connect to server");
                return;
            }

            stream = klient.GetStream();

            stream.Read(buffer, 0, 85);

            if (Encoding.ASCII.GetString(buffer).Replace("\0", "") == "Login: ")
            {
                textBox1.Text = "Log in";
                buttonLogin.Enabled = true;
                buttonConnect.Text = "Disconnect";
                connected = true;
            }

        }

        private void buttonLogin_Click(object sender, EventArgs e)
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
                    DialogResult dialogResult = MessageBox.Show("Do you want to create new account", "Account not found", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        buffer = Encoding.ASCII.GetBytes("Y");
                        stream.Write(buffer, 0, buffer.Length);
                        buffer = new byte[85];
                        stream.Read(buffer, 0, buffer.Length);
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        buffer = Encoding.ASCII.GetBytes("N");
                        stream.Write(buffer, 0, buffer.Length);
                        buffer = new byte[2];
                        stream.Write(buffer, 0, 2);
                        buffer = new byte[85];
                        stream.Read(buffer, 0, 85);
                        return;
                    }
                }

                buffer = new byte[1024];
                stream.Read(buffer, 0, buffer.Length);

                textBox1.Text = "Enter Location and number of days or date";

                buttonGetWeather.Enabled = true;
                buttonLogin.Enabled = false;
            }
        }

        private void buttonGetWeather_Click(object sender, EventArgs e)
        {
            buffer = Encoding.ASCII.GetBytes(textBoxLocation.Text);

            stream.Write(buffer, 0, buffer.Length);

            buffer = new byte[1024];
            stream.Read(buffer, 0, buffer.Length);

            buffer = Encoding.ASCII.GetBytes(textBoxDate.Text);

            stream.Write(buffer, 0, buffer.Length);

            buffer = new byte[2048];
            string data = "";
            do
            {
                stream.Read(buffer, 0, buffer.Length);
                data = Encoding.ASCII.GetString(buffer).Replace("\0", "");
                if(data.Contains("Incorrect weather period, try again"))
                {
                    MessageBox.Show("Incorrect weather period, try different formatting");
                    return;
                }
            } while (!data.Contains(textBoxLocation.Text));

            textBox1.Text = data;
        }

    }
}
