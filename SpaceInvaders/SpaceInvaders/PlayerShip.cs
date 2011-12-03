using System;
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
        public Color color;
        public PlayerShip()
        {
            mass = 10.0f;
        }
        public override int typeID
        {
            get
            {
                return 0;
            }
        }
        public override float MaxSpeed
        {
            get
            {
                return 10.0f;
            }
        }
        public override void LoadContent(ContentManager Content)
        {
            sprite = Content.Load<Texture2D>("playersprite");
        }
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            Rectangle rect = new Rectangle((int)_position.X - sprite.Width / 2, (int)_position.Y + sprite.Height / 2, sprite.Width, sprite.Height);
            Game1.SpriteBatch.Draw(sprite, rect, color);            
        }
        public void InjectInput(KeyboardState ks, MouseState ms)
        {
            if (ks.IsKeyDown(Keys.D))
            {
                Velocity.X += 2.0f;
                RequiresUpdate = true;
            }
            else if (ks.IsKeyDown(Keys.A))
            {
                Velocity.X -= 2.0f;
                RequiresUpdate = true;
            }
            if (ks.IsKeyDown(Keys.W))
            {
                Velocity.Y -= 2.0f;
                RequiresUpdate = true;
            }
            else if (ks.IsKeyDown(Keys.S))
            {
                Velocity.Y += 2.0f;
                RequiresUpdate = true;
            }
        }
        public override void HandleMessage(GameMessage message, bool strict)
        {
            Vector2 newPosition = new Vector2(BitConverter.ToSingle(message.Message, 0), BitConverter.ToSingle(message.Message, 4));
            Velocity = new Vector2(BitConverter.ToSingle(message.Message, 8), BitConverter.ToSingle(message.Message, 12));
            if (strict)
                Position = newPosition;
            else
                Velocity += (newPosition - Position) * 0.064f;
            Angle = BitConverter.ToSingle(message.Message, 16);             
        }
        public override GameMessage GetStateMessage()
        {
            GameMessage msg = new GameMessage();
            msg.DataType = GameState.DataTypeEntityUpdate;
            msg.index = (ushort)ID;
            byte[] array = new byte[20];           
            BitConverter.GetBytes(Position.X).CopyTo(array, 0);
            BitConverter.GetBytes(Position.Y).CopyTo(array, 4);
            BitConverter.GetBytes(Velocity.X).CopyTo(array, 8);
            BitConverter.GetBytes(Velocity.Y).CopyTo(array, 12);
            BitConverter.GetBytes(Angle).CopyTo(array, 16);
            msg.SetMessage(array);
            return msg;
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}
