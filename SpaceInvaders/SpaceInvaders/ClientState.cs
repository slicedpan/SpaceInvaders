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

        public ClientState()
        {
            _client = new Client();
            _client.OnMessage = new Client.Callback(MessageCallback);
            _client.OnError = new Client.ErrorCallback(ErrorCallback);
            _client.OnConnect = new Client.Callback(ConnectCallback);
            _client.OnDisconnect = new Client.Callback(DisconnectCallback);
            _messageStack = new MessageStack<GameMessage>(10);
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
                    _infoStack.Push("Ship pos: " + ship.Position.ToString());
                secondCounter = 0.0d;
            }

            if (createdEntities.Count > 0)
            {
                foreach (IEntity entity in createdEntities)
                {
                    int newIndex = AddEntity(entity);
                    _messages.Add(GameState.SpawnMessage(entity.typeID, entity.ID, entity.Position));
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
                if (msg != null && msg.Message != null)
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
            else
            {
                if (entities.Keys.Contains<int>(playerIndex))
                {
                    ship = entities[playerIndex] as PlayerShip;
                    _infoStack.Push("Ship attached");
                }
            }
        }
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
                            ship = entities[playerIndex] as PlayerShip;
                            clientControlled.Add(ship);
                            _infoStack.Push("Ship attached");
                        }
                        else
                            Query((ushort)playerIndex);
                        break;
                }
            }
            else
            {
                if (message.DataType == DataTypeSpawnEntity)
                {
                    HandleEntityUpdates(message, true);
                }
                else
                {
                    if (entities.Keys.Contains<int>(message.index))
                    {
                        //_infoStack.Push(String.Format("Entity {0} Update Received, DataType {1}", message.index, message.DataType));
                        HandleEntityUpdates(message, false);
                        
                    }
                    else
                    {
                        //_infoStack.Push(String.Format("Entity {0} not found, querying", message.index));
                        Query(message.index);
                    }
                }
            }
        }
        void Query(ushort index)
        {
            if (queries.Contains(index))            
                return;            
            GameMessage query = new GameMessage();
            query.DataType = GameState.DataTypeQuery;
            query.index = index;
            query.MessageSize = 0;
            queries.Add(index);
            _messages.Add(query);
        }
    }
}
