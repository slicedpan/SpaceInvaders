using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceInvaders
{
    public interface IRemovable
    {
        bool isReadyToRemove { get; }
    }
}
