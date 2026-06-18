using RAT_Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT_Data
{
    //KI start (Claude Opus 4.8, prompt 24): local-only / offline connection. Used by the "Use locally only" button
    // on the login screen: NOTHING is sent to a server. Every call just succeeds in memory (assigns a fake id where
    // a real backend would). The topology starts empty and changes live only for this session.
    public class DatabaseConnectionMock : IDatabaseConnection
    {
        private string _ip = "127.0.0.1";
        public string Ip
        {
            get => _ip;
            set { _ip = value; } // accept anything in local mode
        }
        private int _port = 0;
        public int Port
        {
            get => _port;
            set { _port = value; }
        }
        public User User { get; set; }

        // fake auto-increment ids so created objects look "saved"
        private int _nextId = 1;
        private int NextId() => _nextId++;

        public DatabaseConnectionMock()
        {
            // a local-only user that can do everything (no real account exists offline)
            User = new User("local", "", 0, 100, true);
        }

        //Graph — local mode starts with an empty canvas
        public Task<NetworkObjectGraph> GetNetworkGraph() =>
            Task.FromResult(new NetworkObjectGraph { networkObjects = new List<NetworkObject>() });

        //NetworkObject
        public Task<NetworkObject> AddNetworkObject(NetworkObject networkObject)
        {
            if (networkObject.ID <= 0) { networkObject.ID = NextId(); }
            return Task.FromResult(networkObject);
        }
        public Task EditNetworkObject(NetworkObject networkObject) => Task.CompletedTask;
        public Task DeleteNetworkObject(NetworkObject networkObject) => Task.CompletedTask;

        //NetworkObjectInterface
        public Task<NetworkObjectInterface> AddInterface(NetworkObjectInterface networkObjectInterface, NetworkObject networkObject)
        {
            if (networkObjectInterface.ID <= 0) { networkObjectInterface.ID = NextId(); }
            networkObjectInterface.NetworkObjectId = networkObject.ID;
            return Task.FromResult(networkObjectInterface);
        }
        public Task EditInterface(NetworkObjectInterface networkObjectInterface) => Task.CompletedTask;
        public Task DeleteInterface(NetworkObjectInterface networkObjectInterface) => Task.CompletedTask;

        //NetworkConnection
        public Task<NetworkConnection> AddConnection(NetworkConnection networkConnection, NetworkObjectInterface interface1, NetworkObjectInterface interface2)
        {
            if (networkConnection.ID <= 0) { networkConnection.ID = NextId(); }
            return Task.FromResult(networkConnection);
        }
        public Task EditConnection(NetworkConnection networkConnection) => Task.CompletedTask; // KI (prompt 25)
        public Task DeleteConnection(NetworkConnection networkConnection) => Task.CompletedTask;

        //Permissions — in local mode the single local user owns everything
        public Task<List<AccessRight>> GetNetworkObjectPermissions(NetworkObject networkObject) =>
            Task.FromResult(new List<AccessRight>());
        public Task SetPermission(NetworkObject networkObject, NetworkUser targetUser, AccesRights right) => Task.CompletedTask;
        public Task DeletePermission(NetworkObject networkObject, AccessRight accessRight) => Task.CompletedTask;

        //UserDeviceLogins
        public Task<List<Login>> GetUserDeviceLogin(NetworkObject networkObject) =>
            Task.FromResult(new List<Login>());
        public Task<Login> AddUserDeviceLogin(Login login, NetworkObject networkObject)
        {
            if (login.ID <= 0) { login.ID = NextId(); }
            return Task.FromResult(login);
        }
        public Task EditUserDeviceLogin(Login login) => Task.CompletedTask;
        public Task DeletetUserDeviceLogin(Login login) => Task.CompletedTask;

        //User
        public Task<List<User>> GetAllUsers() => Task.FromResult(new List<User> { User });
        public Task<User> Login() => Task.FromResult(User);
        public Task<User> AddUser(User user)
        {
            if (user.ID <= 0) { user.ID = NextId(); }
            return Task.FromResult(user);
        }
        public Task EditUser(User user) => Task.CompletedTask;
        public Task DeleteUser(User user) => Task.CompletedTask;

        //UserSettings
        public Task EditUserSettings(UserSettings userSettings) => Task.CompletedTask;
        public Task<UserSettings> GetUserSettings() => Task.FromResult(new UserSettings());
    }
    //KI end
}
