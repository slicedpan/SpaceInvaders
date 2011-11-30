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
        protected Vector2 _position = Vector2.Zero;
        Vector2 _lastPosition = Vector2.Zero;
        bool requiresUpdate = true;
        public float Friction = 0.3f;
        protected float collisionRadius = 10.0f;
        public virtual float MaxSpeed
        {
            get
            {
                return 1.0f;
            }
        }
        public virtual float MaxAccel
        {
            get
            {
                return 0.1f;
            }
        }
        public bool RequiresUpdate
        {
            get
            {
                return requiresUpdate;
            }
            set
            {
                requiresUpdate = value;
            }
        }
        public Vector2 Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
            }
        }
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

        public virtual void Update(GameTime gameTime)
        { 
            _position += Velocity;
            if ((_lastPosition - Position).Length() > 1.2f * Velocity.Length())
            {
                Velocity += (_position - _lastPosition) * 0.016f;
                _position += (_lastPosition - _position) * 0.5f;
            }
            
            Velocity *= (1.0f - Friction);
            if (Velocity.Length() < 0.001f)
                Velocity = Vector2.Zero;
            else
                requiresUpdate = true;

            if (Velocity.Length() > MaxSpeed)
            {
                Velocity.Normalize();
                Velocity *= MaxSpeed;
            }

            _lastPosition = _position;
        }

        public void Place(Vector2 position)
        {
            _position = position;
            _lastPosition = position;
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
                return new BoundingSphere(new Vector3(_position, 0.0f), collisionRadius);
            }
        }
        public virtual void HandleMessage(GameMessage message, bool strict)
        {
            Vector2 newPosition = new Vector2(BitConverter.ToSingle(message.Message, 0), BitConverter.ToSingle(message.Message, 4));
            Velocity = new Vector2(BitConverter.ToSingle(message.Message, 8), BitConverter.ToSingle(message.Message, 12));
            if (strict)
                Position = newPosition;
            else
                Velocity += (newPosition - Position) * 0.064f;
            Angle = BitConverter.ToSingle(message.Message, 16);
        }
        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }
        public virtual void DrawDebug()
        {
            Game1.SpriteBatch.Draw(Game1.circleTex, new Rectangle((int)(_position.X - collisionRadius), (int)(_position.Y + collisionRadius), (int)(collisionRadius * 2), (int)(collisionRadius * 2)), Color.White);
        }
        public virtual GameMessage GetStateMessage()
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
        public virtual void Collide(PhysicalEntity other)
        {
            float invMass = 1.0f / mass;
            Velocity -= (other.Position - _position) * invMass;
        }
    }
}
