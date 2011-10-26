using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceInvaders
{
    public class ClientInfo
    {
        int _clientNumber;
        public int ClientNumber
        {
            get
            {
                return _clientNumber;
            }
        }
        public string Name = "";
        public ClientInfo(int clientNumber, string name)
        {
            Name = name;
            _clientNumber = clientNumber;
        }
    }
}
