using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT_Logic
{
    public enum LoginType
    {
        SSH,
        Telnet,
        SFTP,
        SCP
    }

    public class Login
    {
        public int Port;

        public LoginType Type;

        public string Password;

        public string Username;

        public int ID;

        public Login(string Username, string Password, int Port, LoginType type)
        {
            this.Username = Username;
            this.Password = Password;
            this.Port = Port;
            this.Type = type;
        }

        //KI start (Claude Opus 4.8, prompt 24): SSH and SFTP ride the same transport (same port + credentials),
        // so a login of either type also serves the other. SCP and Telnet stay distinct. This lets one stored
        // SSH login open an SFTP session (and vice versa) without the user adding a second login.
        public bool Covers(LoginType wanted)
        {
            if (Type == wanted) { return true; }
            bool sshOrSftp = Type == LoginType.SSH || Type == LoginType.SFTP;
            bool wantSshOrSftp = wanted == LoginType.SSH || wanted == LoginType.SFTP;
            return sshOrSftp && wantSshOrSftp;
        }
        //KI end
    }
}
