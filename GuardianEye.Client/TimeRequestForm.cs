using System;
using System.Windows.Forms;

namespace GuardianEye.Client
{
    public partial class TimeRequestForm : Form
    {
    public TimeRequestForm()
    {
        InitializeComponent();
        Load += (_, _) =>
        {
            if (Environment.OSVersion.Version.Build >= 22000)
                UIStyles.EnableMica(Handle);
            UIStyles.EnableRoundedCorners(Handle);
        };
    }

        public int RequestedMinutes => (int)numericUpDownMinutes.Value;
        public string Reason => textBoxReason.Text;

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (RequestedMinutes <= 0)
            {
                MessageBox.Show("Please enter a positive number of minutes.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(Reason))
            {
                MessageBox.Show("Please enter a reason.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}