using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace SpaceInvaders
{
    public class TestScreen : IGameScreen
    {
        protected RenderTarget2D renderTarget;
        protected GraphicsDevice GraphicsDevice;
        protected SpriteBatch SpriteBatch;
        protected KeyboardState lastState = new KeyboardState();
        Texture2D img;

        public TestScreen()
        {
            GraphicsDevice = ScreenManager.currentInstance.GraphicsDevice;
            SpriteBatch = ScreenManager.currentInstance.SpriteBatch;
            renderTarget = new RenderTarget2D(GraphicsDevice, Game1.width, Game1.height);
        }

        public RenderTarget2D RenderTarget
        {
            get
            {
                return renderTarget;
            }
        }

        public virtual void Update(GameTime gameTime)
        {

        }

        public virtual void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.Red);
        }

        public virtual void Remove()
        {
            renderTarget.Dispose();
        }

        public virtual void LoadContent(ContentManager Content)
        {

        }

        public virtual void InjectInput(KeyboardState keyboardState, MouseState mouseState)
        {
            lastState = keyboardState;
        }
    }
}
