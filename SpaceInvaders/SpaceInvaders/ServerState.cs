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
        public static ServerState currentInstance;
        public MessageBox msgBox;

        public ServerState()
        {
            if (currentInstance == null)
            {
                currentInstance = this;
            }
            else
            {
                throw new Exception("only one instance of ServerState is allowed");
            }
            messages = new List<GameMessage>();
            flatList = new List<IEntity>();
            playerInfo= new Dictionary<int, PlayerInfo>();
            currentEntity = 0;
        }
        public override int AddEntity(IEntity entityToAdd)
        {
            flatList.Add(entityToAdd);
            return base.AddEntity(entityToAdd);
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
        public void Reset()
        {
            foreach (IEntity entity in entities.Values)
            {
                // dispose of entity
            }
            entities.Clear();
            flatList.Clear();
        }
        public void Message(int clientNumber, GameMessage message)
        {

        }
        public void ClientConnect(int clientNumber, GameMessage message)
        {
            if (msgBox != null)
                msgBox.AddMessage("Client connected, client number: " + clientNumber);
            int id = AddEntity(new PlayerShip());
            Game1.Server.Connections[clientNumber].Send(GameState.SpawnMessage(1, id, new Vector2(512, 700)));
        }
        public void ClientDisconnect(int clientNumber, GameMessage message)
        {
            if (msgBox != null)
                msgBox.AddMessage(String.Format("Client {0} disconnected: {1}", clientNumber, message.messageAsString()));
        }
    }
}
