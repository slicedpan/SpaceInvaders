using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ONet;

namespace SpaceInvaders
{
    class DummyEntity : IEntity
    {
        int _id;
        Vector2 _position;
        Vector2 _lastPosition;
        bool _requiresUpdate = false;
        
        #region IEntity Members

        public void Update(GameTime gameTime)
        {
            if (_lastPosition != _position)
                _requiresUpdate = true;
            _lastPosition = _position;
        }

        public void LoadContent(ContentManager Content)
        {
            
        }

        public void Draw(GameTime gameTime)
        {
            
        }

        public BoundingSphere BoundingSphere
        {
            get { return new BoundingSphere(new Vector3(_position, 0.0f), 1.0f); }
        }

        public int ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public void HandleMessage(GameMessage message, bool strict)
        {
            _position = new Vector2(BitConverter.ToSingle(message.Message, 0), BitConverter.ToSingle(message.Message, 4));
        }

        public GameMessage GetStateMessage()
        {
            GameMessage msg = new GameMessage();
            byte[] array = new byte[20];
            msg.index = _id;
            msg.DataType = GameState.DataTypeEntityUpdate;
            BitConverter.GetBytes(_position.X).CopyTo(array, 0);
            BitConverter.GetBytes(_position.Y).CopyTo(array, 4);
            BitConverter.GetBytes((float)0.0f).CopyTo(array, 8);
            BitConverter.GetBytes((float)0.0f).CopyTo(array, 12);
            BitConverter.GetBytes((float)0.0f).CopyTo(array, 16);
            msg.SetMessage(array);
            return msg;
        }

        public void HandleSpawnMessage(GameMessage message)
        {
            HandleMessage(message, true);
        }

        public GameMessage GetSpawnMessage()
        {
            GameMessage msg = GetStateMessage();
            BitConverter.GetBytes(typeID).CopyTo(msg.Message, 16);
            return msg;
        }

        public int typeID
        {
            get { return -1; }
        }

        public Vector2 Position
        {
            get { return _position; }
        }

        public bool RequiresUpdate
        {
            get { return _requiresUpdate; }
            set { _requiresUpdate = value; }
        }

        #endregion
    }
}
