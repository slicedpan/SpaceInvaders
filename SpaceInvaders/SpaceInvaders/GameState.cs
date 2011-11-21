using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ONet;

namespace SpaceInvaders
{
    public class GameState
    {
        protected Dictionary<int, IEntity> entities;
        protected List<PhysicalEntity> physicalEntities;
        public void AddEntity(IEntity entityToAdd)
        {
            entities.Add(entityToAdd.ID, entityToAdd);
            if (entityToAdd is PhysicalEntity)
                physicalEntities.Add(entityToAdd as PhysicalEntity);
        }
        public virtual void Update(GameTime gameTime)
        {
            foreach (IEntity entity in entities.Values)
            {
                entity.Update(gameTime);
            }
        }
        public GameState()
        {
            entities = new Dictionary<int, IEntity>();
            physicalEntities = new List<PhysicalEntity>();
        }
        public void HandleMessage(GameMessage message)
        {
            if (message.DataType == GameMessage.Bundle)
            {
                int offset = 0;
                for (int i = 0; i < message.index; ++i)
                {
                    GameMessage msg = new GameMessage();
                    msg.fromBytes(message.Message, 6 + offset);
                    offset += 6;
                    offset += msg.MessageSize;
                    HandleMessage(msg);
                }
            }
            else
            {
                entities[message.index].HandleMessage(message);
            }
        }
    }
}
