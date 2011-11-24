﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SpaceInvaders
{
    class ClientScreen : IGameScreen
    {
        #region IGameScreen Members

        Texture2D bgImg;
        QuadRenderer qR;
        Effect texDraw;

        public ClientScreen()
        {
            qR = new QuadRenderer();
        }

        public void Update(GameTime gameTime)
        {
            
        }

        public void InjectInput(KeyboardState keyboardState, MouseState mouseState)
        {
            
        }

        public void Draw(GameTime gameTime, GraphicsDevice graphicsDevice)
        {
            graphicsDevice.Clear(Color.White);
            texDraw.Parameters["tex"].SetValue(bgImg);
            texDraw.Techniques[0].Passes[0].Apply();
            qR.RenderQuad(graphicsDevice, -Vector2.One, Vector2.One, new Vector2(1.0f / Game1.width, 1.0f / Game1.height));
        }

        public void Remove()
        {
            
        }

        public void LoadContent(ContentManager contentManager)
        {
            bgImg = contentManager.Load<Texture2D>("background");
            texDraw = contentManager.Load<Effect>("shaders/texdraw");
        }

        #endregion
    }
}
