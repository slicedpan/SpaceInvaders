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
    public class ScreenManager
    {
        public static ScreenManager currentInstance;

        public SpriteBatch SpriteBatch;
        public GraphicsDevice GraphicsDevice;
        public IGameScreen currentScreen;
        public IGameScreen lastScreen;
        ContentManager ContentManager;

        float lastAlpha = 0.0f;
        float currentAlpha = 0.0f;
        double delayCounter = -1.0d;
        double delayTime = double.MaxValue;
        bool transition = false;

        public ScreenManager(GraphicsDevice graphicsDevice, ContentManager cm)
        {
            if (currentInstance == null)
            {
                ContentManager = cm;
                GraphicsDevice = graphicsDevice;
                SpriteBatch = new SpriteBatch(graphicsDevice);
                currentInstance = this;
            }
            else
            {
                throw new Exception("only one instance of ScreenManager can be instantiated");
            }
        }
        public void SetScreen(IGameScreen screen)
        {
            currentScreen = screen;
        }
        public void Draw(GameTime gameTime)
        {
            currentScreen.Draw(gameTime);
            if (transition)
            {
                lastScreen.Draw(gameTime);
            }
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
            if (transition)
            {
                SpriteBatch.Draw(currentScreen.RenderTarget, new Vector2(0.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, currentAlpha));
                SpriteBatch.Draw(lastScreen.RenderTarget, new Vector2(0.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, lastAlpha));
            }
            else
            {
                SpriteBatch.Draw(currentScreen.RenderTarget, new Vector2(0.0f, 0.0f), Color.White);
            }
            SpriteBatch.End();
        }
        public void Update(GameTime gameTime)
        {
            if (delayCounter < 0.0d)
            {

            }
            else if (delayCounter < delayTime)
            {
                float diff = (float)((1.0d / delayTime) * gameTime.ElapsedGameTime.TotalMilliseconds);
                lastAlpha -= diff;
                currentAlpha += diff;
                delayCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
                transition = true;
            }
            else
            {
                delayCounter = -1.0d;
                lastScreen.Remove();
                lastScreen = null;
                transition = false;
            }
            currentScreen.Update(gameTime);
            currentScreen.InjectInput(Keyboard.GetState(), Mouse.GetState());
        }
        public void Switch(IGameScreen newScreen, double delay = 1000.0d)
        {
            newScreen.LoadContent(ContentManager);
            lastScreen = currentScreen;
            currentScreen = newScreen;
            lastAlpha = 1.0f;
            currentAlpha = 0.0f;
            delayTime = delay;
            delayCounter = 0.0d;
            transition = true;
        }
    }
}
