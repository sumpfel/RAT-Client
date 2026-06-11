using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT_Logic
{
    //KI start (Claude Opus 4.8, prompt 1): permission model so a NetworkObject can grant different users different rights
    /// <summary>
    /// The kinds of access a user can be granted on a <see cref="NetworkObject"/>.
    /// Flags so a single permission can combine several rights.
    /// </summary>
    [Flags]
    public enum AccessRight
    {
        None = 0,
        View = 1 << 0,   // see the device and its specs
        Edit = 1 << 1,   // change name / specs / interfaces
        Connect = 1 << 2,   // open ssh / sftp / scp sessions
        Snmp = 1 << 3,   // use the MIB browser (get/set)
        Manage = 1 << 4,   // change who else may access this device
        Full = View | Edit | Connect | Snmp | Manage
    }
    //KI end
}
