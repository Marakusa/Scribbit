using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Scribbit
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();

            string user = "User";

            user = Environment.UserName;

            licenceText.Text = string.Format(licenceText.Text, user);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://raw.githubusercontent.com/Marakusa/Scribbit/master/LICENSE",
                UseShellExecute = true
            });
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Close();
        }
    }
}
