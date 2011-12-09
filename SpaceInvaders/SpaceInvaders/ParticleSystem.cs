using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceInvaders
{
    public class ParticleSystem : IClientEntity
    {
        List<Particle> particles;
        bool active = true;
        int frameCounter = 0;
        public bool isReadyToRemove
        {
            get { return !active; }
        }

        Texture2D particleSprite;

        public ParticleSystem(int number, Vector2 position, Color color)
        {
            particles = new List<Particle>();
            for (int i = 0; i < number; ++i)
            {
                particles.Add(new Particle(position, color));
            }
        }

        public void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            ++frameCounter;
            if (frameCounter > 180)
                active = false;
            foreach (Particle particle in particles)
            {
                particle.Update(gameTime);
            }
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            particleSprite = Content.Load<Texture2D>("particle");
            foreach (Particle particle in particles)
            {
                particle.sprite = particleSprite;
            }
        }

        public void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            foreach (Particle particle in particles)
            {
                particle.Draw(gameTime);
            }
        }        
    }
}
