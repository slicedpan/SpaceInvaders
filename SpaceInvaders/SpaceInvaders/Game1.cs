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
        public static SpriteBatch SpriteBatch;        
        
        Dictionary<int, ClientInfo> clients;
        public static KeyboardBuffer KeyboardBuffer;
        public static int width = 1024;
        public static int height = 768;
        ScreenManager screenManager;
        public static SpriteFont Font;
        public static SpriteFont mbFont;
        public static UI UITex;
        public static double updatesPerSecond = 30.0d;
        public static Texture2D circleTex;
        public static BoundingSphere screenExtent;
        public static Random rand;

        public Game1()
        {
            rand = new Random();
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = width;
            graphics.PreferredBackBufferHeight = height;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            clients = new Dictionary<int, ClientInfo>();
            KeyboardBuffer = new KeyboardBuffer(this.Window.Handle);
            KeyboardBuffer.TranslateMessage = true;
            float screenRadius = (float)Math.Sqrt(Math.Pow(width / 2.0d, 2.0d) + Math.Pow(height / 2.0d, 2.0d));
            screenExtent = new BoundingSphere(new Vector3((float)width / 2.0f, (float)height / 2.0f, 0.0f), screenRadius);
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
            var c = new ClientState();
            var s = new ServerState();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            UITex = new UI();
            UITex.LoadContent(Content);
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            screenManager = new ScreenManager(GraphicsDevice, Content);            
            screenManager.SetScreen(new MenuScreen());
            Font = Content.Load<SpriteFont>("Segoe");
            mbFont = Content.Load<SpriteFont>("text");
            circleTex = Content.Load<Texture2D>("circle");

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
                ClientState.currentInstance.Client.Disconnect("Player quit");
                this.Exit();
            }

            // TODO: Add your update logic here
            screenManager.Update(gameTime);

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
