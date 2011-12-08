using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ONet;
using Microsoft.Xna.Framework.Content;

namespace SpaceInvaders
{
    public class GameState
    {

        public const int IndexScoreUpdate = 2;
        public const int IndexHealthUpdate = 3;        
        public const int IndexSpawnEntity = 4;
        public const int IndexInitialisePlayerShip = 5;
        public const int IndexPlayerDeath = 6;
        public const int IndexGameOver = 7;
        public const int IndexRespawnShip = 8;
        
        public const ushort DataTypeMetaInfo = 2;
        public const ushort DataTypeEntityUpdate = 1;
        public const ushort DataTypeSpawnEntity = 3;
        public const ushort DataTypeDespawnEntity = 5;
        public const ushort DataTypeEntityQuery = 4;
        public const ushort DataTypeRequest = 6;
        public const ushort DataTypeReassignID = 7;

        protected Dictionary<int, IEntity> entities;
        protected List<PhysicalEntity> physicalEntities;

        protected ContentManager _contentManager;

        protected int idCounter = 0;

        public void AddTestEntities()
        {

        }

        public virtual void AddEntity(int ID, IEntity entityToAdd)
        {            
            entities.Add(ID, entityToAdd);
            if (idCounter <= ID)
                idCounter = ID + 1;
            entityToAdd.ID = ID;
            if (entityToAdd is PhysicalEntity)
                physicalEntities.Add(entityToAdd as PhysicalEntity);
        }
        public virtual int AddEntity(IEntity entityToAdd)
        {
            int id = GetNextID();   
            AddEntity(id, entityToAdd);
            return id;   
        }
        public int GetNextID()
        {
            while (entities.Keys.Contains<int>(idCounter))
                ++idCounter;
            idCounter++;
            return idCounter - 1;
        }

        public int GetIndex(IEntity entity)
        {
            int indexOfEntity = -1;
            foreach (KeyValuePair<int, IEntity> kvp in entities)
            {
                if (kvp.Value == entity)
                {
                    indexOfEntity = kvp.Key;
                    break;
                }
            }
            return indexOfEntity;
        }

        public virtual void RemoveEntity(IEntity entityToRemove)
        {
            int indexOfEntity = GetIndex(entityToRemove);
            if (indexOfEntity < 0)
                return;
            entities.Remove(indexOfEntity);
            if (entityToRemove is PhysicalEntity)
                physicalEntities.Remove(entityToRemove as PhysicalEntity);
        }
        public virtual void Update(GameTime gameTime)
        {
            for (int i = 0; i < physicalEntities.Count; ++i)
            {
                for (int j = i + 1; j < physicalEntities.Count; ++j)
                {
                    if (physicalEntities[i].BoundingSphere.Intersects(physicalEntities[j].BoundingSphere))
                    {
                        physicalEntities[i].Collide(physicalEntities[j]);
                        physicalEntities[j].Collide(physicalEntities[i]);
                    }
                }
            }
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

        public virtual void Draw(GameTime gameTime)
        {
            foreach (IEntity entity in entities.Values)
            {
                entity.Draw(gameTime);
            }
            foreach (PhysicalEntity pe in physicalEntities)
            {
                pe.DrawDebug();
            }
        }

        public void HandleEntityUpdates(GameMessage message, bool strict)
        {
            if (message.DataType == GameState.DataTypeSpawnEntity)
            {
                Spawn(message);
            }
            else if (message.DataType == GameState.DataTypeDespawnEntity)
            {                
                Despawn(message.index);
            }
            else
            {
                if (!entities.Keys.Contains<int>(message.index))
                {
                    //AddEntity(message.index, new DummyEntity());
                }
                else
                {
                    entities[message.index].HandleMessage(message, strict);
                }
            }
        }

        public static GameMessage DespawnMessage(int index)
        {
            GameMessage msg = new GameMessage();
            msg.DataType = DataTypeDespawnEntity;
            msg.index = index;
            msg.MessageSize = 0;
            return msg;
        }
        private void Despawn(int index)
        {
            RemoveEntity(entities[index]);
        }
        public void Spawn(GameMessage message)
        {
            int index = message.index;
            int p = BitConverter.ToInt32(message.Message, 16);
            if (entities.Keys.Contains<int>(index))
            {
                if (entities[index].typeID == -1)
                {
                    entities.Remove(index);
                }
                else
                {
                    return;
                }
            }

            if (idCounter <= index)
                idCounter = index + 1;

            switch (p)
            {
                case 0:
                    var ship = new PlayerShip();                    
                    ship.HandleSpawnMessage(message);
                    AddEntity(index, ship);
                    break;
                case 1:
                    var enemyShip = new EnemyShip();
                    enemyShip.HandleSpawnMessage(message);
                    AddEntity(index, enemyShip);
                    break;
                case 2:                   
                    var bullet = new Bullet();
                    bullet.HandleSpawnMessage(message);
                    AddEntity(index, bullet);
                    break;
                case 3:
                    var chunk = new BuildingChunk();
                    chunk.HandleSpawnMessage(message);
                    AddEntity(index, chunk);
                    break;
            }            
        }
        protected bool ReassignID(int oldIndex, int newIndex)
        {
            if (!entities.Keys.Contains<int>(oldIndex))
                return false;
            if (entities.Keys.Contains<int>(newIndex))
                return false;
            IEntity entity = entities[oldIndex];
            RemoveEntity(entity);
            AddEntity(newIndex, entity);
            return true;
        }

        public GameMessage ReassignIndexMessage(int oldIndex, int newIndex)
        {
            GameMessage msg = new GameMessage();
            msg.DataType = GameState.DataTypeReassignID;
            msg.index = oldIndex;
            byte[] array = new byte[4];
            BitConverter.GetBytes(newIndex).CopyTo(array, 0);
            msg.SetMessage(array);
            return msg;
        }
    }
}
