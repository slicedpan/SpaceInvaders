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
        double updateCounter = 0.0d;
        double secondCounter = 0.0d;
        const double updatesPerSec = 20.0d;
        Dictionary<int, PlayerInfo> playerInfo;
        public static ServerState currentInstance;

        GameServer _server;
        MessageStack<GameMessage> _messageStack;
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
            _messageStack = new MessageStack<GameMessage>(10);
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

            GameMessage message;

            foreach (IAIControlled AI in AIControlledEntities)
            {
                AI.Think(gameTime);                
            }

            if (createdEntities.Count > 0)
            {
                foreach (IEntity entity in createdEntities)
                {
                    int newIndex = AddEntity(entity);
                    messages.Add(GameState.SpawnMessage(entity.typeID, entity.ID, entity.Position));
                }
                createdEntities.Clear();
            }

            CullEntities();

            while (_messageStack.Pop(out message))
            {
                if (message == null)
                {

                }
                else
                {                    
                    if (message.DataType == GameState.DataTypeQuery)
                    {
                        //_infoStack.Push(String.Format("Entity {0} queried, sending info", message.index));
                        if (entities.Keys.Contains<int>(message.index))
                            messages.Add(GameState.SpawnMessage(entities[message.index].typeID, message.index, entities[message.index].Position));
                    }
                    else 
                    {
                        //_infoStack.Push(String.Format("Entity {0} Update Received, DataType {1}", message.index, message.DataType));
                        if (message.Message != null)
                            HandleEntityUpdates(message, true); 
                    }
                }
            }

            if (updateCounter > (1000.0d / Game1.updatesPerSecond))
            {
                foreach (IEntity entity in entities.Values)
                {
                    if (entity.RequiresUpdate)
                    {
                        messages.Add(entity.GetStateMessage());
                    }
                }
                if (messages.Count > 0)
                {
                    if (messages.Count == 1)
                    {
                        _server.Send(messages[0]);
                    }
                    else
                    {
                        _server.Send(PopulateMessageBundle());
                        messages.Clear();
                    }
                }
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
        public GameMessage PopulateMessageBundle()
        {
            while (messages.Count > 50)
            {
                messages.Remove(messages[0]);
            }
            return GameMessage.MessageBundle(messages);
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
            try
            {
                if (message.DataType == GameMessage.Bundle)
                {
                    foreach (GameMessage msg in GameMessage.SplitBundle(message))
                    {
                        _messageStack.Push(msg);
                    }
                }
                else
                {
                    _messageStack.Push(message);
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
                _infoStack.Push(String.Format("Client {0} connected from address {1}", clientNumber, _server.Connections[clientNumber].Socket.RemoteEndPoint));
                PlayerShip ship = new PlayerShip();
                int clientShipIndex = AddEntity(ship);
                ship.Place(new Vector2(Game1.width / 2.0f, Game1.height - 20.0f));

                List<GameMessage> initMessages = new List<GameMessage>();

                foreach (IEntity entity in entities.Values)
                {
                    initMessages.Add(GameState.SpawnMessage(entity.typeID, entity.ID, entity.Position));
                }

                foreach (KeyValuePair<int, Connection> kvp in _server.Connections)
                {
                    if (kvp.Key != clientNumber)
                        kvp.Value.Send(GameState.SpawnMessage(0, clientShipIndex, ship.Position));
                }

                GameMessage initMessage = new GameMessage();
                initMessage.DataType = GameState.DataTypeMetaInfo;
                initMessage.index = GameState.IndexInitialisePlayerShip;
                byte[] arr = new byte[4];
                BitConverter.GetBytes(clientShipIndex).CopyTo(arr, 0);
                initMessage.SetMessage(arr);
                initMessages.Add(initMessage);
                _server.Connections[clientNumber].Send(GameMessage.MessageBundle(initMessages));
            }
            catch (Exception e)
            {
                _errorStack.Push(e.Message);
            }
        }        
    }
}
