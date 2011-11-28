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
        int mouseX, mouseY;
        int ticks;

        public MenuScreen()
        {
            lastState = new KeyboardState();
            lastMouseState = new MouseState();
            Game1.KeyboardBuffer.Enabled = true;
            serverRect = new Rectangle(50, 668, 120, 50);
            connectRect = new Rectangle(Game1.width - 170, 668, 120, 50);            
        }
        public void Update(GameTime gameTime)
        {
            Game1.KeyboardBuffer.TranslateMessage = true;
            String newText = Game1.KeyboardBuffer.GetText();
            if (newText.IndexOf('\b') >= 0)
            {
                if (ip.Length > 0)
                    ip = ip.Substring(0, ip.Length - 1);
            }
            else
            {
                ip += newText;
            }
            ticks += gameTime.ElapsedGameTime.Milliseconds;
        }        

        public void InjectInput(KeyboardState keyboardState, MouseState mouseState)
        {
            mouseX = mouseState.X;
            mouseY = mouseState.Y;
            if (mouseState.LeftButton == ButtonState.Released && lastMouseState.LeftButton == ButtonState.Pressed)
            {
                if (Utils.Intersects(mouseX, mouseY, serverRect))
                {
                    serverBox = new MessageBox(8, 0, 0);
                    serverBox.AddMessage("Started Server");
                    ServerState serverState = new ServerState();
                    serverState.msgBox = serverBox;
                    //Game1.Server = new ONet.GameServer();                  
                }
                else if (Utils.Intersects(mouseX, mouseY, connectRect))
                {
                    IPAddress addr;
                    if (Game1.Client != null && Game1.Client.Connected)
                    {
                        Game1.Client.Disconnect("just wanted to");
                        Game1.Client.Dispose();
                    }
                    else
                    {
                        if (ip == "")
                            ip = "127.0.0.1";
                        if (!IPAddress.TryParse(ip, out addr))
                        {
                            clientBox.AddMessage("Incorrect IP address");
                        }
                        else  
                        {
                            //Game1.Client = new ONet.Client(new IPEndPoint(addr, 8024));
                            Game1.Client.OnConnect = new Client.Callback(ClientConnect);
                            Game1.Client.OnError = new Client.ErrorCallback(ClientError);
                            Game1.Client.TryConnect();
                            clientBox = new MessageBox(8, Game1.width - 400, 0);
                            clientBox.AddMessage("Started Client, connecting to : " + ip);
                        }
                        ip = "";
                    }
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
            if (clientBox != null)
            {
                clientBox.Draw();
            }
            if (serverBox != null)
            {
                serverBox.Draw();
            }            
            Game1.SpriteBatch.DrawString(Game1.Font, ip, new Vector2(400, 0), Color.White);
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

        
        public void ClientConnect(GameMessage message)
        {
            ScreenManager.currentInstance.Switch(new ClientScreen());
            clientBox.AddMessage("Connected");
        }
        public void ClientDisconnect(GameMessage message)
        {
            clientBox.AddMessage(String.Format("Disconnected: {0}", message.messageAsString()));            
        }
        public void ClientError(string message)
        {
            clientBox.AddMessage(message);
        }
        public void ServerError(string message)
        {
            serverBox.AddMessage(message);
        }
        public void ClientMessage(GameMessage message)
        {

        }
    }
}
