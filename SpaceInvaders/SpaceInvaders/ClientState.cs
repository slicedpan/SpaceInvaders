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
        
        public override void Update(GameTime gameTime)
        {

            secondCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (secondCounter > 1000.0d)
            {
                if (ship != null)
                    _infoStack.Push("Ship pos: " + ship.Position.ToString());
                secondCounter = 0.0d;
            }

            foreach (IEntity entity in clientControlled)
            {
                if (entity.RequiresUpdate)
                {
                    _messages.Add(entity.GetStateMessage());
                    entity.RequiresUpdate = false;
                }
            }

            _client.Send(GameMessage.MessageBundle(_messages));

            GameMessage msg;
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
                    ship = entities[playerIndex] as PlayerShip;
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
                        if (entities.Keys.Contains<int>(playerIndex))
                        {
                            ship = entities[playerIndex] as PlayerShip;
                            clientControlled.Add(ship);
                        }
                        else
                            Query((ushort)playerIndex);
                        break;
                }
            }
            else
            {
                if (entities.Keys.Contains<int>(message.index))
                {
                    HandleEntityUpdates(message);
                }
                else
                {
                    Query(message.index);
                }
            }
        }
        void Query(ushort index)
        {
            GameMessage query = new GameMessage();
            query.DataType = GameState.DataTypeQuery;
            query.index = index;
            query.MessageSize = 0;
            _messages.Add(query);
        }
    }
}
