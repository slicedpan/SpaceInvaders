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
        IEntity owner;
        public IEntity Owner
        {
            get
            {
                return owner;
            }
        }
        public override float MaxSpeed
        {
            get
            {
                return 50.0f;
            }
        }
        public Bullet(IEntity owner)
        {
            Friction = 0.0f;
            this.owner = owner;
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
            Game1.SpriteBatch.Draw(sprite, Position, Color.White);
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
            if (other == owner)
                return;
            active = false;
        }
        #region IRemovable Members

        public GameMessage GetSpawnMessage(int ownerID)
        {
            GameMessage msg = new GameMessage();
            msg.DataType = GameState.DataTypeSpawnEntity;
            msg.index = ID;
            byte[] array = new byte[24];
            BitConverter.GetBytes(typeID).CopyTo(array, 0);
            BitConverter.GetBytes(_position.X).CopyTo(array, 4);
            BitConverter.GetBytes(_position.Y).CopyTo(array, 8);
            BitConverter.GetBytes(ownerID).CopyTo(array, 12);
            BitConverter.GetBytes(Velocity.X).CopyTo(array, 16);
            BitConverter.GetBytes(Velocity.Y).CopyTo(array, 20);               
            msg.SetMessage(array);
            return msg;
        }

        public bool isReadyToRemove
        {
            get { return !active; }
        }

        #endregion
    }
}
