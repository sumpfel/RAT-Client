using RAT_Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT_Data
{
    public interface IDatabaseConnection
    {
        string Ip
        {
            get;
            set;
        }

        int Port
        {
            get;
            set;
        }

        User User { get; set; }

        //Graph
        public Task<NetworkObjectGraph> GetNetworkGraph();

        //NetworkObject
        public Task<NetworkObject> AddNetworkObject(NetworkObject networkObject);
        public Task EditNetworkObject(NetworkObject networkObject);
        public Task DeleteNetworkObject(NetworkObject networkObject);

        //KI start (Claude Opus 4.8, prompt 14): interface, connection and permission persistence.
        // These were missing, so the settings window could only ever edit things in memory. They map
        // onto the backend's /networkObjectInterface, /networkObjectConnection and
        // /networkObjectPermission routes. (See DatabaseConnection for the HTTP mapping.)

        //NetworkObjectInterface
        public Task<NetworkObjectInterface> AddInterface(NetworkObjectInterface networkObjectInterface, NetworkObject networkObject);
        public Task EditInterface(NetworkObjectInterface networkObjectInterface);
        public Task DeleteInterface(NetworkObjectInterface networkObjectInterface);

        //NetworkConnection (the two endpoints are given by their interfaces)
        public Task<NetworkConnection> AddConnection(NetworkConnection networkConnection, NetworkObjectInterface interface1, NetworkObjectInterface interface2);
        public Task DeleteConnection(NetworkConnection networkConnection);

        //AccessRights / permissions on a NetworkObject
        public Task<List<AccessRight>> GetNetworkObjectPermissions(NetworkObject networkObject);
        public Task SetPermission(NetworkObject networkObject, NetworkUser targetUser, AccesRights right);
        public Task DeletePermission(NetworkObject networkObject, AccessRight accessRight);
        //KI end

        //UserDeviceLogins (ssh, telnet, etc)
        public Task<List<Login>> GetUserDeviceLogin(NetworkObject networkObject);
        public Task<Login> AddUserDeviceLogin(Login login, NetworkObject networkObject);
        public Task EditUserDeviceLogin(Login login);
        public Task DeletetUserDeviceLogin(Login login);

        //User
        public Task<List<User>> GetAllUsers();
        public Task<User> Login();
        public Task<User> AddUser(User user);
        public Task EditUser(User user);
        public Task DeleteUser(User user);

        //UserSettings
        public Task EditUserSettings(UserSettings userSettings);
        //KI start (Claude Opus 4.8, prompt 22): load the saved settings (zoom / showPorts / showInterfaces) at login
        public Task<UserSettings> GetUserSettings();
        //KI end
    }
}
