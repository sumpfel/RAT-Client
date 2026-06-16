using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT_Logic
{
    //KI start (Claude Opus 4.8, prompt 11): hierarchical per-user access rights on a NetworkObject.
    // NOTE: RAT_Logic cannot reference RAT_Data (RAT_Data already depends on RAT_Logic -> a project cycle
    // won't compile), so AccessRight references the logic-layer NetworkUser. The WPF/data layer maps the
    // RAT_Data.User onto a NetworkUser. (As written, `User user;` resolved to Lextm.SharpSnmpLib.Security.User,
    // not RAT_Data.User, which is why it compiled but wasn't what was intended.)
    public enum AccesRights
    {
        Hidden = 0, // don't see device or its connections at all; a visible device wired to it is drawn as an anonymous "?" device
        See = 1,    // can see the device and its interfaces (logins are per-user, so a user can add their own ssh logins etc.)
        Edit = 2,   // can change the interfaces and other settings (nmae etc) (cannot delete the object)
        Admin = 3,  // Edit + change permissions of users with LOWER rights on this object (cannot delete the object)
        Owner = 4   // change permissions of admins, grant/remove "Owner" on other users, and delete the object
    }

    public class AccessRight
    {
        public NetworkUser User;
        public AccesRights Rights;

        //KI start (Claude Opus 4.8, prompt 14): backend permission-row id (0 == not persisted yet) so the
        // row can be deleted on the server. Filled by DatabaseConnection.GetNetworkObjectPermissions.
        public int ID;
        //KI end

        public AccessRight(NetworkUser user, AccesRights rights)
        {
            User = user;
            Rights = rights;
        }
    }
    //KI end
}
