using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT_Data
{
    public class Account
    {
        public User? User { get; set; }


        public void Login(string username, string password)
        {
            // TODO
            throw new NotImplementedException();
        }

        public void Logout()
        {
            this.User = null;
        }

        public void Register()
        {
            // TODO
            throw new NotImplementedException();
        }

        public void Edit()
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
