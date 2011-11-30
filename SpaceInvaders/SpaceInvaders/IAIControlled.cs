using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SpaceInvaders
{
    interface IAIControlled
    {
        void Think(GameTime gameTime);
        Vector2 Target { get; }
        List<IEntity> creationList { set; }
    }
}
