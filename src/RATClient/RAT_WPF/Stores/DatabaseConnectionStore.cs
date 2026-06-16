using RAT_Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT_WPF.Stores
{
    //KI start (Claude Opus 4.8, prompt: link the C# frontend with the RAT-Backend database):
    // App-wide holder for the active IDatabaseConnection. After a successful login the
    // LoginCommand stores the connected DatabaseConnection here so the TopologyViewModel
    // (and any other view) can read/write the database without re-authenticating.
    //
    // Built as a tiny static store to match the existing app-wide RAT_Logic.Session.CurrentUser
    // pattern (no DI container is used in this project).
    public static class DatabaseConnectionStore
    {
        /// <summary>The connection authenticated at login, or null while logged out.</summary>
        public static IDatabaseConnection? Current { get; set; }

        //KI start (Claude Opus 4.8, prompt 15): remember the last server the user logged into, so after a
        // logout the login screen only has to ask for username + password again (IP/port are pre-filled).
        public static string? LastServerIp { get; set; }
        public static int? LastServerPort { get; set; }
        //KI end
    }
    //KI end
}
