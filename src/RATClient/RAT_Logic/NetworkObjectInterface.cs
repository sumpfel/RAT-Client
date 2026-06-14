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
    }
}
