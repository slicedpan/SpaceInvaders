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
        double lastTime;
        Dictionary<int, List<GameMessage>> messages = new Dictionary<int,List<GameMessage>>();
        Dictionary<int, double> lastMessage = new Dictionary<int,double>();
        Dictionary<int, PlayerShip> ships = new Dictionary<int, PlayerShip>();
        List<GameMessage> broadcastMessages = new List<GameMessage>();
        public GameMessage CurrentBundle;
        double updateCounter = 0.0d;
        double secondCounter = 0.0d;
        Dictionary<int, PlayerInfo> playerInfo;
        Dictionary<int, MessageStack<GameMessage>> _messageStacks = new Dictionary<int, MessageStack<GameMessage>>();
        public static ServerState currentInstance;
        int numShips = 0;
        int maxNumShips = 8;
        bool gameActive = true;

        Color[] shipColors = new Color[16]
        {
            Color.Green,
            Color.Blue,
            Color.Brown,
            Color.Cyan,
            Color.DarkGray,
            Color.DarkOliveGreen,
            Color.DarkSlateBlue,
            Color.DarkViolet,
            Color.ForestGreen,
            Color.Goldenrod,
            Color.Khaki,
            Color.LemonChiffon,
            Color.LightPink,
            Color.Linen,
            Color.LightSlateGray,
            Color.MediumBlue
        };

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
            float offset = 150.0f;
            createdEntities.AddRange(BuildingChunk.CreateBuilding(new Vector2(512.0f, 600.0f)));
            createdEntities.AddRange(BuildingChunk.CreateBuilding(new Vector2(512.0f + offset, 600.0f)));
            createdEntities.AddRange(BuildingChunk.CreateBuilding(new Vector2(512.0f + 2 * offset, 600.0f)));
            createdEntities.AddRange(BuildingChunk.CreateBuilding(new Vector2(512.0f - offset, 600.0f)));
            createdEntities.AddRange(BuildingChunk.CreateBuilding(new Vector2(512.0f - 2 * offset, 600.0f)));
        }

        public override void AddEntity(int ID, IEntity entityToAdd)
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
            base.AddEntity(ID, entityToAdd);
        }

        public override void RemoveEntity(IEntity entityToRemove)
        {
            broadcastMessages.Add(GameState.DespawnMessage(entityToRemove.ID));
            if (entityToRemove is IAIControlled)
            {
                AIControlledEntities.Remove(entityToRemove as IAIControlled);
            }
            base.RemoveEntity(entityToRemove);
        }

        int secondsElapsed = 0;

        public override void Update(GameTime gameTime)
        {
            updateCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
            secondCounter += gameTime.ElapsedGameTime.TotalMilliseconds;

            foreach (KeyValuePair<int, PlayerInfo> kvp in playerInfo)
            {
                if (ships.Keys.Contains<int>(kvp.Key))
                {
                    kvp.Value.Score += ships[kvp.Key].score;
                    ships[kvp.Key].score = 0;
                }
            }

            lastTime = gameTime.TotalGameTime.TotalSeconds;
            if (gameActive)
            {
                foreach (IAIControlled AI in AIControlledEntities)
                {
                    AI.Think(gameTime);
                }
            }

            if (createdEntities.Count > 0)
            {
                int newIndex = AddEntity(createdEntities[0]);
                broadcastMessages.Add(createdEntities[0].GetSpawnMessage());
                createdEntities.Remove(createdEntities[0]);
            }
            if (gameActive)
            {
                GenerateMetaInfo();
            }

            CullEntities();

            HandleMessages();

            UpdateClients();

            if (secondCounter > 1000.0d)
            {
                foreach (IEntity entity in entities.Values)
                {
                    //_infoStack.Push(String.Format("Entity {0}, position {1}", entity.ID, entity.Position));
                }
                secondCounter = 0.0d;
                ++secondsElapsed;
                if (secondsElapsed > 30)
                {
                    maxNumShips += 2;
                    secondsElapsed = 0;
                }
            }
            if (gameActive)
                base.Update(gameTime);
        }

        private void GenerateMetaInfo()
        {
            int numPlayers = 0;
            int deadPlayers = 0;
            List<int> deadShips = new List<int>();
            foreach (KeyValuePair<int, PlayerShip> kvp in ships)
            {
                ++numPlayers;
                if (kvp.Value.isDead)
                {
                    RemoveEntity(kvp.Value);
                    GameMessage deathMessage = new GameMessage();
                    deathMessage.DataType = DataTypeMetaInfo;
                    deathMessage.index = IndexPlayerDeath;
                    deathMessage.SetMessage(new byte[1]);
                    playerInfo[kvp.Key].Alive = false;
                    messages[kvp.Key].Add(deathMessage);
                    ++deadPlayers;
                    deadShips.Add(kvp.Key);
                }
                GameMessage healthMessage = new GameMessage();
                healthMessage.DataType = DataTypeMetaInfo;
                healthMessage.index = IndexHealthUpdate;
                healthMessage.SetMessage(BitConverter.GetBytes(kvp.Value.health));
                GameMessage scoreMessage = new GameMessage();
                scoreMessage.DataType = DataTypeMetaInfo;
                scoreMessage.index = IndexScoreUpdate;
                scoreMessage.SetMessage(BitConverter.GetBytes(playerInfo[kvp.Key].Score));
                messages[kvp.Key].Add(healthMessage);
                messages[kvp.Key].Add(scoreMessage);
            }
            foreach (int index in deadShips)
            {
                ships.Remove(index);
            }
            /*
            if (deadPlayers == numPlayers)
            {
                GameMessage gameOverMessage = new GameMessage();
                gameOverMessage.DataType = DataTypeMetaInfo;
                gameOverMessage.index = IndexGameOver;
                gameOverMessage.SetMessage(new byte[1]);
                broadcastMessages.Add(gameOverMessage);
                gameActive = false;
            }
            */
        }

        private void UpdateClients()
        {
            if (updateCounter > (1000.0d / Game1.updatesPerSecond))
            {
                if (numShips < maxNumShips)
                {
                    var ship = new EnemyShip();
                    ship.Place(new Vector2(rand.Next(Game1.width), -100));
                    createdEntities.Add(ship);
                    ++numShips;
                }
                foreach (IEntity entity in entities.Values)
                {
                    if (entity.RequiresUpdate)
                    {
                        entity.RequiresUpdate = false;
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
                            messageList.Value.Clear();
                        }
                        else if (messageList.Value.Count < 100)
                        {
                            Connection conn;
                            if (_server.Connections.TryGetValue(messageList.Key, out conn))
                                conn.Send(GameMessage.MessageBundle(messageList.Value));
                            else
                            {

                            }
                            messageList.Value.Clear();
                        }
                        else
                        {
                            List<GameMessage> updateMessages = new List<GameMessage>();
                            for (int i = 0; i < 100; ++i)
                            {
                                updateMessages.Add(messageList.Value[0]);
                                messageList.Value.Remove(messageList.Value[0]);
                            }
                            Connection conn;
                            if (_server.Connections.TryGetValue(messageList.Key, out conn))
                            {
                                conn.Send(GameMessage.MessageBundle(updateMessages));
                            }
                            else
                            {

                            }
                        }                        
                    }
                }
                broadcastMessages.Clear();
                updateCounter = 0.0d;
            }
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
                            {
                                _infoStack.Push(String.Format("Received query for entity {0}:{1}", message.index, entities[message.index].GetType().ToString()));
                                 messages[kvp.Key].Add(entities[message.index].GetSpawnMessage());
                            }
                            else
                            {
                                _infoStack.Push(String.Format("Received query for entity {0}, does not exist, sending despawn message", message.index));
                                messages[kvp.Key].Add(GameState.DespawnMessage(message.index));
                            }
                        }
                        else if (message.DataType == GameState.DataTypeRequest)
                        {
                            switch (message.index)
                            {
                                case GameState.IndexInitialisePlayerShip:
                                    InitialisePlayerShip(kvp.Key);
                                    _infoStack.Push(String.Format("Initialisation request from client {0}, ship index {1}", kvp.Key, playerInfo[kvp.Key].EntityID));
                                    break;
                                case GameState.IndexRespawnShip:

                                    if (ships.Keys.Contains<int>(kvp.Key))
                                    {
                                        ships.Remove(kvp.Key);
                                    }

                                    PlayerShip ship = new PlayerShip();
                                    int clientShipIndex = AddEntity(ship);
                                    ship.Place(new Vector2(rand.Next(Game1.width - 100) + 50, Game1.height - 50.0f));
                                    ship.color = shipColors[kvp.Key % 16];
                                    ships.Add(kvp.Key, ship);
                                    playerInfo[kvp.Key].EntityID = clientShipIndex;
                                    InitialisePlayerShip(kvp.Key);
                                    
                                    break;
                            }
                        }
                        else if (message.DataType == GameState.DataTypeReassignID)
                        {
                            if (!entities.Keys.Contains<int>(message.index))
                                return;
                            int newIndex = BitConverter.ToInt32(message.Message, 0);
                            _infoStack.Push(String.Format("Reassigning object {0}:{1} to ID {2}", entities[message.index].GetType().ToString(), message.index, newIndex));
                            if (!ReassignID(message.index, newIndex))
                            {
                                int nextIndex = GetNextID();
                                messages[kvp.Key].Add(ReassignIndexMessage(newIndex, nextIndex));
                                ReassignID(newIndex, nextIndex);
                            }
                        }
                        else
                        {
                            //_infoStack.Push(String.Format("Entity {0} Update Received, DataType {1}", message.index, message.DataType));
                            if (message.DataType == GameState.DataTypeSpawnEntity)
                            {
                                _infoStack.Push(String.Format("Spawning entity. index: {0}, typeID: {1}", message.index, BitConverter.ToInt32(message.Message, 16)));
                                if (message.index > idCounter)
                                    idCounter = message.index + 1;
                                if (entities.Keys.Contains<int>(message.index))
                                {
                                    int nextID = GetNextID();
                                    messages[kvp.Key].Add(ReassignIndexMessage(message.index, nextID));
                                    message.index = nextID;
                                    Spawn(message);
                                }
                                else
                                {
                                    HandleEntityUpdates(message, true);
                                }
                            }
                            else
                            {
                                if (message.Message != null)
                                    HandleEntityUpdates(message, true);
                            }
                        }
                    }
                }
            }
        }

        private void InitialisePlayerShip(int clientIndex)
        {
            GameMessage initMessage = new GameMessage();
            initMessage.DataType = GameState.DataTypeMetaInfo;
            initMessage.index = GameState.IndexInitialisePlayerShip;
            byte[] arr = new byte[4];
            BitConverter.GetBytes(playerInfo[clientIndex].EntityID).CopyTo(arr, 0);
            initMessage.SetMessage(arr);
            messages[clientIndex].Add(initMessage);
        }        

        private void CullEntities()
        {
            List<IRemovable> toBeRemoved = new List<IRemovable>();
            for (int i = removableEntities.Count - 1; i >= 0; --i)
            {
                if (removableEntities[i].isReadyToRemove)
                {
                    toBeRemoved.Add(removableEntities[i]);
                    _infoStack.Push(String.Format("Removing entity {0}:{1}", (removableEntities[i] as IEntity).ID, removableEntities[i].GetType().ToString()));
                    RemoveEntity(removableEntities[i] as IEntity);
                    if (removableEntities[i] is EnemyShip)
                          --numShips;
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
                lastMessage[clientNumber] = lastTime;
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
                _messageStacks.Add(clientNumber, new MessageStack<GameMessage>(500));
                messages.Add(clientNumber, new List<GameMessage>());

                _infoStack.Push(String.Format("Client {0} connected from address {1}", clientNumber, _server.Connections[clientNumber].Socket.RemoteEndPoint));
                PlayerShip ship = new PlayerShip();
                int clientShipIndex = AddEntity(ship);
                ship.Place(new Vector2(rand.Next(Game1.width - 100) + 50.0f, Game1.height - 50.0f));
                ship.color = shipColors[clientNumber % 16];

                ships.Add(clientNumber, ship);

                foreach (IEntity entity in entities.Values)
                {
                    if (entity != ship)
                    {
                        messages[clientNumber].Add(entity.GetSpawnMessage());                        
                    }
                }

                foreach (KeyValuePair<int, List<GameMessage>> kvp in messages)
                {                   
                    kvp.Value.Add(ship.GetSpawnMessage());                
                }

                GameMessage initMessage = new GameMessage();
                initMessage.DataType = GameState.DataTypeMetaInfo;
                initMessage.index = GameState.IndexInitialisePlayerShip;
                byte[] arr = new byte[7];
                BitConverter.GetBytes(clientShipIndex).CopyTo(arr, 0);

                arr[4] = shipColors[clientNumber % 16].R;
                arr[5] = shipColors[clientNumber % 16].G;
                arr[6] = shipColors[clientNumber % 16].B;

                initMessage.SetMessage(arr);
                messages[clientNumber].Add(initMessage);

                playerInfo[clientNumber].EntityID = clientShipIndex;
                lastMessage.Add(clientNumber, lastTime);

            }
            catch (Exception e)
            {
                _errorStack.Push(e.Message);
            }
        }

        #endregion

    }
}
