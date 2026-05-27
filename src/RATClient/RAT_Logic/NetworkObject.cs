using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace RAT_Logic
{
    public enum NetworkObjectType 
    { 
        Router,
        Switch,
        Server,
        Client
    }
    public class NetworkObject
    {
        public List<NetworkInterface> NetworkInterfaces = new List<NetworkInterface>();

        public NetworkObjectType Type;

        public NetworkObjectSettings Settings;

        public string Name;

        public int ID;

        public void OpenSSH(Login login) // TODO: See if by id is better than by login
        {
            // TODO
            throw new NotImplementedException();
        }

        public void OpenFTP(Login login)
        {
            // TODO
            throw new NotImplementedException();
        }

        public void OpenTelnet(Login login)
        {
            // TODO
            throw new NotImplementedException();
        }

        public void OpenSnmp(Login login)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
