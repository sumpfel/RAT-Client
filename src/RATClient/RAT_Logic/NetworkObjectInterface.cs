using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT_Logic
{
    public class NetworkObjectInterface
    {
        public string Name = "";
        public int MaxSpeed;
        public NetworkConnection? Connection;
        public IP? IP;

        //KI start (Claude Opus 4.8, prompt 7): software up/down state for modelled (non-host) interfaces
        public bool IsUp = true;
        //KI end

        //KI start (Claude Opus 4.8, prompt 26): open TCP ports found by an nmap port scan (for the ShowPorts view).
        // Not persisted to the backend (no column); a discovery-time annotation shown on the canvas.
        public List<int> OpenPorts = new List<int>();
        //KI end

        //KI start (Claude Opus 4.8, prompt 14): database identity so interfaces can be edited/deleted on the
        // backend. ID == 0 means "not saved yet"; NetworkObjectId links back to the owning NetworkObject row
        // (needed by the backend's networkObjectInterface routes). Both are filled by DatabaseConnection.
        public int ID;
        public int NetworkObjectId;
        //KI end
    }
}
