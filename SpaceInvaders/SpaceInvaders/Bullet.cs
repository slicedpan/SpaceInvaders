using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using ONet;

namespace SpaceInvaders
{
    public class Bullet : PhysicalEntity, IRemovable
    {
        Texture2D sprite;
        bool active = true;
        public Color color = Color.Red;
        public int ownerID;
        public bool isDown = false;
        public List<IEntity> creationList;
        public override float MaxSpeed
        {
            get
            {
                return 50.0f;
            }
        }        
        public Bullet()
        {
            Friction = 0.0f;
            collisionRadius = 4.0f;
        }
        public override int typeID
        {
            get
            {
                return 2;
            }
        }
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (!active)
                return;
            Rectangle rect;
            if (isDown)
            {
                rect = new Rectangle((int)_position.X - sprite.Width / 2, (int)_position.Y - sprite.Height + 11, sprite.Width, sprite.Height);
            }
            else
            {
                rect = new Rectangle((int)_position.X - sprite.Width / 2, (int)_position.Y + sprite.Height - 3, sprite.Width, sprite.Height);
            }
            Game1.SpriteBatch.Draw(sprite, rect, color);   
        }
        public override void LoadContent(ContentManager Content)
        {
            sprite = Content.Load<Texture2D>("bullet");
        }
        public override void Update(GameTime gameTime)
        {
            if (!Game1.screenExtent.Intersects(BoundingSphere))
                active = false;
            else
            {
                base.Update(gameTime);
            }
        }
        public override void Collide(PhysicalEntity other)
        {
            if (other.ID == ownerID || other is Bullet)
                return;
            IDamageable damageableOther = other as IDamageable;
            if (damageableOther != null)
            {
                damageableOther.TakeDamage(10);
            }
            active = false;
        }

        public override void HandleSpawnMessage(GameMessage message)
        {
            HandleMessage(message, true);
            ownerID = BitConverter.ToInt32(message.Message, 20);
            color.R = message.Message[24];
            color.G = message.Message[25];
            color.B = message.Message[26];
            isDown = BitConverter.ToBoolean(message.Message, 27);
        }

        public override GameMessage GetSpawnMessage()
        {
            GameMessage msg = new GameMessage();
            msg.DataType = GameState.DataTypeSpawnEntity;
            msg.index = ID;
            byte[] array = new byte[28];
            BitConverter.GetBytes(Position.X).CopyTo(array, 0);
            BitConverter.GetBytes(Position.Y).CopyTo(array, 4);
            BitConverter.GetBytes(Velocity.X).CopyTo(array, 8);
            BitConverter.GetBytes(Velocity.Y).CopyTo(array, 12);
            BitConverter.GetBytes(typeID).CopyTo(array, 16);
            BitConverter.GetBytes(ownerID).CopyTo(array, 20);
            array[24] = color.R;
            array[25] = color.G;
            array[26] = color.B;
            BitConverter.GetBytes(isDown).CopyTo(array, 27);
            msg.SetMessage(array);
            return msg;
        }

        #region IRemovable Members

        public bool isReadyToRemove
        {
            get { return !active; }
        }

        #endregion
    }
}
