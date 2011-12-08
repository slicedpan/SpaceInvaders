﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SpaceInvaders
{
    public class BuildingChunk : PhysicalEntity, IDamageable, IRemovable
    {
        int health = 100;
        Texture2D sprite;

        public override int typeID
        {
            get
            {
                return 3;
            }
        }       

        public BuildingChunk()
        {
            mass = float.MaxValue;  //immovable for all intents and purposes
            collisionRadius = 8.0f;
        }

        public override void Update(GameTime gameTime)
        {
            Velocity = Vector2.Zero;
            base.Update(gameTime);
        }

        public void TakeDamage(int amount)
        {
            health -= 10;
        }

        public bool isReadyToRemove
        {
            get { return (health <= 0); }
        }

        public override void Draw(GameTime gameTime)
        {
            Rectangle rect = new Rectangle((int)_position.X - sprite.Width / 2, (int)_position.Y + 3 * sprite.Height / 2, sprite.Width, sprite.Height);
            Game1.SpriteBatch.Draw(sprite, rect, new Color(0.0f, 0.3f * (health / 100.0f) + 0.7f, 0.0f) * (0.3f * (health / 100.0f) + 0.7f));
        }

        public override void LoadContent(ContentManager Content)
        {
            sprite = Content.Load<Texture2D>("chunk");
        }

        public static List<IEntity> CreateBuilding(Vector2 position)
        {
            List<IEntity> retList = new List<IEntity>();
            Vector2 xOffs = new Vector2(8.0f, 0.0f);
            Vector2 yOffs = new Vector2(0.0f, -8.0f);
            var chunk = new BuildingChunk();
            chunk.Place(position);
            retList.Add(chunk);
            chunk = new BuildingChunk();
            chunk.Place(position + xOffs);
            retList.Add(chunk);
            chunk = new BuildingChunk();
            chunk.Place(position - xOffs);
            retList.Add(chunk);
            chunk = new BuildingChunk();
            chunk.Place(position + 2 * xOffs);
            retList.Add(chunk);
            chunk = new BuildingChunk();
            chunk.Place(position - 2 * xOffs);
            retList.Add(chunk);
            chunk = new BuildingChunk();
            chunk.Place(position + 3 * xOffs);
            retList.Add(chunk);
            chunk = new BuildingChunk();
            chunk.Place(position - 3 * xOffs);
            retList.Add(chunk);

            chunk = new BuildingChunk();
            chunk.Place(position + 3 * xOffs - yOffs);
            retList.Add(chunk);
            chunk = new BuildingChunk();
            chunk.Place(position - 3 * xOffs - yOffs);
            retList.Add(chunk);

            chunk = new BuildingChunk();
            chunk.Place(position + yOffs);
            retList.Add(chunk);
            chunk = new BuildingChunk();
            chunk.Place(position + xOffs + yOffs);
            retList.Add(chunk);
            chunk = new BuildingChunk();
            chunk.Place(position - xOffs + yOffs);
            retList.Add(chunk);
            chunk = new BuildingChunk();
            chunk.Place(position + 2 * xOffs + yOffs);
            retList.Add(chunk);
            chunk = new BuildingChunk();
            chunk.Place(position - 2 * xOffs + yOffs);
            retList.Add(chunk);
            chunk = new BuildingChunk();
            chunk.Place(position + 3 * xOffs + yOffs);
            retList.Add(chunk);
            chunk = new BuildingChunk();
            chunk.Place(position - 3 * xOffs + yOffs);
            retList.Add(chunk);

            chunk = new BuildingChunk();
            chunk.Place(position + 2 * yOffs);
            retList.Add(chunk);
            chunk = new BuildingChunk();
            chunk.Place(position + xOffs + 2 * yOffs);
            retList.Add(chunk);
            chunk = new BuildingChunk();
            chunk.Place(position - xOffs + 2 * yOffs);
            retList.Add(chunk);
            chunk = new BuildingChunk();
            chunk.Place(position + 2 * xOffs + 2 * yOffs);
            retList.Add(chunk);
            chunk = new BuildingChunk();
            chunk.Place(position - 2 * xOffs + 2 * yOffs);
            retList.Add(chunk);
            chunk = new BuildingChunk();
            chunk.Place(position + 3 * xOffs + 2 * yOffs);
            retList.Add(chunk);
            chunk = new BuildingChunk();
            chunk.Place(position - 3 * xOffs + 2 * yOffs);
            retList.Add(chunk);

            chunk = new BuildingChunk();
            chunk.Place(position + 3 * yOffs);
            retList.Add(chunk);
            chunk = new BuildingChunk();
            chunk.Place(position + xOffs + 3 * yOffs);
            retList.Add(chunk);
            chunk = new BuildingChunk();
            chunk.Place(position - xOffs + 3 * yOffs);
            retList.Add(chunk);
            chunk = new BuildingChunk();
            chunk.Place(position + 2 * xOffs + 3 * yOffs);
            retList.Add(chunk);
            chunk = new BuildingChunk();
            chunk.Place(position - 2 * xOffs + 3 * yOffs);
            retList.Add(chunk);
            
            return retList;
        }

        public override ONet.GameMessage GetStateMessage()
        {
            return base.GetStateMessage();
        }
    }
}