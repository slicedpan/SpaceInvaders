using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SpaceInvaders
{
    class ServerScreen : IGameScreen
    {
        #region IGameScreen Members

        public void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public void InjectInput(KeyboardState keyboardState, MouseState mouseState)
        {
            throw new NotImplementedException();
        }

        public void Draw(GameTime gameTime, GraphicsDevice graphicsDevice)
        {
            throw new NotImplementedException();
        }

        public void Remove()
        {
            throw new NotImplementedException();
        }

        public void LoadContent(ContentManager contentManager)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
