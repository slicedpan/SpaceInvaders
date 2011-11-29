using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ONet;

namespace SpaceInvaders
{
    public interface IEntity
    {
        void Update(GameTime gameTime);
        void LoadContent(ContentManager Content);
        void Draw(GameTime gameTime);
        BoundingSphere BoundingSphere { get; }
        int ID { get; set; }
        void HandleMessage(GameMessage message);
        GameMessage GetStateMessage();
        int typeID { get; }
        Vector2 Position { get; }
        bool RequiresUpdate { get; set; }
    }
}
