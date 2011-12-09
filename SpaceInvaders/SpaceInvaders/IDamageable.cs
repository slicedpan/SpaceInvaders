using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceInvaders
{
    public interface IDamageable
    {
        void TakeDamage(int amount);
        int Health { get; }
    }
}
