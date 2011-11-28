﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ONet;

namespace SpaceInvaders
{
    public class PlayerShip : PhysicalEntity
    {
        Texture2D sprite;
        public override int typeID
        {
            get
            {
                return 0;
            }
        }
        public override void LoadContent(ContentManager Content)
        {
            sprite = Content.Load<Texture2D>("playersprite");
        }
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            Game1.SpriteBatch.Draw(sprite, Position, Color.White);
        }
        public void InjectInput(KeyboardState ks, MouseState ms)
        {
            if (ks.IsKeyDown(Keys.D))
                _position.X += 2.0f;
            else if (ks.IsKeyDown(Keys.A))
                _position.X -= 2.0f;
            if (ks.IsKeyDown(Keys.W))
                _position.Y -= 2.0f;
            else if (ks.IsKeyDown(Keys.S))
                _position.Y += 2.0f;
        }
        public override void HandleMessage(GameMessage message)
        {
            
        }
        public override GameMessage GetStateMessage()
        {
            GameMessage msg = new GameMessage();
            msg.DataType = GameState.DataTypeEntityUpdate;
            msg.index = 0;
            byte[] array = new byte[26];
            BitConverter.GetBytes(GameState.DataTypeEntityUpdate);            
            BitConverter.GetBytes(Position.X).CopyTo(array, 0);
            BitConverter.GetBytes(Position.Y).CopyTo(array, 4);
            BitConverter.GetBytes(Velocity.X).CopyTo(array, 8);
            BitConverter.GetBytes(Velocity.Y).CopyTo(array, 12);
            BitConverter.GetBytes(Angle).CopyTo(array, 16);
            msg.fromBytes(array);
            return msg;
        }
    }
}
