using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ONet;

namespace SpaceInvaders
{
    public class PhysicalEntity : IEntity
    {
        public Vector2 Position = Vector2.Zero;
        public float Angle = 0.0f;
        public Vector2 Velocity = Vector2.Zero;
        public float mass = 1.0f;
        int id = 0;

        public virtual int typeID
        {
            get
            {
                throw new Exception("no type id");
            }
        }

        public void Update(GameTime gameTime)
        {
            Position += Velocity;
        }

        public virtual void LoadContent(ContentManager Content)
        {
            
        }

        public virtual void Draw(GameTime gameTime)
        {

        }

        public virtual BoundingSphere BoundingSphere
        {
            get 
            {                  
                return new BoundingSphere(Vector3.Zero, 1.0f);
            }
        }
        public virtual void HandleMessage(GameMessage message)
        {
            
        }
        public virtual int ID
        {
            get
            {
                return id;
            }
        }
        public virtual GameMessage GetStateMessage()
        {
            throw new NotImplementedException();
        }
    }
}
