using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT_Logic
{
    //KI start (Claude Opus 4.8, prompt 1/11): logic-layer identity of a user.
    /// <summary>
    /// A lightweight identity of a user inside the logic layer.
    /// RAT_Logic cannot reference RAT_Data.User (that would be a circular project
    /// reference), so the WPF layer maps the logged-in RAT_Data.User onto this.
    /// Carries the account-level flags the rights logic needs (CanCreate, Privileges).
    /// </summary>
    public class NetworkUser
    {
        //KI start (Claude Opus 4.8, prompt 17): these were public *fields*. WPF data binding only binds to
        // *properties*, so {Binding UserName} on the PermUserCombo silently showed blank items. Auto-properties
        // are source-compatible with the existing field reads/writes and make the bindings resolve.
        public string UserName { get; set; }
        public int ID { get; set; }

        /// <summary>Account is allowed to create new network objects.</summary>
        public bool CanCreate { get; set; }

        /// <summary>Global account tier (not used to gate per-object rights here).</summary>
        public int Privileges { get; set; }
        //KI end

        public NetworkUser(string userName, int id, bool canCreate = false, int privileges = 10)
        {
            UserName = userName;
            ID = id;
            CanCreate = canCreate;
            Privileges = privileges;
        }

        public override string ToString() => $"{UserName} (#{ID})";
    }
    //KI end
}
