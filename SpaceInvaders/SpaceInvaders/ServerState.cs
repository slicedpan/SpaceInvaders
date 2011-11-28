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

        GameServer _server;
        MessageStack<GameMessage> _messageStack;
        MessageStack<String> _errorStack;
        MessageStack<String> _infoStack;

        #region accessors

        public MessageStack<String> InfoStack
        {
            get
            {
                return _infoStack;
            }
        }

        public MessageStack<String> ErrorStack
        {
            get
            {
                return _errorStack;
            }
        }

        public MessageStack<GameMessage> GameMessageStack
        {
            get
            {
                return _messageStack;
            }
        } 

        public GameServer GameServer
        {
            get
            {
                return _server;
            }
        }

        #endregion

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
            _server = new GameServer();
            _server.OnClientConnect = new ONet.GameServer.Callback(ClientConnect);
            _server.OnClientDisconnect = new ONet.GameServer.Callback(ClientDisconnect);
            _server.OnClientMessage = new ONet.GameServer.Callback(Message);
            _messageStack = new MessageStack<GameMessage>();
            _infoStack = new MessageStack<string>();
            _errorStack = new MessageStack<string>();
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
                _server.Send(CurrentBundle);
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
            _messageStack.Push(message);
        }
        public void ClientDisconnect(int clientNumber, GameMessage message)
        {
            _infoStack.Push(String.Format("Client {0} disconnected: {1}", clientNumber, message.messageAsString()));
        }
        public void ClientConnect(int clientNumber, GameMessage message)
        {
            _infoStack.Push(String.Format("Client {0} connected from address {1}", clientNumber, _server.Connections[clientNumber].Socket.RemoteEndPoint));
            PlayerShip ship = new PlayerShip();
            int clientShipIndex = AddEntity(ship);
            _server.Send(GameState.SpawnMessage(0, clientShipIndex, ship.Position));

            GameMessage initMessage = new GameMessage();
            initMessage.DataType = GameState.DataTypeMetaInfo;
            initMessage.index = GameState.IndexInitialisePlayerShip;
            byte[] arr = new byte[4];
            BitConverter.GetBytes(clientShipIndex).CopyTo(arr, 0);
            initMessage.SetMessage(arr);
            _server.Connections[clientNumber].Send(initMessage);
        }        
    }
}
