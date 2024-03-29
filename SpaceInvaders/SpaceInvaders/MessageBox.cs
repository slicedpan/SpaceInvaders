﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace SpaceInvaders
{
    public class MessageBox
    {
        List<String> messages;
        int _maxLines;
        int _X, _Y;
        int _width, _height;
        Rectangle _rect;
        Window bgWindow;
        int messageCounter = 10;
        public Color color = Color.Red;
        public MessageBox(int lines, int X, int Y)
        {
            messages = new List<string>();            
            _maxLines = lines;
            _X = X;
            _Y = Y;
            _width = 400;
            _height = lines * 11 + 16;
            _rect = new Rectangle(_X, _Y, _width, _height);
            bgWindow = new Window();
            bgWindow.SetPosition(_X + (_width / 2), _Y + (_height / 2));
            bgWindow.SetSize(_width, _height);
            Init();
        }
        public MessageBox(Rectangle size)
        {
            _X = size.X;
            _Y = size.Y;
            _width = size.Width;
            _height = size.Height;
            _rect = size;
            _maxLines = (int)Math.Floor((_height - 16) / 11.0d);
            bgWindow = new Window();
            bgWindow.SetPosition(_X, _Y);
            bgWindow.SetSize(_width, _height);
            Init();
        }
        void Init()
        {
            for (int i = 0; i < _maxLines; ++i)
            {
                messages.Add("");
            }
        }
        public void SpriteDraw()
        {
            Game1.SpriteBatch.Begin();
            Draw();
            Game1.SpriteBatch.End();
        }

        public void Draw()
        {
            if (IsVisible)
            {
                bgWindow.Draw(Game1.SpriteBatch, color);
                for (int i = 0; i < messages.Count; ++i)
                {
                    if (messages[i] != "")
                        Game1.SpriteBatch.DrawString(Game1.mbFont, String.Format("{0} - {1}", messageCounter - 18 + i, messages[i]), new Vector2(_X + 8, _Y + (i * 11) + 3), Color.White * (0.4f + (i * 0.06f)));                    
                }
            }
        }
        public void AddMessage(string message)
        {            
            messages.Add(message);
            ++messageCounter;
            if (messages.Count > _maxLines)
            {
                messages.Remove(messages[0]);
            }   
        }
        public bool IsVisible = true;    
    }
}
