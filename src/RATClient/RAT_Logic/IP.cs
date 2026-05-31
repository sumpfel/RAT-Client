using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RAT_Logic
{
    public class IP
    {
        public string IPv4;
        public string IPv6;
        public string IPv4SubnetMask;
        public int IPv6PrefixLength;
        public string IPv4Gateway;

        public static bool IsIpv4Valid(string ipv4)
        {
            if (Regex.IsMatch(ipv4, @"^(((?!25?[6-9])[12]\d|[1-9])?\d\.?\b){4}$")) // Regex from https://stackoverflow.com/questions/5284147/validating-ipv4-addresses-with-regexp
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsPortVlaid(Object port)
        {
            try
            {
                Convert.ToUInt16(port);
                return true;
            }
            catch {  return false; }
        }
    }
}
