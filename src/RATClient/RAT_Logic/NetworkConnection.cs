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
        public NetworkObjectInterface[] networkObectInterfaces;
        public int Speed;
        public NetworkConnectionType Type;
        public string Note;
        public string Name;

        //KI start (Claude Opus 4.8, prompt 14): database identity (0 == not saved yet) so a connection can be
        // deleted on the backend. Filled by DatabaseConnection when the graph is loaded / a connection is created.
        public int ID;
        //KI end

        public NetworkConnection(NetworkObjectInterface networkObectInterface1, NetworkObjectInterface networkObectInterface2, int speed, NetworkConnectionType type, string note, string name)
        {
            this.networkObectInterfaces = new NetworkObjectInterface[2] {networkObectInterface1, networkObectInterface2};
            Speed = speed;
            Type = type;
            Note = note;
            Name = name;
        }

        public NetworkObjectInterface GetConnectedInterface(NetworkObjectInterface networkObectInterface)
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
