using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace SpaceInvaders
{
    public class UI
    {
        public Texture2D background;
        public Texture2D sideBorder;
        public Texture2D topbotBorder;
        public Texture2D tlCorner;
        public Texture2D blCorner;

        public void LoadContent(ContentManager Content)
        {
            background = Content.Load<Texture2D>("gradient");
            sideBorder = Content.Load<Texture2D>("sideborder");
            topbotBorder = Content.Load<Texture2D>("topbotborder");
            tlCorner = Content.Load<Texture2D>("tlcorner");
            blCorner = Content.Load<Texture2D>("blcorner");
        }
    }
}
