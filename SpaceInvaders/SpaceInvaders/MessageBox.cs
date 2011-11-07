using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace SpaceInvaders
{
    class MessageBox
    {
        List<String> messages;
        int _maxLines;
        int _X, _Y;
        int _width, _height;
        Rectangle _rect;
        Window bgWindow;
        public MessageBox(int lines, int X, int Y)
        {
            messages = new List<string>();
            _maxLines = lines;
            _X = X;
            _Y = Y;
            _width = 400;
            _height = lines * 11 + 16;
            _rect = new Rectangle(_X, _Y, _width, _height);
            bgWindow = new Window(new Color(255, 35, 0));
            bgWindow.SetPosition(_X + (_width / 2), _Y + (_height / 2));
            bgWindow.SetSize(_width, _height);
        }
        public MessageBox(Rectangle size)
        {
            _X = size.X;
            _Y = size.Y;
            _width = size.Width;
            _height = size.Height;
            _rect = size;
            _maxLines = (int)Math.Floor((_height - 16) / 11.0d);
            bgWindow = new Window(new Color(255, 35, 0));
            bgWindow.SetPosition(_X, _Y);
            bgWindow.SetSize(_width, _height);
        }
        public void SpriteDraw()
        {
            Game1.SpriteBatch.Begin();
            Draw();
            Game1.SpriteBatch.End();
        }

        public void Draw()
        {
            bgWindow.Draw(Game1.SpriteBatch);
            for (int i = 0; i < messages.Count; ++i)
            {                
                Game1.SpriteBatch.DrawString(Game1.mbFont, messages[i], new Vector2(_X + 8, _Y + (i * 11) + 3), Color.White);                
            }
        }
        public void AddMessage(string message)
        {            
            messages.Add(message);
            if (messages.Count > _maxLines)
            {
                messages.Remove(messages[0]);
            }   
        }            
    }
}
