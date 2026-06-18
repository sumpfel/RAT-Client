using RAT_Data;
using RAT_WPF.Stores;
using RAT_WPF.ViewModels;

namespace RAT_WPF.Commands
{
    //KI start (Claude Opus 4.8, prompt 24): "Use locally only" — skip the login + database connection entirely.
    // Shows a reminder that nothing is saved, installs the in-memory DatabaseConnectionMock as the active
    // connection (so every save just succeeds locally), seeds a local user, and goes to the topology.
    public class LocalOnlyCommand : CommandBase
    {
        private readonly NavigationStore _navigationStore;

        public LocalOnlyCommand(NavigationStore navigationStore)
        {
            _navigationStore = navigationStore;
        }

        public override void Execute(object? parameter)
        {
            RatDialog.Show(
                "Local-only mode",
                "You're starting WITHOUT a server connection.\n\n" +
                "Nothing will be saved to a database — devices, interfaces, logins and connections live only " +
                "for this session and are gone when you close the app. SSH/SFTP/SCP/Telnet to real devices still " +
                "works; only persistence is disabled.",
                "Icon.NoConnection");

            // in-memory mock connection: every operation just succeeds, nothing leaves the machine
            DatabaseConnectionMock mock = new DatabaseConnectionMock();
            DatabaseConnectionStore.Current = mock;

            // a local user that may create / own everything (no real account exists offline)
            RAT_Logic.Session.CurrentUser = new RAT_Logic.NetworkUser("local", 0, canCreate: true, privileges: 100);

            _navigationStore.CurrentViewModel = new TopologyViewModel(_navigationStore);
        }
    }
    //KI end
}
