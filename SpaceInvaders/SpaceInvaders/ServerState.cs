﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ONet;
using Microsoft.Xna.Framework;

namespace SpaceInvaders
{
    public class ServerState : GameState
    {

        List<IEntity> flatList;
        Dictionary<int, List<GameMessage>> messages = new Dictionary<int,List<GameMessage>>();
        List<GameMessage> broadcastMessages = new List<GameMessage>();
        public GameMessage CurrentBundle;
        double updateCounter = 0.0d;
        double secondCounter = 0.0d;
        const double updatesPerSec = 20.0d;
        Dictionary<int, PlayerInfo> playerInfo;
        Dictionary<int, MessageStack<GameMessage>> _messageStacks = new Dictionary<int, MessageStack<GameMessage>>();
        public static ServerState currentInstance;

        GameServer _server;
        MessageStack<String> _errorStack;
        MessageStack<String> _infoStack;

        List<IEntity> createdEntities = new List<IEntity>();
        List<IAIControlled> AIControlledEntities = new List<IAIControlled>();
        List<IRemovable> removableEntities = new List<IRemovable>();

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

        public Dictionary<int, MessageStack<GameMessage>> GameMessageStack
        {
            get
            {
                return _messageStacks;
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
            flatList = new List<IEntity>();
            playerInfo= new Dictionary<int, PlayerInfo>();
            _server = new GameServer();
            _server.OnClientConnect = new ONet.GameServer.Callback(ClientConnect);
            _server.OnClientDisconnect = new ONet.GameServer.Callback(ClientDisconnect);
            _server.OnClientMessage = new ONet.GameServer.Callback(Message);
            _server.OnError = new ONet.GameServer.ErrorCallback(ErrorMessage);
            _infoStack = new MessageStack<string>(10);
            _errorStack = new MessageStack<string>(10);
            createdEntities.Add(new EnemyShip());
        }

        public override int AddEntity(IEntity entityToAdd)
        {
            flatList.Add(entityToAdd);
            if (entityToAdd is IAIControlled)
            {
                var AI = entityToAdd as IAIControlled;
                AI.creationList = createdEntities;
                AIControlledEntities.Add(AI);
                
            }
            if (entityToAdd is IRemovable)
            {
                removableEntities.Add(entityToAdd as IRemovable);
            }
            return base.AddEntity(entityToAdd);
        }

        public override void Update(GameTime gameTime)
        {
            updateCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
            secondCounter += gameTime.ElapsedGameTime.TotalMilliseconds;

            foreach (IAIControlled AI in AIControlledEntities)
            {
                AI.Think(gameTime);                
            }

            if (createdEntities.Count > 0)
            {
                foreach (IEntity entity in createdEntities)
                {
                    int newIndex = AddEntity(entity);
                    broadcastMessages.Add(GameState.SpawnMessage(entity.typeID, entity.ID, entity.Position));
                }
                createdEntities.Clear();
            }

            CullEntities();

            HandleMessages();

            if (updateCounter > (1000.0d / Game1.updatesPerSecond))
            {
                foreach (IEntity entity in entities.Values)
                {
                    if (entity.RequiresUpdate)
                    {
                        broadcastMessages.Add(entity.GetStateMessage());
                    }
                }
                foreach (KeyValuePair<int, List<GameMessage>> messageList in messages)
                {
                    messageList.Value.AddRange(broadcastMessages);
                    if (messageList.Value.Count > 0)
                    {
                        if (messageList.Value.Count == 1)
                        {
                            _server.Connections[messageList.Key].Send(messageList.Value[0]);
                        }
                        else
                        {
                            while (messageList.Value.Count > 50)
                            {
                                messageList.Value.Remove(messageList.Value[0]);
                            }
                            GameMessage initMessage = new GameMessage();
                            initMessage.DataType = GameState.DataTypeMetaInfo;
                            initMessage.index = GameState.IndexInitialisePlayerShip;
                            byte[] arr = new byte[4];
                            BitConverter.GetBytes(playerInfo[messageList.Key].EntityID).CopyTo(arr, 0);
                            initMessage.SetMessage(arr);


                            if (messageList.Value.Contains<GameMessage>(initMessage, new GameMessageComparer()))
                            {

                            }
                            Connection conn;
                            if(_server.Connections.TryGetValue(messageList.Key, out conn))
                                conn.Send(GameMessage.MessageBundle(messageList.Value));
                        }
                        messageList.Value.Clear();
                    }
                }
                broadcastMessages.Clear();
                updateCounter = 0.0d;
            }
            if (secondCounter > 1000.0d)
            {
                foreach (IEntity entity in entities.Values)
                {
                    _infoStack.Push(String.Format("Entity {0}, position {1}", entity.ID, entity.Position));
                }
                secondCounter = 0.0d;
            }
            
            base.Update(gameTime);
        }

        private void HandleMessages()
        {
            GameMessage message;
            foreach (KeyValuePair<int, MessageStack<GameMessage>> kvp in _messageStacks)
            {
                while (kvp.Value.Pop(out message))
                {
                    if (message == null)
                    {

                    }
                    else
                    {
                        if (message.DataType == GameState.DataTypeEntityQuery)
                        {
                            //_infoStack.Push(String.Format("Entity {0} queried, sending info", message.index));
                            if (entities.Keys.Contains<int>(message.index))
                                messages[kvp.Key].Add(GameState.SpawnMessage(entities[message.index].typeID, message.index, entities[message.index].Position));
                        }
                        else if (message.DataType == GameState.DataTypeRequest)
                        {
                            switch (message.index)
                            {
                                case GameState.IndexInitialisePlayerShip:
                                    GameMessage initMessage = new GameMessage();
                                    initMessage.DataType = GameState.DataTypeMetaInfo;
                                    initMessage.index = GameState.IndexInitialisePlayerShip;
                                    byte[] arr = new byte[4];
                                    int clientID = kvp.Key;
                                    BitConverter.GetBytes(playerInfo[clientID].EntityID).CopyTo(arr, 0);
                                    initMessage.SetMessage(arr);
                                    messages[kvp.Key].Add(initMessage);

                                    _infoStack.Push(String.Format("Initialisation request from client {0}, ship index {1}", clientID, playerInfo[clientID].EntityID));
                                    break;
                            }
                        }
                        else
                        {
                            //_infoStack.Push(String.Format("Entity {0} Update Received, DataType {1}", message.index, message.DataType));
                            if (message.Message != null)
                                HandleEntityUpdates(message, true);
                        }
                    }
                }
            }
        }

        private void CullEntities()
        {
            List<IRemovable> toBeRemoved = new List<IRemovable>();
            for (int i = removableEntities.Count - 1; i >= 0; --i)
            {
                if (removableEntities[i].isReadyToRemove)
                {
                    toBeRemoved.Add(removableEntities[i]);
                    RemoveEntity(removableEntities[i] as IEntity);
                }
            }
            foreach (IRemovable removable in toBeRemoved)
            {
                removableEntities.Remove(removable);
            }
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

        #region callbacks

        public void ErrorMessage(string errorMessage)
        {
            _errorStack.Push(errorMessage);
        }

        public void Message(int clientNumber, GameMessage message)
        {
            try
            {
                if (message.DataType == GameMessage.Bundle)
                {
                    foreach (GameMessage msg in GameMessage.SplitBundle(message))
                    {
                        _messageStacks[clientNumber].Push(msg);
                    }
                }
                else
                {
                    _messageStacks[clientNumber].Push(message);
                }
            }
            catch (Exception e)
            {
                _errorStack.Push(e.Message);
            }
        }

        public void ClientDisconnect(int clientNumber, GameMessage message)
        {
            try
            {
                _infoStack.Push(String.Format("Client {0} disconnected: {1}", clientNumber, message.messageAsString()));
                messages.Remove(clientNumber);
                _messageStacks.Remove(clientNumber);
                broadcastMessages.Add(DespawnMessage(playerInfo[clientNumber].EntityID));
                RemoveEntity(entities[playerInfo[clientNumber].EntityID]);
                playerInfo.Remove(clientNumber);
            }
            catch (Exception e)
            {
                _errorStack.Push(e.Message);
            }
        }

        public void ClientConnect(int clientNumber, GameMessage message)
        {
            try
            {

                playerInfo.Add(clientNumber, new PlayerInfo(clientNumber));
                _messageStacks.Add(clientNumber, new MessageStack<GameMessage>(50));
                messages.Add(clientNumber, new List<GameMessage>());

                _infoStack.Push(String.Format("Client {0} connected from address {1}", clientNumber, _server.Connections[clientNumber].Socket.RemoteEndPoint));
                PlayerShip ship = new PlayerShip();
                int clientShipIndex = AddEntity(ship);
                ship.Place(new Vector2(Game1.width / 2.0f, Game1.height - 20.0f));

                foreach (IEntity entity in entities.Values)
                {
                    messages[clientNumber].Add(GameState.SpawnMessage(entity.typeID, entity.ID, entity.Position));
                }

                foreach (KeyValuePair<int, List<GameMessage>> kvp in messages)
                {
                    if (kvp.Key != clientNumber)
                        kvp.Value.Add(GameState.SpawnMessage(0, clientShipIndex, ship.Position));
                }

                GameMessage initMessage = new GameMessage();
                initMessage.DataType = GameState.DataTypeMetaInfo;
                initMessage.index = GameState.IndexInitialisePlayerShip;
                byte[] arr = new byte[4];
                BitConverter.GetBytes(clientShipIndex).CopyTo(arr, 0);
                initMessage.SetMessage(arr);
                messages[clientNumber].Add(initMessage);

                playerInfo[clientNumber].EntityID = clientShipIndex;


            }
            catch (Exception e)
            {
                _errorStack.Push(e.Message);
            }
        }

        #endregion

    }
}
