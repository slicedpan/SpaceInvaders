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
    public class PlayerShip : PhysicalEntity, IDamageable
    {
        Texture2D sprite;
        public Color color;
        public int health = 100;
        List<IEntity> _creationList;
        KeyboardState _lastState = new KeyboardState();
        ClientState _clientState;
        public ClientState ClientState
        {
            set
            {
                _clientState = value;
            }
        }
        public List<IEntity> CreationList
        {
            set
            {
                _creationList = value;
            }
        }
        bool _isDead = false;
        public bool isDead
        {
            get
            {
                return _isDead;
            }
        }
        public PlayerShip()
        {
            color = Color.White;
            mass = 10.0f;
            collisionRadius = 14.0f;
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
            Rectangle rect = new Rectangle((int)_position.X - sprite.Width / 2, (int)_position.Y + sprite.Height, sprite.Width, sprite.Height);
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
            if (ks.IsKeyDown(Keys.Space) && !_lastState.IsKeyDown(Keys.Space))
            {
                Fire();
            }
            _lastState = ks;
        }
        public override void HandleMessage(GameMessage message, bool strict)
        {
            Vector2 newPosition = new Vector2(BitConverter.ToSingle(message.Message, 0), BitConverter.ToSingle(message.Message, 4));
            Velocity = new Vector2(BitConverter.ToSingle(message.Message, 8), BitConverter.ToSingle(message.Message, 12));
            if (strict)
            {
                Position = newPosition;
                _lastPosition = newPosition;
            }
            else
                Velocity += (newPosition - Position) * 0.064f;                         
        }
        public override GameMessage GetStateMessage()
        {
            GameMessage msg = new GameMessage();
            msg.DataType = GameState.DataTypeEntityUpdate;
            msg.index = ID;
            byte[] array = new byte[16];           
            BitConverter.GetBytes(Position.X).CopyTo(array, 0);
            BitConverter.GetBytes(Position.Y).CopyTo(array, 4);
            BitConverter.GetBytes(Velocity.X).CopyTo(array, 8);
            BitConverter.GetBytes(Velocity.Y).CopyTo(array, 12);
            msg.SetMessage(array);
            return msg;
        }
        public override GameMessage GetSpawnMessage()
        {
            GameMessage msg = new GameMessage();
            msg.DataType = GameState.DataTypeSpawnEntity;
            msg.index = ID;
            byte[] array = new byte[23];
            BitConverter.GetBytes(typeID).CopyTo(array, 16);
            BitConverter.GetBytes(Position.X).CopyTo(array, 0);
            BitConverter.GetBytes(Position.Y).CopyTo(array, 4);
            BitConverter.GetBytes(Velocity.X).CopyTo(array, 8);
            BitConverter.GetBytes(Velocity.Y).CopyTo(array, 12);
            BitConverter.GetBytes(typeID).CopyTo(array, 16);
            array[20] = color.R;
            array[21] = color.G;
            array[22] = color.B;
            msg.SetMessage(array);
            return msg;
        }
        public override void HandleSpawnMessage(GameMessage message)
        {
            HandleMessage(message, true);
            color.R = message.Message[20];
            color.G = message.Message[21];
            color.B = message.Message[22];
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public void TakeDamage(int amount)
        {
            health -= amount;
            if (_clientState != null)
            {

            }
            if (health <= 0)
                Die();
        }
        void Die()
        {
            _isDead = true;
        }
        void Fire()
        {
            if (_creationList != null)
            {
                Bullet bullet = new Bullet();
                bullet.ownerID = ID;
                bullet.color = this.color;
                bullet.Place(this.Position + new Vector2(0.0f, -14.0f));
                bullet.Velocity = new Vector2(0.0f, -20.0f);
                _creationList.Add(bullet);

            }
        }
    }
}
