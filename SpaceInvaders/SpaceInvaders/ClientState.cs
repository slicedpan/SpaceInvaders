using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ONet;
using Microsoft.Xna.Framework.Input;

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

        MessageStack<String> InfoStack
        {
            get
            {
                return _infoStack;
            }
        }

        MessageStack<String> ErrorStack
        {
            get
            {
                return _errorStack;
            }
        }
        MessageStack<GameMessage> GameMessageStack
        {
            get
            {
                return _messageStack;
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
            if (currentInstance == null)
                currentInstance = this;
            else
                throw new Exception("only one instance of client state allowed");
        }
        public void Draw(GameTime gameTime)
        {
            foreach (IEntity entity in entities.Values)
            {
                entity.Draw(gameTime);
            }
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
            if (message.DataType == GameMessage.Bundle || message.DataType == 1)
                HandleEntityUpdates(message);
            else if (message.DataType == 2)
            {
                switch (message.index)
                {
                    case ScoreUpdate:
                        score = BitConverter.ToInt32(message.Message, 0);
                        break;
                    case HealthUpdate:
                        health = BitConverter.ToInt32(message.Message, 0);
                        break;
                    case SpawnEntity:
                        Spawn(BitConverter.ToInt32(message.Message, 0), new Vector2(BitConverter.ToSingle(message.Message, 4), BitConverter.ToSingle(message.Message, 8)));
                        break;
                }
            }
        }
        private void Spawn(int p, Vector2 position)
        {
            switch (p)
            {
                case 1:
                    ship = new PlayerShip();
                    ship.Position = position;
                    playerIndex = p;
                    AddEntity(p, ship);                    
                    break;
                case 2:
                    break;
            }
        }
    }
}
