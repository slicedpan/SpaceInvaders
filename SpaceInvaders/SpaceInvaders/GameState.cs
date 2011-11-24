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

        public const ushort ScoreUpdate = 2;
        public const ushort HealthUpdate = 3;
        public const ushort SpawnShip = 4;
        public const ushort SpawnEntity = 5;
        public const ushort EntityUpdate = 6;

        protected Dictionary<int, IEntity> entities;
        protected List<PhysicalEntity> physicalEntities;

        ushort counter = 0;

        public void AddTestEntities()
        {

        }

        public virtual void AddEntity(int ID, IEntity entityToAdd)
        {
            entities.Add(ID, entityToAdd);
            if (entityToAdd is PhysicalEntity)
                physicalEntities.Add(entityToAdd as PhysicalEntity);
        }
        public virtual int AddEntity(IEntity entityToAdd)
        {
            entities.Add(counter, entityToAdd);
            ++counter;
            if (entityToAdd is PhysicalEntity)
                physicalEntities.Add(entityToAdd as PhysicalEntity);
            return counter - 1;   
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
        public void HandleEntityUpdates(GameMessage message)
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
                    HandleEntityUpdates(msg);
                }
            }
            else
            {
                entities[message.index].HandleMessage(message);
            }
        }
        public static GameMessage SpawnMessage(int entityType, int entityID, Vector2 position)
        {
            GameMessage msg = new GameMessage();
            msg.DataType = 2;
            msg.index = GameState.SpawnEntity;
            byte[] array = new byte[12];
            BitConverter.GetBytes(entityID).CopyTo(array, 0);
            BitConverter.GetBytes(position.X).CopyTo(array, 4);
            BitConverter.GetBytes(position.Y).CopyTo(array, 8);
            msg.SetMessage(array);
            return msg;
        }
    }
}
