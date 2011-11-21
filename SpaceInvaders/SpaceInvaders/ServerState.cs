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
        List<GameMessage> messages;
        public ServerState()
        {
            messages = new List<GameMessage>();
        }
        public void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
        public GameMessage GetMessageBundle()
        {
            byte[] array;
            int length = 0;
            foreach (GameMessage message in messages)
            {
                length += 6;
                length += message.MessageSize;
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
            return bundle;
        }
    }
}
