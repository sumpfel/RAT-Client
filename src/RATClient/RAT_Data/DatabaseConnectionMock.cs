using RAT_Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT_Data
{
    public class DatabaseConnectionMock : IDatabaseConnection
    {
        private string _ip;
        public string Ip
        {
            get => _ip;
            set
            {
                if (IP.IsIpv4Valid(value)) {_ip = value;}
                else { throw new FormatException("this is not an IP"); }
            }
        }
        private int _port;
        public int Port
        {
            get => _port;
            set
            {
                if (IP.IsPortVlaid(value)){ _port = value; }
                else { throw new FormatException("this is not a Port"); }
            }
        }
        public User User { get; set; }

        DatabaseConnectionMock(User user,string ip, int port)
        {
            User = user;
            Ip = ip;
            Port = port;
        }


        public Task<NetworkObject> AddNetworkObject(NetworkObject networkObject)
        {
            throw new NotImplementedException();
        }

        public Task<User> AddUser(User user)
        {
            throw new NotImplementedException();
        }

        public Task<Login> AddUserDeviceLogin(Login login)
        {
            throw new NotImplementedException();
        }

        public Task DeleteNetworkObject(NetworkObject networkObject)
        {
            throw new NotImplementedException();
        }

        public Task DeletetUserDeviceLogin(Login login)
        {
            throw new NotImplementedException();
        }

        public Task DeleteUser(User user)
        {
            throw new NotImplementedException();
        }

        public Task EditNetworkObject(NetworkObject networkObject)
        {
            throw new NotImplementedException();
        }

        public Task EditUser(User user)
        {
            throw new NotImplementedException();
        }

        public Task EditUserDeviceLogin(Login login)
        {
            throw new NotImplementedException();
        }

        public Task EditUserSettings(UserSettings userSettings)
        {
            throw new NotImplementedException();
        }

        public Task<List<User>> GetAllUsers()
        {
            throw new NotImplementedException();
        }

        public Task<NetworkObjectGraph> GetNetworkGraph()
        {
            throw new NotImplementedException();
        }

        public Task<List<Login>> GetUserDeviceLogin()
        {
            throw new NotImplementedException();
        }

        public Task<User> Login()
        {
            string userName = "RAT";
            string password = "password";
            if (User.UserName == userName && User.Password == password)
            {
                return Task.FromResult(new User(userName, password, 0));
            }
            else
            {
                throw new AccessViolationException("Wrong User or Password to connecto to the server.");
            }
        }
    }
}
