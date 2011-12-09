using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceInvaders
{
    public class Particle
    {
        public Texture2D sprite;
        Vector2 position;
        Vector2 velocity;
        Color color;
        float alpha = 1.0f;
        int frameLifetime = 0;
        public Particle(Vector2 position, Color color)
        {
            this.position = position;
            velocity = new Vector2(Game1.rand.Next(14) - 7, Game1.rand.Next(14) - 7);
            frameLifetime = Game1.rand.Next(60) + 60;
            int newR = color.R;
            int newG = color.G;
            int newB = color.B;
            newR += (Game1.rand.Next(150) - 60);
            newG += (Game1.rand.Next(150) - 60);
            newB += (Game1.rand.Next(150) - 60);
            newR = Math.Abs(newR);
            newG = Math.Abs(newG);
            newB = Math.Abs(newB);
            if (newR > 255)            
                newR = 255 - (newR - 255);            
            if (newG > 255)
                newG = 255 - (newG - 255);
            if (newB > 255)
                newB = 255 - (newB - 255);
            this.color = new Color(newR, newG, newB);
        }
        public void Draw(GameTime gameTime)
        {
            Rectangle rect = new Rectangle((int)position.X, (int)position.Y, 4, 4);
            Game1.SpriteBatch.Draw(sprite, rect, this.color * alpha);
        }        
        public void Update(GameTime gameTime)
        {
            position += velocity;
            alpha -= 1.0f / frameLifetime;
        }
    }
}
