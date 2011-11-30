using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ONet;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace SpaceInvaders
{
    class EnemyShip : PhysicalEntity, IAIControlled
    {
        Texture2D sprite;
        Vector2 AITarget;
        Random rand;
        double nextFire = -1.0d;
        double fireRate = 0.5d;
        List<IEntity> _createList;

        public override float MaxSpeed
        {
            get
            {
                return 6.0f;
            }
        }
        public override float MaxAccel
        {
            get
            {
                return 0.4f;
            }
        }

        public EnemyShip()
        {
            rand = new Random();
            AITarget = _position;
            mass = 20.0f;
        }

        public override int typeID
        {
            get
            {
                return 1;
            }
        }
        public override void LoadContent(ContentManager Content)
        {
            sprite = Content.Load<Texture2D>("enemyship");
        }
        public override void Draw(GameTime gameTime)
        {
            Game1.SpriteBatch.Draw(sprite, _position, Color.White);
        }   

        public void Think(GameTime gameTime)
        {
            if ((_position - AITarget).Length() < 4.0f)
            {
                ChooseNewTarget();
            }
            else
            {
                Vector2 velocityDir = (AITarget - _position);
                velocityDir.Normalize();
                Velocity += velocityDir * MaxAccel;
            }

            if (nextFire < 0.0d)
                nextFire = (1.0d + (rand.NextDouble() / 2.0d)) * (1000.0d / fireRate) + gameTime.TotalGameTime.TotalMilliseconds;
            else if (nextFire < gameTime.TotalGameTime.TotalMilliseconds)
            {
                Fire();
                nextFire = (1.0d + (rand.NextDouble() / 2.0d)) * (1000.0d / fireRate) + gameTime.TotalGameTime.TotalMilliseconds;
            }            
        }

        void Fire()
        {
            if (_createList != null)
            {
                var bullet = new Bullet(this);
                bullet.Place(_position + new Vector2(0.0f, 1.0f));
                bullet.Velocity = new Vector2(0.0f, 20.0f);
                _createList.Add(bullet);
            }
        }

        void ChooseNewTarget()
        {
            AITarget = new Vector2((float)rand.Next(Game1.width), (float)rand.Next(Game1.height));
        }

        public Vector2 Target
        {
            get { return AITarget; }
        }


        public List<IEntity> creationList
        {
            set { _createList = value; }
        }


    }
}
