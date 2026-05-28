using System;
using System.Windows.Forms;

namespace GuardianEye.Client
{
    public partial class ClientMainForm : Form
    {
        public ClientMainForm()
        {
            InitializeComponent();
        }

        public void UpdateTimeDisplay(int totalSeconds)
        {
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            labelTime.Text = $"{minutes:D2}:{seconds:D2}";
        }

        private void buttonLogout_Click(object sender, EventArgs e)
        {
            // TODO: Implement logout logic
            MessageBox.Show("Logout clicked");
        }

        private void buttonRequestTime_Click(object sender, EventArgs e)
        {
            // TODO: Implement request time logic
            MessageBox.Show("Request time clicked");
        }
    }
}