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
        FTP
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
    }
}
