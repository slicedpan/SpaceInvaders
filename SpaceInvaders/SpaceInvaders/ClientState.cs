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

        public ClientState()
        {
            Game1.Client.OnMessage = new Client.Callback(ServerMessage);
        }
        public void Draw(GameTime gameTime)
        {
            foreach (IEntity entity in entities.Values)
            {
                entity.Draw(gameTime);
            }
        }
        public void InjectInput(KeyboardState keyboardState, MouseState mouseState)
        {
            if (ship != null)
            {
                ship.InjectInput(keyboardState, mouseState);
            }
        }
        public void ServerMessage(GameMessage message)
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
                    case SpawnShip:
                        ship = entities[BitConverter.ToInt32(message.Message, 0)] as PlayerShip;
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
                    break;
                case 2:
                    break;
            }
        }
    }
}
