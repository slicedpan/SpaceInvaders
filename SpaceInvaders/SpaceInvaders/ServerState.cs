using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ONet;
using Microsoft.Xna.Framework;

namespace SpaceInvaders
{
    public class ServerState : GameState
    {
        int currentEntity;
        List<IEntity> flatList;
        List<GameMessage> messages;
        public GameMessage CurrentBundle;
        double counter = 0.0d;
        const double updatesPerSec = 20.0d;
        Dictionary<int, PlayerInfo> playerInfo;

        public ServerState()
        {
            messages = new List<GameMessage>();
            flatList = new List<IEntity>();
            playerInfo= new Dictionary<int, PlayerInfo>();
            currentEntity = 0;
        }
        public override void AddEntity(IEntity entityToAdd)
        {
            flatList.Add(entityToAdd);
            base.AddEntity(entityToAdd);
        }
        public override void Update(GameTime gameTime)
        {
            counter += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (counter > (1.0d / updatesPerSec))
            {
                PopulateMessageBundle();
                counter = 0.0d;
            }
            base.Update(gameTime);
        }
        public void PopulateMessageBundle()
        {
            byte[] array;
            int length = 0;
            int updateCount = 0;
            foreach (GameMessage message in messages)
            {
                length += 6;
                length += message.MessageSize;
            }
            while (length < 400 && updateCount < flatList.Count)
            {
                if (currentEntity < 0)
                    currentEntity = flatList.Count;
                GameMessage message = flatList[currentEntity].GetStateMessage();
                length += message.MessageSize + 6;
                messages.Add(message);
                ++updateCount;
                --currentEntity;
            }
            array = new byte[length];
            length = 0;
            foreach (GameMessage message in messages)
            {
                message.toBytes().CopyTo(array, length);
                length += 6;
                length += message.MessageSize;
            }
            GameMessage bundle = new GameMessage(array);
            bundle.index = (ushort)messages.Count;
            bundle.DataType = GameMessage.Bundle;
            bundle.MessageSize = (ushort)array.Length;
            CurrentBundle = bundle;
        }
    }
}
