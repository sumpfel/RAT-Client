using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT_Logic
{
    //KI start (Claude Opus 4.8, prompt 1): permission object = which user + which rights
    /// <summary>
    /// A lightweight identity of a user inside the logic layer.
    /// RAT_Logic cannot reference RAT_Data.User (that would be a circular project
    /// reference), so the WPF layer maps the logged-in RAT_Data.User onto this.
    /// </summary>
    public class NetworkUser
    {
        public string UserName;
        public int ID;

        public NetworkUser(string userName, int id)
        {
            UserName = userName;
            ID = id;
        }

        public override string ToString() => $"{UserName} (#{ID})";
    }

    /// <summary>
    /// Couples a <see cref="NetworkUser"/> with the <see cref="AccessRight"/>s
    /// they hold on a single <see cref="NetworkObject"/>.
    /// </summary>
    public class Permission
    {
        public NetworkUser User;
        public AccessRight Rights;

        public Permission(NetworkUser user, AccessRight rights)
        {
            User = user;
            Rights = rights;
        }

        public bool Has(AccessRight right) => (Rights & right) == right;
    }
    //KI end
}
