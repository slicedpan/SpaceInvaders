using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceInvaders
{
    public class MenuScreen : IGameScreen
    {
        KeyboardState lastState;
        MouseState lastMouseState;
        String ip = "";
        Texture2D serverButton;
        Texture2D connectButton;
        Texture2D cursorImg;
        Rectangle serverRect;
        Rectangle connectRect;
        int mouseX, mouseY;

        public MenuScreen()
        {
            lastState = new KeyboardState();
            lastMouseState = new MouseState();
            Game1.KeyboardBuffer.Enabled = true;
            serverRect = new Rectangle(340, 500, 120, 50);
            connectRect = new Rectangle(340, 400, 120, 50);
        }
        public void Update(GameTime gameTime)
        {
            Game1.KeyboardBuffer.TranslateMessage = true;
            ip += Game1.KeyboardBuffer.GetText();
        }

        public void InjectInput(KeyboardState keyboardState, MouseState mouseState)
        {
            mouseX = mouseState.X;
            mouseY = mouseState.Y;
            if (mouseState.LeftButton == ButtonState.Released && lastMouseState.LeftButton == ButtonState.Pressed)
            {
                
            }

            lastState = keyboardState;
            lastMouseState = mouseState;
        }

        public void Draw(GameTime gameTime, GraphicsDevice graphicsDevice)
        {
            graphicsDevice.Clear(Color.Black);
            Game1.SpriteBatch.Begin();
            Game1.SpriteBatch.Draw(serverButton, serverRect, Color.White);
            Game1.SpriteBatch.Draw(connectButton, connectRect, Color.White);
            Game1.SpriteBatch.Draw(cursorImg, new Rectangle(mouseX, mouseY, 18, 16), Color.White); 
            Game1.SpriteBatch.DrawString(Game1.Font, ip, Vector2.Zero, Color.White);
            Game1.SpriteBatch.End();
        }

        public void Remove()
        {
            
        }

        public void LoadContent(ContentManager contentManager)
        {
            serverButton = contentManager.Load<Texture2D>("server");
            connectButton = contentManager.Load<Texture2D>("connect");
            cursorImg = contentManager.Load<Texture2D>("cursor");
        }
    }
}
