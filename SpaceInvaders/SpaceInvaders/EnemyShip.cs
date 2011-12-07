using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ONet;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace SpaceInvaders
{
    class EnemyShip : PhysicalEntity, IAIControlled, IDamageable
    {
        Texture2D sprite;
        Vector2 AITarget;
        Random rand;
        double nextFire = -1.0d;
        double fireRate = 0.5d;
        List<IEntity> _createList;
        Color color = Color.Red;

        public override float MaxSpeed
        {
            get
            {
                return 6.0f;
            }
        }
        public override float MaxAccel
        {
            get
            {
                return 0.4f;
            }
        }

        public EnemyShip()
        {
            rand = new Random();
            AITarget = _position;
            mass = 20.0f;
        }

        public override int typeID
        {
            get
            {
                return 1;
            }
        }
        public override void LoadContent(ContentManager Content)
        {
            sprite = Content.Load<Texture2D>("enemyship");
        }
        public override void Draw(GameTime gameTime)
        {
            Rectangle rect = new Rectangle((int)_position.X - sprite.Width / 2, (int)_position.Y + 2 * sprite.Height, sprite.Width, sprite.Height);
            Game1.SpriteBatch.Draw(sprite, rect, color);
        }   

        public void Think(GameTime gameTime)
        {
            if ((_position - AITarget).Length() < 4.0f)
            {
                ChooseNewTarget();
            }
            else
            {
                Vector2 velocityDir = (AITarget - _position);
                velocityDir.Normalize();
                Velocity += velocityDir * MaxAccel;
            }

            if (nextFire < 0.0d)
                nextFire = (1.0d + (rand.NextDouble() / 2.0d)) * (1000.0d / fireRate) + gameTime.TotalGameTime.TotalMilliseconds;
            else if (nextFire < gameTime.TotalGameTime.TotalMilliseconds)
            {
                Fire();
                nextFire = (1.0d + (rand.NextDouble() / 2.0d)) * (1000.0d / fireRate) + gameTime.TotalGameTime.TotalMilliseconds;
            }            
        }

        public override void HandleSpawnMessage(GameMessage message)
        {
            HandleMessage(message, true);
            color.R = message.Message[20];
            color.G = message.Message[21];
            color.B = message.Message[22];
        }

        public override GameMessage GetSpawnMessage()
        {
            GameMessage msg = new GameMessage();
            msg.DataType = GameState.DataTypeSpawnEntity;
            msg.index = ID;
            byte[] array = new byte[23];
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

        void Fire()
        {
            if (_createList != null)
            {
                var bullet = new Bullet();
                bullet.ownerID = ID;
                bullet.color = this.color;
                bullet.Place(_position + new Vector2(0.0f, 1.0f));
                bullet.Velocity = new Vector2(0.0f, 20.0f);
                _createList.Add(bullet);
            }
        }

        void ChooseNewTarget()
        {
            AITarget = new Vector2((float)rand.Next(Game1.width), (float)rand.Next(Game1.height));
        }

        public Vector2 Target
        {
            get { return AITarget; }
        }

        public List<IEntity> creationList
        {
            set { _createList = value; }
        }

        public void TakeDamage(int amount)
        {
            
        }
    }
}
