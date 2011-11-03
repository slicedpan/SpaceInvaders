using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SpaceInvaders
{
    class MessageBox
    {
        List<String> messages;
        int _maxLines;
        int _X, _Y;
        public MessageBox(int lines, int X, int Y)
        {
            messages = new List<string>();
            _maxLines = lines;
            _X = X;
            _Y = Y;
        }
        public void SpriteDraw()
        {
            Game1.SpriteBatch.Begin();
            Draw();
            Game1.SpriteBatch.End();
        }
        public void Draw()
        {
            for (int i = 0; i < messages.Count; ++i)
            {
                Game1.SpriteBatch.DrawString(Game1.Font, messages[i], new Vector2(_X, _Y + (i * 12)), Color.White);
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
