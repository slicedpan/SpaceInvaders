using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceInvaders
{
    public class PlayerInfo
    {
        public int Score = 0;
        public int Health = 100;
        public int EntityID;
        public bool Alive = true;

        public PlayerInfo(int entityID)
        {
            EntityID = entityID;
        }
    }
}
