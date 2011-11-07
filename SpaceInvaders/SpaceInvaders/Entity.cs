using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace SpaceInvaders
{
    interface IEntity
    {
        void Update(GameTime gameTime);
        void LoadContent(ContentManager Content);
        void Draw(GameTime gameTime);
        BoundingSphere BoundingSphere { get; }
    }
}
