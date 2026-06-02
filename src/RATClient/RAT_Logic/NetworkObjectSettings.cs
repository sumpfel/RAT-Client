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

        public List<Login> SshLogins = new List<Login>();

        public List<Login> TelnetLogins = new List<Login>();

        public List<Login> FtpLogins = new List<Login>();

        public SnmpSettings Snmp;

        public void AddLogin(Login login)
        {
            switch (login.Type)
            {
                case LoginType.SSH:
                    SshLogins.Add(login);
                    break;
                case LoginType.Telnet:
                    TelnetLogins.Add(login);
                    break;
                case LoginType.FTP:
                    FtpLogins.Add(login);
                    break;
                default:
                    break;
            }

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
            switch (type)
            {
                case LoginType.SSH:
                    return SshLogins;
                case LoginType.Telnet:
                    return TelnetLogins;
                case LoginType.FTP:
                    return FtpLogins;
                default:
                    return new List<Login>();
            }
        }
    }
}
