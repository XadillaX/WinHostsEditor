using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace WinHostEditor
{
    class DataRow
    {
        public int Num;
        public String Domain;
        public String IP;

        public bool Valid;
        public String Origin;

        public static bool IsIP(String IP)
	    {
            IPAddress address;
            if (IPAddress.TryParse(IP, out address)) return true;
            else return false;
        }
    }
}
