
namespace WeatherClient
{
    partial class ClientWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBoxIPAddress = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.ClientLogTextBox = new System.Windows.Forms.TextBox();
            this.textBoxLogin = new System.Windows.Forms.TextBox();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.LoginButton = new System.Windows.Forms.Button();
            this.textBoxPort = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxLocation = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.GetWeatherButton = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxDate = new System.Windows.Forms.TextBox();
            this.ChangePasswordButton = new System.Windows.Forms.Button();
            this.SaveWeatherButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxIPAddress
            // 
            this.textBoxIPAddress.Location = new System.Drawing.Point(113, 14);
            this.textBoxIPAddress.Name = "textBoxIPAddress";
            this.textBoxIPAddress.Size = new System.Drawing.Size(154, 23);
            this.textBoxIPAddress.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(65, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 21);
            this.label1.TabIndex = 1;
            this.label1.Text = "IP";
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(143, 72);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(75, 23);
            this.buttonConnect.TabIndex = 2;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // ClientLogTextBox
            // 
            this.ClientLogTextBox.Location = new System.Drawing.Point(334, 14);
            this.ClientLogTextBox.Multiline = true;
            this.ClientLogTextBox.Name = "ClientLogTextBox";
            this.ClientLogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ClientLogTextBox.Size = new System.Drawing.Size(512, 493);
            this.ClientLogTextBox.TabIndex = 3;
            // 
            // textBoxLogin
            // 
            this.textBoxLogin.Location = new System.Drawing.Point(113, 125);
            this.textBoxLogin.Name = "textBoxLogin";
            this.textBoxLogin.Size = new System.Drawing.Size(154, 23);
            this.textBoxLogin.TabIndex = 4;
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(113, 154);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.Size = new System.Drawing.Size(154, 23);
            this.textBoxPassword.TabIndex = 5;
            this.textBoxPassword.UseSystemPasswordChar = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label2.Location = new System.Drawing.Point(39, 123);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 21);
            this.label2.TabIndex = 6;
            this.label2.Text = "Login";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label3.Location = new System.Drawing.Point(30, 152);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 21);
            this.label3.TabIndex = 7;
            this.label3.Text = "Password";
            // 
            // LoginButton
            // 
            this.LoginButton.Enabled = false;
            this.LoginButton.Location = new System.Drawing.Point(143, 183);
            this.LoginButton.Name = "LoginButton";
            this.LoginButton.Size = new System.Drawing.Size(75, 23);
            this.LoginButton.TabIndex = 8;
            this.LoginButton.Text = "Login";
            this.LoginButton.UseVisualStyleBackColor = true;
            this.LoginButton.Click += new System.EventHandler(this.LoginButton_Click);
            // 
            // textBoxPort
            // 
            this.textBoxPort.Location = new System.Drawing.Point(113, 43);
            this.textBoxPort.Name = "textBoxPort";
            this.textBoxPort.Size = new System.Drawing.Size(154, 23);
            this.textBoxPort.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label4.Location = new System.Drawing.Point(65, 45);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 21);
            this.label4.TabIndex = 10;
            this.label4.Text = "Port";
            // 
            // textBoxLocation
            // 
            this.textBoxLocation.Location = new System.Drawing.Point(163, 257);
            this.textBoxLocation.Name = "textBoxLocation";
            this.textBoxLocation.Size = new System.Drawing.Size(154, 23);
            this.textBoxLocation.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label5.Location = new System.Drawing.Point(3, 255);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(152, 21);
            this.label5.TabIndex = 12;
            this.label5.Text = "Location eg. Warsaw";
            // 
            // GetWeatherButton
            // 
            this.GetWeatherButton.Enabled = false;
            this.GetWeatherButton.Location = new System.Drawing.Point(48, 367);
            this.GetWeatherButton.Name = "GetWeatherButton";
            this.GetWeatherButton.Size = new System.Drawing.Size(109, 23);
            this.GetWeatherButton.TabIndex = 13;
            this.GetWeatherButton.Text = "Get weather";
            this.GetWeatherButton.UseVisualStyleBackColor = true;
            this.GetWeatherButton.Click += new System.EventHandler(this.GetWeatherButton_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label6.Location = new System.Drawing.Point(3, 284);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(154, 42);
            this.label6.TabIndex = 15;
            this.label6.Text = "Date eg. 30-11-2020\r\n or number of days";
            // 
            // textBoxDate
            // 
            this.textBoxDate.Location = new System.Drawing.Point(163, 303);
            this.textBoxDate.Name = "textBoxDate";
            this.textBoxDate.Size = new System.Drawing.Size(154, 23);
            this.textBoxDate.TabIndex = 14;
            // 
            // ChangePasswordButton
            // 
            this.ChangePasswordButton.Enabled = false;
            this.ChangePasswordButton.Location = new System.Drawing.Point(124, 212);
            this.ChangePasswordButton.Name = "ChangePasswordButton";
            this.ChangePasswordButton.Size = new System.Drawing.Size(112, 23);
            this.ChangePasswordButton.TabIndex = 16;
            this.ChangePasswordButton.Text = "Change password";
            this.ChangePasswordButton.UseVisualStyleBackColor = true;
            this.ChangePasswordButton.Click += new System.EventHandler(this.ChangePasswordButton_Click);
            // 
            // SaveWeatherButton
            // 
            this.SaveWeatherButton.Enabled = false;
            this.SaveWeatherButton.Location = new System.Drawing.Point(182, 367);
            this.SaveWeatherButton.Name = "SaveWeatherButton";
            this.SaveWeatherButton.Size = new System.Drawing.Size(103, 23);
            this.SaveWeatherButton.TabIndex = 17;
            this.SaveWeatherButton.Text = "Save weather";
            this.SaveWeatherButton.UseVisualStyleBackColor = true;
            this.SaveWeatherButton.Click += new System.EventHandler(this.SaveWeatherButton_Click);
            // 
            // ClientWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(858, 540);
            this.Controls.Add(this.SaveWeatherButton);
            this.Controls.Add(this.ChangePasswordButton);
            this.Controls.Add(this.textBoxDate);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.GetWeatherButton);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBoxLocation);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxPort);
            this.Controls.Add(this.LoginButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.textBoxLogin);
            this.Controls.Add(this.ClientLogTextBox);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxIPAddress);
            this.Name = "ClientWindow";
            this.Text = "Weather Client";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxIPAddress;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.TextBox ClientLogTextBox;
        private System.Windows.Forms.TextBox textBoxLogin;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button LoginButton;
        private System.Windows.Forms.TextBox textBoxPort;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxLocation;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button GetWeatherButton;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxDate;
        private System.Windows.Forms.Button ChangePasswordButton;
        private System.Windows.Forms.Button SaveWeatherButton;
    }
}

