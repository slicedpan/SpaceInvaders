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
        public static Color ColorFromBytes(byte[] array, int offset)
        {
            Color color = new Color((int)array[offset], (int)array[offset + 1], (int)array[offset + 2]);
            return color;
        }
    }
    public static class ExtensionMethods
    {
        public static void FromBytes(this Color color, byte[] array)
        {
            color.FromBytes(array, 0);
        }
        public static void FromBytes(this Color color, byte[] array, int offset)
        {
            if (array.Length < 3 + offset)
                throw new ArgumentException("length of array is too small");
            color.R = array[offset];
            color.G = array[offset + 1];
            color.B = array[offset + 2];
            if (array.Length > 3 + offset)
                color.A = array[3 + offset];
        }
    }    
}
