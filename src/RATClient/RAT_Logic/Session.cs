using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT_Logic
{
    //KI start (Claude Opus 4.8, prompt 1): app-wide current user so PC network objects can be owned by whoever is logged in
    /// <summary>
    /// Holds the user currently logged into the client. Set once at login.
    /// Lives in the logic layer (as a <see cref="NetworkUser"/>) so both the
    /// WPF and logic layers can read it without a circular reference on RAT_Data.
    /// </summary>
    public static class Session
    {
        public static NetworkUser? CurrentUser { get; set; }
    }
    //KI end
}
