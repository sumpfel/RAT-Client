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

        //UserDeviceLogins (ssh, telnet, etc)
        public Task<List<Login>> GetUserDeviceLogin();
        public Task<Login> AddUserDeviceLogin(Login login);
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
        //TODO: AccessRights
        //public Task<>
    }
}
