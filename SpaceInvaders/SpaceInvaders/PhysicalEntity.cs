using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace SpaceInvaders
{
    class PhysicalEntity : IEntity
    {
        public Vector2 Position;
        public float Angle;
        public Vector2 Velocity;
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
            get { throw new NotImplementedException(); }
        }
    }
}
