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
        RenderTarget2D oldRenderTarget;
        RenderTarget2D newRenderTarget;

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
                newRenderTarget = new RenderTarget2D(GraphicsDevice, Game1.width, Game1.height, false, SurfaceFormat.Color, DepthFormat.Depth24);
                oldRenderTarget = new RenderTarget2D(GraphicsDevice, Game1.width, Game1.height, false, SurfaceFormat.Color, DepthFormat.Depth24);
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
            currentScreen.LoadContent(ContentManager);
        }
        public void Draw(GameTime gameTime)
        {
            if (transition)
            {
                GraphicsDevice.SetRenderTarget(newRenderTarget);
                currentScreen.Draw(gameTime, GraphicsDevice);
                GraphicsDevice.SetRenderTarget(oldRenderTarget);
                lastScreen.Draw(gameTime, GraphicsDevice);
                GraphicsDevice.SetRenderTarget(null);
                SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
                SpriteBatch.Draw(newRenderTarget, new Vector2(0.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, currentAlpha));
                SpriteBatch.Draw(oldRenderTarget, new Vector2(0.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, lastAlpha));
                SpriteBatch.End();
            }
            else
            {
                GraphicsDevice.SetRenderTarget(null);
                currentScreen.Draw(gameTime, GraphicsDevice);
            }            
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
