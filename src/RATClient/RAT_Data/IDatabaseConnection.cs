using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT_Data
{
    public interface IDatabaseConnection
    {
        string ip
        {
            get;
            set;
        }

        string port
        {
            get;
            set;
        }
    }
}
