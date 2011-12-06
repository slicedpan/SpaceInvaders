using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ONet;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace SpaceInvaders
{
    public class ClientState : GameState
    {  
        int health;
        int score;
        PlayerShip ship;
        int playerIndex = -1;
        public MessageBox errorBox;
        public static ClientState currentInstance;
        List<IEntity> clientControlled = new List<IEntity>();
        Client _client;
        MessageStack<GameMessage> _messageStack;
        MessageStack<String> _errorStack;
        MessageStack<String> _infoStack;
        List<GameMessage> _messages = new List<GameMessage>();
        List<int> queries = new List<int>();
        List<IEntity> createdEntities = new List<IEntity>();
        Color shipColor = Color.White;

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

        public Client Client
        {
            get
            {
                return _client;
            }
        }

        #endregion        

        public ClientState()
        {
            _client = new Client();
            _client.OnMessage = new Client.Callback(MessageCallback);
            _client.OnError = new Client.ErrorCallback(ErrorCallback);
            _client.OnConnect = new Client.Callback(ConnectCallback);
            _client.OnDisconnect = new Client.Callback(DisconnectCallback);
            _messageStack = new MessageStack<GameMessage>(50);
            _infoStack = new MessageStack<string>(10);
            _errorStack = new MessageStack<string>(10);

            if (currentInstance == null)
                currentInstance = this;
            else
                throw new Exception("only one instance of client state allowed");
        }

        double secondCounter = 0.0d;
        double updateCounter = 0.0d;

        public GameMessage PopulateMessageBundle()
        {
            while (_messages.Count > 50)
            {
                _messages.Remove(_messages[0]);
            }
            return GameMessage.MessageBundle(_messages);
        }
        
        public override void Update(GameTime gameTime)
        {

            secondCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (secondCounter > 1000.0d)
            {
                if (ship != null)
                    //_infoStack.Push("Ship pos: " + ship.Position.ToString());
                secondCounter = 0.0d;
            }

            if (createdEntities.Count > 0)
            {
                foreach (IEntity entity in createdEntities)
                {
                    int newIndex = AddEntity(entity);
                    if (entity is Bullet)
                    {
                        Bullet bullet = entity as Bullet;
                        int ownerIndex = GetIndex(bullet.Owner);
                        _messages.Add(bullet.GetSpawnMessage(ownerIndex));
                    }
                    else
                    {
                        clientControlled.Add(entity);                    
                        _messages.Add(GameState.SpawnMessage(entity.typeID, entity.ID, entity.Position));
                    }
                }
                createdEntities.Clear();
            }

            foreach (IEntity entity in clientControlled)
            {
                if (entity.RequiresUpdate)
                {
                    _messages.Add(entity.GetStateMessage());
                    entity.RequiresUpdate = false;
                }
            }
            updateCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (updateCounter > (1000.0d / Game1.updatesPerSecond))
            {
                _client.Send(PopulateMessageBundle());
                _messages.Clear();
                updateCounter = 0;
            }
            GameMessage msg;
            queries.Clear();
            while (_messageStack.Pop(out msg))
            {
                if (msg != null)
                    HandleMessage(msg);
            }
            base.Update(gameTime);           
        }

        public override void AddEntity(int ID, IEntity entityToAdd)
        {
            entityToAdd.LoadContent(_contentManager);
            base.AddEntity(ID, entityToAdd);
        }

        public void LoadContent(ContentManager cm)
        {
            _contentManager = cm;
        }

        public void InjectInput(KeyboardState keyboardState, MouseState mouseState)
        {
            if (ship != null)
            {
                ship.InjectInput(keyboardState, mouseState);
            }
        }

        #region callbacks

        public void MessageCallback(GameMessage message)
        {
            try
            {
                if (message.DataType == GameMessage.Bundle)
                {
                    List<GameMessage> messages = GameMessage.SplitBundle(message);
                    for (int i = 0; i < messages.Count; ++i)
                    {
                        _messageStack.Push(messages[i]);
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

        void ErrorCallback(String error)
        {
            try
            {
                _errorStack.Push(error);
            }
            catch (Exception e)
            {
                _errorStack.Push(e.Message);
            }
        }

        void ConnectCallback(GameMessage message)
        {
            try
            {
                _infoStack.Push(String.Format("Connected to: {0}", _client.EndPoint));
            }
            catch (Exception e)
            {
                _errorStack.Push(e.Message);
            }
        }

        void DisconnectCallback(GameMessage message)
        {
            try
            {
                _infoStack.Push(String.Format("Disconnected from server: {0}", message.messageAsString()));
            }
            catch (Exception e)
            {
                _errorStack.Push(e.Message);
            }
            _client.Dispose();
            _client = null;
        }

        #endregion

        void HandleMessage(GameMessage message)
        {
            if (message.DataType == GameState.DataTypeMetaInfo)
            {
                switch (message.index)
                {
                    case IndexScoreUpdate:
                        score = BitConverter.ToInt32(message.Message, 0);
                        break;
                    case IndexHealthUpdate:
                        health = BitConverter.ToInt32(message.Message, 0);
                        break;
                    case IndexInitialisePlayerShip:
                        playerIndex = BitConverter.ToInt32(message.Message, 0);                        
                        _infoStack.Push(String.Format("Player Initialised, ship index: {0}", playerIndex));
                        if (entities.Keys.Contains<int>(playerIndex))
                        {
                            InitialiseShip(message);
                        }
                        else
                            Query(playerIndex);
                        break;
                }
            }
            else
            {
                if (message.DataType == DataTypeDespawnEntity)
                {
                    _infoStack.Push("Despawn called for entity: " + message.index.ToString());
                    if (entities.Keys.Contains<int>(message.index))
                        RemoveEntity(entities[message.index]);
                }
                else if (message.DataType == DataTypeSpawnEntity)
                {
                    HandleEntityUpdates(message, true);
                }
                else if (message.DataType == DataTypeReassignID)
                {
                    if (!entities.Keys.Contains<int>(message.index))
                        return;
                    int newIndex = BitConverter.ToInt32(message.Message, 0);
                    _infoStack.Push(String.Format("Reassigning object {0}:{1} to ID {2}", entities[message.index].GetType().ToString(), message.index, newIndex));
                    if (!ReassignID(message.index, newIndex))
                    {
                        int nextIndex = GetNextID();
                        _messages.Add(ReassignIndexMessage(newIndex, nextIndex));
                        ReassignID(newIndex, nextIndex);
                    }
                }
                else
                {
                    if (entities.Keys.Contains<int>(message.index))
                    {
                        //_infoStack.Push(String.Format("Entity {0} Update Received, DataType {1}", message.index, message.DataType));
                        if (clientControlled.Contains(entities[message.index]))
                            HandleEntityUpdates(message, false);
                        else
                            HandleEntityUpdates(message, true);
                    }
                    else
                    {
                        //_infoStack.Push(String.Format("Entity {0} not found, querying", message.index));
                        Query(message.index);
                    }
                }
            }
        }

        private void InitialiseShip(GameMessage message)
        {
            ship = entities[playerIndex] as PlayerShip;
            clientControlled.Add(ship);
            _infoStack.Push("Ship attached");
            ship.color = new Color((int)message.Message[4], (int)message.Message[5], (int)message.Message[6]);
            ship.CreationList = createdEntities;
        }
        void Query(int index)
        {          
            GameMessage query = new GameMessage();
            _infoStack.Push("Querying entity: " + index.ToString());
            query.DataType = GameState.DataTypeEntityQuery;
            query.index = index;
            query.MessageSize = 0;
            queries.Add(index);
            if (!_messages.Contains<GameMessage>(query, new GameMessageComparer()))
                _messages.Add(query);
        }
        void MakeRequest(int index)
        {
            GameMessage request = new GameMessage();
            _infoStack.Push("Making request type: " + index.ToString());
            request.DataType = GameState.DataTypeRequest;
            request.index = index;
            request.MessageSize = 0;            

            if (!_messages.Contains<GameMessage>(request, new GameMessageComparer()))
                _messages.Add(request);
        }
        public void QueryPlayerShip()
        {
            if (playerIndex >= 0)
                Query(playerIndex);
        }
        public void RequestInitialisation()
        {
            MakeRequest(GameState.IndexInitialisePlayerShip);
        }
    }
}
