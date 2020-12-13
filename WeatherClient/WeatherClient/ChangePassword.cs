using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WeatherClient
{
    public partial class ChangePassword : Form
    {
        public ChangePassword()
        {
            InitializeComponent();

            buttonChange.DialogResult = DialogResult.OK;
        }
    }
}
