using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Net;
using ONet;

namespace SpaceInvaders
{
    public class MenuScreen : IGameScreen
    {
        KeyboardState lastState;
        MouseState lastMouseState;
        String ip = "";
        Texture2D serverButton;
        Texture2D connectButton;
        Texture2D cursorImg;
        Rectangle serverRect;
        Rectangle connectRect;
        MessageBox serverBox;
        MessageBox clientBox;
        MessageBox otherStuff;
        int mouseX, mouseY;
        int ticks;

        public MenuScreen()
        {
            lastState = new KeyboardState();
            lastMouseState = new MouseState();
            Game1.KeyboardBuffer.Enabled = true;
            serverRect = new Rectangle(340, 500, 120, 50);
            connectRect = new Rectangle(340, 400, 120, 50);
            otherStuff = new MessageBox(8, 400, 12);
        }
        public void Update(GameTime gameTime)
        {
            Game1.KeyboardBuffer.TranslateMessage = true;
            ip += Game1.KeyboardBuffer.GetText();
            ticks += gameTime.ElapsedGameTime.Milliseconds;
            if (ticks > 1000)
            {
                ticks -= 1000;
                if (clientBox != null)
                {
                    clientBox.AddMessage(Game1.Client.Attempts + " attempts" );
                }
            }
        }        

        public void InjectInput(KeyboardState keyboardState, MouseState mouseState)
        {
            mouseX = mouseState.X;
            mouseY = mouseState.Y;
            if (mouseState.LeftButton == ButtonState.Released && lastMouseState.LeftButton == ButtonState.Pressed)
            {
                if (Utils.Intersects(mouseX, mouseY, serverRect))
                {
                    Game1.Server = new ONet.GameServer();
                    Game1.Server.OnClientConnect = new GameServer.Callback(ServerClientConnect);
                    Game1.Server.OnClientMessage = new GameServer.Callback(ServerMessage);
                    Game1.Server.OnClientDisconnect = new GameServer.Callback(ServerClientDisconnect);
                    serverBox = new MessageBox(8, 0, 0);
                    serverBox.AddMessage("Started Server");
                }
                else if (Utils.Intersects(mouseX, mouseY, connectRect))
                {
                    IPAddress addr;
                    if (!IPAddress.TryParse(ip, out addr))
                    {
                        otherStuff.AddMessage("Incorrect IP address");
                    }
                    else
                    {
                        Game1.Client = new ONet.Client(new IPEndPoint(IPAddress.Loopback, 8024));  
                        Game1.Client.OnConnect = new Client.Callback(ClientConnect);
                        Game1.Client.TryConnect();
                        clientBox = new MessageBox(8, 0, 100);
                        clientBox.AddMessage("Started Client, connecting to : " + ip);
                    }
                    ip = "";
                }
            }

            lastState = keyboardState;
            lastMouseState = mouseState;
        }

        public void Draw(GameTime gameTime, GraphicsDevice graphicsDevice)
        {
            graphicsDevice.Clear(Color.Black);
            Game1.SpriteBatch.Begin();
            Game1.SpriteBatch.Draw(serverButton, serverRect, Color.White);
            Game1.SpriteBatch.Draw(connectButton, connectRect, Color.White);
            Game1.SpriteBatch.Draw(cursorImg, new Rectangle(mouseX, mouseY, 18, 16), Color.White); 
            Game1.SpriteBatch.DrawString(Game1.Font, ip, new Vector2(400, 0), Color.White);
            if (clientBox != null)
            {
                clientBox.Draw();
            }
            if (serverBox != null)
            {
                serverBox.Draw();
            }
            otherStuff.Draw();
            Game1.SpriteBatch.End();
        }

        public void Remove()
        {
            
        }

        public void LoadContent(ContentManager contentManager)
        {
            serverButton = contentManager.Load<Texture2D>("server");
            connectButton = contentManager.Load<Texture2D>("connect");
            cursorImg = contentManager.Load<Texture2D>("cursor");
        }

        public void ServerMessage(int clientNumber, GameMessage message)
        {

        }
        public void ServerClientConnect(int clientNumber, GameMessage message)
        {
            serverBox.AddMessage("Client connected, client number: " + clientNumber);
        }
        public void ServerClientDisconnect(int clientNumber, GameMessage message)
        {

        }
        public void ClientConnect(GameMessage message)
        {
            clientBox.AddMessage("Connected");
        }
    }
}
