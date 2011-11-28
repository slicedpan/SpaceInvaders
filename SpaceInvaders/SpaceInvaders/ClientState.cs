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
        Client _client;
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
            _errorStack.Push(error);
        }

        void ConnectCallback(GameMessage message)
        {
            _infoStack.Push(String.Format("Connected to: {0}", _client.EndPoint));
        }

        void DisconnectCallback(GameMessage message)
        {
            _infoStack.Push(String.Format("Disconnected from server: {0}", message.messageAsString()));
        }

        public ClientState()
        {
            _client = new Client();
            _client.OnMessage = new Client.Callback(MessageCallback);
            _client.OnError = new Client.ErrorCallback(ErrorCallback);
            _client.OnConnect = new Client.Callback(ConnectCallback);
            _client.OnDisconnect = new Client.Callback(DisconnectCallback);
            _messageStack = new MessageStack<GameMessage>();
            _infoStack = new MessageStack<string>();
            _errorStack = new MessageStack<string>();

            if (currentInstance == null)
                currentInstance = this;
            else
                throw new Exception("only one instance of client state allowed");
        }
        
        public override void Update(GameTime gameTime)
        {
            if (ship == null)
            {
                if (playerIndex >= 0)
                {
                    ship = entities[playerIndex] as PlayerShip;
                }
            }
            
            GameMessage msg;
            while (_messageStack.Pop(out msg))
            {
                HandleMessage(msg);
            }
            base.Update(gameTime);

            foreach (IEntity entity in entities.Values)
            {
                _client.Send(entity.GetStateMessage());
            }            
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
        public void MessageCallback(GameMessage message)
        {
            _messageStack.Push(message);            
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
                        ship = entities[playerIndex] as PlayerShip;
                        break;
                }
            }
            else
            {
                HandleEntityUpdates(message);
            }
        }
        
    }
}
