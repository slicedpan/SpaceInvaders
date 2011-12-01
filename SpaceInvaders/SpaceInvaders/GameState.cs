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
        
        public const ushort DataTypeMetaInfo = 2;
        public const ushort DataTypeEntityUpdate = 1;
        public const ushort DataTypeSpawnEntity = 3;
        public const ushort DataTypeDespawnEntity = 5;
        public const ushort DataTypeEntityQuery = 4;
        public const ushort DataTypeRequest = 6;

        protected Dictionary<int, IEntity> entities;
        protected List<PhysicalEntity> physicalEntities;

        protected ContentManager _contentManager;



        ushort counter = 0;

        public void AddTestEntities()
        {

        }

        public virtual void AddEntity(int ID, IEntity entityToAdd)
        {            
            entities.Add(ID, entityToAdd);
            entityToAdd.ID = ID;
            if (entityToAdd is PhysicalEntity)
                physicalEntities.Add(entityToAdd as PhysicalEntity);
        }
        public virtual int AddEntity(IEntity entityToAdd)
        {
            AddEntity(counter, entityToAdd);
            ++counter;
            return counter - 1;   
        }
        public virtual void RemoveEntity(IEntity entityToRemove)
        {
            int indexOfEntity = -1;
            foreach (KeyValuePair<int, IEntity> kvp in entities)
            {
                if (kvp.Value == entityToRemove)
                {
                    indexOfEntity = kvp.Key;
                    break;
                }
            }
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

        public void Draw(GameTime gameTime)
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
                Spawn(message.index, BitConverter.ToInt32(message.Message, 0), new Vector2(BitConverter.ToSingle(message.Message, 4), BitConverter.ToSingle(message.Message, 8)));
            }
            else if (message.DataType == GameState.DataTypeDespawnEntity)
            {                
                Despawn(message.index);
            }
            else
            {
                if (!entities.Keys.Contains<int>(message.index))
                    AddEntity(message.index, new DummyEntity());
                entities[message.index].HandleMessage(message, strict);
            }
        }
        public static GameMessage SpawnMessage(int entityType, int entityID, Vector2 position)
        {
            GameMessage msg = new GameMessage();
            msg.DataType = DataTypeSpawnEntity;
            msg.index = entityID;
            byte[] array = new byte[12];
            BitConverter.GetBytes(entityType).CopyTo(array, 0);
            BitConverter.GetBytes(position.X).CopyTo(array, 4);
            BitConverter.GetBytes(position.Y).CopyTo(array, 8);
            msg.SetMessage(array);
            return msg;
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
        private void Spawn(int index, int p, Vector2 position)
        {
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

            switch (p)
            {
                case 0:
                    var ship = new PlayerShip();
                    ship.Position = position;
                    AddEntity(index, ship);
                    break;
                case 1:
                    var enemyShip = new EnemyShip();
                    enemyShip.Position = position;
                    AddEntity(index, enemyShip);
                    break;
                case 2:
                    var bullet = new Bullet(new DummyEntity());
                    bullet.Place(position);
                    AddEntity(index, bullet);
                    break;
            }            
        }
    }
}
