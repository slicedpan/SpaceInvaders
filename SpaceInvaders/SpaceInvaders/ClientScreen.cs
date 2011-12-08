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
    class ClientScreen : IGameScreen
    {
        #region IGameScreen Members

        Texture2D bgImg;
        QuadRenderer qR;
        Effect texDraw;
        ClientState clientState;
        MessageBox clientBox;
        MessageBox serverBox;
        KeyboardState lastState = new KeyboardState();
        Texture2D healthbar;
        Rectangle healthrect = new Rectangle(0, 0, 200, 800);
        Rectangle current = new Rectangle(2, 2, 20, 80);

        public ClientScreen()
        {
            qR = new QuadRenderer();
            clientBox = new MessageBox(10, 0, 0);
            serverBox = new MessageBox(10, 624, 0);
            serverBox.IsVisible = false;
            clientBox.IsVisible = false; 
            clientState = ClientState.currentInstance;            
        }

        public void Update(GameTime gameTime)
        {
            clientState.Update(gameTime);
            if (ServerState.currentInstance.GameServer.isActive)
                ServerState.currentInstance.Update(gameTime);
            String clientTextMessage;
            while (clientState.InfoStack.Pop(out clientTextMessage))
            {
                clientBox.AddMessage(clientTextMessage);
            }
            while (clientState.ErrorStack.Pop(out clientTextMessage))
            {
                clientBox.AddMessage(clientTextMessage);
            }
            while (ServerState.currentInstance.InfoStack.Pop(out clientTextMessage))
            {
                serverBox.AddMessage(clientTextMessage);
            }
            while (ServerState.currentInstance.ErrorStack.Pop(out clientTextMessage))
            {
                serverBox.AddMessage(clientTextMessage);
            }
        }

        public void InjectInput(KeyboardState keyboardState, MouseState mouseState)
        {
            if (keyboardState.IsKeyDown(Keys.Q) && !lastState.IsKeyDown(Keys.Q))
            {
                clientBox.IsVisible = !clientBox.IsVisible;
                serverBox.IsVisible = !serverBox.IsVisible;
            }
            if (keyboardState.IsKeyDown(Keys.P) && !lastState.IsKeyDown(Keys.P))
            {
                clientState.RequestInitialisation();
            }

            clientState.InjectInput(keyboardState, mouseState);
            lastState = keyboardState;
        }

        public void Draw(GameTime gameTime, GraphicsDevice graphicsDevice)
        {
            graphicsDevice.Clear(Color.White);
            texDraw.Parameters["tex"].SetValue(bgImg);
            texDraw.Techniques[0].Passes[0].Apply();
            qR.RenderQuad(graphicsDevice, -Vector2.One, Vector2.One, new Vector2(1.0f / Game1.width, 1.0f / Game1.height));
            Game1.SpriteBatch.Begin();

            clientState.Draw(gameTime);

            clientBox.Draw();
            serverBox.Draw();

            if (clientState.ship != null)
            {
                current.Height = ((clientState.ship.health * 8) / 10);
                current.Y = 82 - current.Height;
                healthrect.Height = (clientState.ship.health * 8);
            }
            Game1.SpriteBatch.Draw(healthbar, current, healthrect, Color.White, 0, Vector2.Zero, SpriteEffects.FlipVertically, 0);

            if (clientState.overlay != null)
            {
                clientState.overlay.Draw();
            }

            Game1.SpriteBatch.End();
        }

        public void Remove()
        {
            
        }

        public void LoadContent(ContentManager contentManager)
        {
            bgImg = contentManager.Load<Texture2D>("background");
            texDraw = contentManager.Load<Effect>("shaders/texdraw");
            clientState.LoadContent(contentManager);
            healthbar = contentManager.Load<Texture2D>("healthbar");
        }

        #endregion
    }
}
