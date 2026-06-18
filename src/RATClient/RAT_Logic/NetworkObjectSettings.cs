using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT_Logic
{
    public class NetworkObjectSettings
    {
        // TODO: See if should be read only

        public List<Login> Logins = new List<Login>();

        public SnmpSettings Snmp;

        public void AddLogin(Login login)
        {
            Logins.Add(login);
            // TODO: Add Login to Database


        }

        public void RemoveLogin(Login login)
        {
            throw new NotImplementedException();

            // TODO

            // TODO: Remove Login from Database
        }

        public void SetSnmpSettings(SnmpSettings snmp)
        {
            this.Snmp = snmp;

            // TODO

            // TODO: Update SNMP Settings in Database
        }

        public List<Login> GetAllLoginsByType(LoginType type)
        {
            //KI start (Claude Opus 4.8, prompt 24): use Covers() so an SSH login also matches SFTP (and vice versa)
            return Logins.Where(l => l.Covers(type)).ToList();
            //KI end
        }

        //KI start (Claude Opus 4.8, prompt 24): first stored login that can serve the wanted protocol, or null.
        public Login? FindLoginFor(LoginType type)
        {
            return Logins.FirstOrDefault(l => l.Covers(type));
        }
        //KI end
    }
}
