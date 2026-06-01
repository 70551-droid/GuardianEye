namespace GuardianEye.Admin;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var database = new DatabaseService();
        var login = new LoginForm(database);

        if (login.ShowDialog() == DialogResult.OK)
        {
            var sessionManager = new SessionManager();
            var tcpServer = new TcpServer(sessionManager, database);
            var udpBroadcaster = new UdpBroadcaster();
            var dashboard = new DashboardForm(tcpServer, udpBroadcaster, sessionManager, database);
            Application.Run(dashboard);
        }
    }
}
