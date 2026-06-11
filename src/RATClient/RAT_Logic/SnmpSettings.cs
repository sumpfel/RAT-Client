using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT_Logic
{
    public class SnmpSettings
    {
        public string ReadCommunity;

        public string WriteCommunity;

        public int Port = 161;

        public int ID;

        public SnmpSettings(string readCommunity, string writeCommunity, int iD)
        {
            ReadCommunity = readCommunity;
            WriteCommunity = writeCommunity;
            ID = iD;
        }

        //KI start (Claude Opus 4.8, prompt 1): convenience ctor for the MIB browser UI
        public SnmpSettings(string readCommunity, string writeCommunity, int port, int iD)
        {
            ReadCommunity = readCommunity;
            WriteCommunity = writeCommunity;
            Port = port;
            ID = iD;
        }
        //KI end
    }
}
