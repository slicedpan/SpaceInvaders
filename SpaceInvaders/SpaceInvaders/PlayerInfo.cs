using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceInvaders
{
    public class PlayerInfo
    {
        public int Score;
        public int Health;
        public int EntityID;

        public PlayerInfo(int entityID)
        {
            EntityID = entityID;
        }
    }
}
