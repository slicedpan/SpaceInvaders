using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using ONet;
using System.Net;

namespace SpaceInvaders
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public static GameServer Server;
        public static Client Client;
        Dictionary<int, ClientInfo> clients;
        KeyboardState lastState;
        public static KeyboardBuffer KeyboardBuffer;
        public static int width = 800;
        public static int height = 600;
        ScreenManager screenManager;
        public static SpriteFont Font;
        string curMessage = "";

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = width;
            graphics.PreferredBackBufferHeight = height;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            clients = new Dictionary<int, ClientInfo>();
            lastState = new KeyboardState();
            KeyboardBuffer = new KeyboardBuffer(this.Window.Handle);
            KeyboardBuffer.Enabled = true;
            KeyboardBuffer.TranslateMessage = true;            
        }

        public void ClientConnects(int clientNumber, GameMessage message)
        {
            clients.Add(clientNumber, new ClientInfo(clientNumber, String.Format("client: {0}, addr: {1}", clientNumber, Server.Connections[clientNumber].Socket.RemoteEndPoint.ToString())));            
        }

        public void ClientDisconnects(int clientNumber, GameMessage message)
        {
            clients.Remove(clientNumber);
        }

        public void ClientMessage(int clientNumber, GameMessage message)
        {
            if (message.DataType == MessageType.SetName)
            {

            }
        }   

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            screenManager = new ScreenManager(GraphicsDevice, Content);
            screenManager.SetScreen(new TestScreen());
            Font = Content.Load<SpriteFont>("Segoe");
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            KeyboardState keyState;
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }
            if (keyState.IsKeyDown(Keys.P) && !lastState.IsKeyDown(Keys.P))
            {
                if (Server == null)
                {
                    Server = new GameServer();
                    Server.OnClientConnect = new GameServer.Callback(ClientConnects);
                    Server.OnClientDisconnect = new GameServer.Callback(ClientDisconnects);
                    Server.OnClientMessage = new GameServer.Callback(ClientMessage);
                }
            }
            if (keyState.IsKeyDown(Keys.C) && !lastState.IsKeyDown(Keys.C))
            {
                if (Client == null)
                {
                    Client = new Client(new IPEndPoint(IPAddress.Parse(curMessage), 8024));
                }
            }
            // TODO: Add your update logic here
            curMessage += KeyboardBuffer.GetText();
            lastState = keyState;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            screenManager.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}
