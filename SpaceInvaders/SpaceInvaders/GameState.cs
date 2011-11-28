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

        public const ushort IndexScoreUpdate = 2;
        public const ushort IndexHealthUpdate = 3;        
        public const ushort IndexSpawnEntity = 4;
        public const ushort IndexInitialisePlayerShip = 5;
        
        public const ushort DataTypeMetaInfo = 2;
        public const ushort DataTypeEntityUpdate = 1;
        public const ushort DataTypeSpawnEntity = 3;

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

        public void Draw(GameTime gameTime)
        {
            foreach (IEntity entity in entities.Values)
            {
                entity.Draw(gameTime);
            }
        }

        public void HandleEntityUpdates(GameMessage message)
        {
            if (message.DataType == GameState.DataTypeSpawnEntity)
            {
                Spawn(message.index, BitConverter.ToInt32(message.Message, 0), new Vector2(BitConverter.ToSingle(message.Message, 4), BitConverter.ToSingle(message.Message, 8)));
            }
            else
            {
                entities[message.index].HandleMessage(message);
            }
        }
        public static GameMessage SpawnMessage(int entityType, int entityID, Vector2 position)
        {
            GameMessage msg = new GameMessage();
            msg.DataType = DataTypeSpawnEntity;
            msg.index = (ushort)entityID;
            byte[] array = new byte[12];
            BitConverter.GetBytes(entityType).CopyTo(array, 0);
            BitConverter.GetBytes(position.X).CopyTo(array, 4);
            BitConverter.GetBytes(position.Y).CopyTo(array, 8);
            msg.SetMessage(array);
            return msg;
        }
        private void Spawn(int index, int p, Vector2 position)
        {
            switch (p)
            {
                case 0:
                    var ship = new PlayerShip();
                    ship.Position = position;
                    AddEntity(index, ship);
                    break;
                case 1:
                    break;
            }
        }

    }
}
