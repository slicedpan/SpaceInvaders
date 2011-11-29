using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpaceInvaders;
using ONet;

namespace ConsoleApplication1
{
    class Program
    {
        static MessageStack<string> stack = new MessageStack<string>(10);
        static MessageStack<GameMessage> messageStack = new MessageStack<GameMessage>(10);

        static bool compareMessage(GameMessage m1, GameMessage m2)
        {
            if (m1.MessageSize != m2.MessageSize)
                return false;
            for (int i = 0; i < m1.MessageSize; ++i)
            {
                if (m1.Message[i] != m2.Message[i])
                    return false;
            }
            return (m1.DataType == m2.DataType && m1.index == m2.index);
        }    

        static void Main(string[] args)
        {
            List<String> strings = new List<string>();
            Random rand = new Random();
            strings.Add("blah");
            strings.Add("another string");
            strings.Add("string number 3");
            foreach (String txt in strings)
            {
                stack.Push(txt);
            }
            String outStr;
            int j = 0;
            while (stack.Pop(out outStr))
            {
                if (outStr != strings[j])
                    Console.WriteLine("string {0} does not match");
                ++j;
            }
            GameMessage[] messages = new GameMessage[10];
            for (int i = 0; i < 10; ++i)
            {
                messages[i] = new GameMessage();
                messages[i].DataType = (ushort)rand.Next(65535);
                messages[i].index = (ushort)rand.Next(65535);
                messages[i].MessageSize = (ushort)rand.Next(64);
                byte[] bytes = new byte[messages[i].MessageSize];
                for (int k = 0; k < messages[i].MessageSize; ++k)
                {
                    bytes[k] = (byte)rand.Next(255);
                }
                messages[i].SetMessage(bytes);
            }
            GameMessage bundle = GameMessage.MessageBundle(messages);
            List<GameMessage> split = GameMessage.SplitBundle(bundle);
            for (int i = 0; i < 10; ++i)
            {
                if (!compareMessage(split[i], messages[i]))
                    Console.WriteLine("message {0} does not match", i);
            }
        }
    }
}
