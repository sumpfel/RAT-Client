using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT_Data
{
    public class UserSettings
    {
        //KI start (Claude Opus 4.8, prompt: link the C# frontend with the RAT-Backend database):
        // made these public so DatabaseConnection.EditUserSettings can send them to the backend
        // (PUT /user/settings/ expects zoom / show_ports / show_interfaces). Defaults mirror the
        // backend's DBUserSettings defaults.
        public int Zoom = 100;
        public bool ShowPorts;
        public bool ShowInterfaces;
        //KI end
    }
}
