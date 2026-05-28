using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT_Logic
{
    public enum NetworkConnectionType
    {
        Wireless,
        Wired,
    }

    public class NetworkConnection
    {
        private NetworkObectInterface[] networkObectInterfaces;
        public int Speed;
        public NetworkObjectType Type;
        public string Note;
        public string Name;

        NetworkConnection(NetworkObectInterface networkObectInterface1, NetworkObectInterface networkObectInterface2, int speed, NetworkObjectType type, string note, string name)
        {
            this.networkObectInterfaces = new NetworkObectInterface[2] {networkObectInterface1, networkObectInterface2};
            Speed = speed;
            Type = type;
            Note = note;
            Name = name;
        }

        public NetworkObectInterface GetConnectedInterface(NetworkObectInterface networkObectInterface)
        {
            if (networkObectInterface == networkObectInterfaces[0])
            {
                return networkObectInterfaces[1];
            }else if (networkObectInterface == networkObectInterfaces[1])
            {
                return networkObectInterfaces[0];
            }else
            {
                throw new Exception($"Network Connection is not connected to {networkObectInterface}");
            }
        }
    }
}
