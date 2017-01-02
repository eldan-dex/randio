using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Randio_2 {
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    class Game : Microsoft.Xna.Framework.Game {
        #region Public variables
        public const int WIDTH = 1280; //1280 small, 1280 medium, 1296 big
        public const int HEIGHT = 720; //704 small, 1280 medium, 720 big
        public static SpriteFont font;
        #endregion

        #region Private variables
        private GraphicsDeviceManager graphics;
        private SpriteBatch levelSpriteBatch;
        private SpriteBatch osdSpriteBatch;
        private SpriteFont debugFont;
        private SpriteBatch testSB;
        private Texture2D testBG;

        private Map map;
        private Camera camera;
        private KeyboardState keyboardState;
        
        //"O" - toggle debug
        public static bool debugEnabled = false; //public because... eh.
        private bool oEnabled = false;

        //"P" - play next frame (in debug mode)
        private bool nextFrame = false;
        private bool pEnabled = false; //for the P button
        #endregion

        #region Public methods
        public Game() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        #endregion

        #region Protected methods
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            graphics.PreferredBackBufferWidth = WIDTH; //X
            graphics.PreferredBackBufferHeight = HEIGHT; //Y
            graphics.ApplyChanges();

            camera = new Camera(GraphicsDevice.Viewport);
            CreateMap();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            levelSpriteBatch = new SpriteBatch(GraphicsDevice);
            osdSpriteBatch = new SpriteBatch(GraphicsDevice);
            debugFont = Content.Load<SpriteFont>("debug"); //there will be some osd in the final game, but the font will not be stored externally
            font = Content.Load<SpriteFont>("font"); //this will be loaded dynamically
            testSB = new SpriteBatch(GraphicsDevice); //remove this
            //testBG = new ShapesBG(GraphicsDevice, testSB, WIDTH, HEIGHT).Texture; //remove this
            //testBG = new LSystemBG(GraphicsDevice, testSB, WIDTH, HEIGHT).Texture; //remove this
            //testBG = new CityBG(GraphicsDevice, testSB, WIDTH, HEIGHT).Texture; //remove this
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent() {
            
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            ProcessInputs();

            //For step-by-step physics
            if ((debugEnabled && nextFrame) || !debugEnabled) {
                map.Update(gameTime, keyboardState);
                nextFrame = false;
            }

            if (map.ReachedExit)
                Exit(); //temporary. Normally this would reinitialize everything and create a new level
                
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            DrawLevel(gameTime);
            DrawOSDs();

            base.Draw(gameTime);
        }
        #endregion

        #region Private methods
        private void DrawLevel(GameTime gameTime)
        {
            var viewMatrix = camera.GetViewMatrix();
            levelSpriteBatch.Begin(transformMatrix: viewMatrix);
            //levelSpriteBatch.Draw(testBG, new Rectangle(0, 0, WIDTH, HEIGHT), Color.White); //uncomment this for background testing
            map.Draw(gameTime, levelSpriteBatch); //comment this out for background testing
            levelSpriteBatch.End();
        }

        private void DrawOSDs()
        {
            osdSpriteBatch.Begin();
            if (debugEnabled)
            {
                osdSpriteBatch.DrawString(debugFont, "DEBUG ENABLED", new Vector2(10, 10), Color.Red);
                osdSpriteBatch.DrawString(debugFont, "Player X: " + map.Player.Position.X + "     Player Y: " + map.Player.Position.Y + "\nDirection: " + (map.Player.Direction == 1 ? "right" : "left") + "\nCurrentTile: " + map.Player.CurrentTile + "\nHP: " + map.Player.HP.ToString() + "/" + map.Player.MaxHP.ToString() + ", Speed: " + map.Player.Speed.ToString() + "\nStrength: " + map.Player.Strength.ToString() + ", Defense: " + map.Player.Defense.ToString(), new Vector2(10, 30), Color.Red);
            }

            if (map.quests != null)
            {
                Vector2 questPosition = (debugEnabled ? new Vector2(10, 120) : new Vector2(10, 10));
                osdSpriteBatch.DrawString(font, map.quests.QuestsStatus(), questPosition, Color.DarkGreen);
            }
            osdSpriteBatch.End();
        }

        private void CreateMap() {
            map = new Map(GraphicsDevice, camera, WIDTH*10, HEIGHT); //parameters
        }

        private void ProcessInputs() {
            keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            if (keyboardState.IsKeyDown(Keys.L)) //debug: remove this in final game
                map.ResetPlayer();

            //"O" key enables/disables debugging features
            if (keyboardState.IsKeyDown(Keys.O)) {
                if (oEnabled)
                {
                    oEnabled = false;
                    debugEnabled = !debugEnabled;
                }
            }
            else
                oEnabled = true;
            
            //Enter the land of debugging
            if (debugEnabled) {
                //"P" key for step-by-step physics
                if (keyboardState.IsKeyDown(Keys.P) && pEnabled) {
                    pEnabled = false;
                    nextFrame = true;
                }
                else
                    pEnabled = true;

                //Arrow keys for manual camera controls
                if (keyboardState.IsKeyDown(Keys.Right))
                    camera.Position += new Vector2(5f, 0);

                else if (keyboardState.IsKeyDown(Keys.Left))
                    camera.Position -= new Vector2(5f, 0);
            }


            //Additional global keyboard inputs - global menu key, etc

        }
        #endregion
    }
}