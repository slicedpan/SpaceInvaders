using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace SpaceInvaders
{
    public interface IClientEntity
    {
        void Update(GameTime gameTime);
        void LoadContent(ContentManager Content);
        void Draw(GameTime gameTime);
        bool isReadyToRemove { get; }
    }
}
