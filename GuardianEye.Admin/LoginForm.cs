namespace GuardianEye.Admin;

public partial class LoginForm : Form
{
    private readonly DatabaseService _database;

    public LoginForm(DatabaseService database)
    {
        InitializeComponent();
        _database = database;
        Load += (_, _) => UIStyles.EnableAcrylic(Handle);
    }

    private void buttonLogin_Click(object sender, EventArgs e)
    {
        string username = textBoxUsername.Text.Trim();
        string password = textBoxPassword.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            labelError.Text = "Please enter username and password.";
            labelError.Visible = true;
            return;
        }

        if (_database.ValidateAdmin(username, password))
        {
            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            labelError.Text = "Invalid admin credentials.";
            labelError.Visible = true;
        }
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void textBox_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (e.KeyChar == (char)Keys.Enter)
        {
            buttonLogin_Click(sender, e);
        }
    }
}
