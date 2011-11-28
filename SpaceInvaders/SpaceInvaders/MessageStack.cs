using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ONet;

namespace SpaceInvaders
{
    public class MessageStack<T>
    {
        List<T> messages = new List<T>();
        public bool Pop(out T message)
        {
            if (messages.Count == 0)
            {
                message = default(T);
                return false;
            }
            else
            {
                message = messages[0];
                messages.Remove(message);
                return true;
            }
        }
        public void Push(T message)
        {
            messages.Add(message);
        }
    }
}
