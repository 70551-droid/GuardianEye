using System;
using System.Windows.Forms;

namespace GuardianEye.Client
{
    public partial class LockScreenForm : Form
    {
        public LockScreenForm()
        {
            InitializeComponent();
        }

        // We can add a method to set a custom message if needed
        public void SetMessage(string message)
        {
            labelMessage.Text = message;
        }
    }
}