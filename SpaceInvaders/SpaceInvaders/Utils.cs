using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SpaceInvaders
{
    class Utils
    {
        public static bool Intersects(int X, int Y, Rectangle rect)
        {
            if (X > rect.Left && X < rect.Right)
            {
                if (Y > rect.Top && Y < rect.Bottom)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
