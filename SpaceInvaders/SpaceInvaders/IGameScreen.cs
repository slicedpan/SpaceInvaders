using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace SpaceInvaders
{
    public interface IGameScreen
    {
        void Update(GameTime gameTime);
        void InjectInput(KeyboardState keyboardState, MouseState mouseState);
        void Draw(GameTime gameTime, GraphicsDevice graphicsDevice);
        void Remove();
        void LoadContent(ContentManager contentManager);
    }
}
