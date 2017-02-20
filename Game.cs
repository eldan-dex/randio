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
        public static bool endIntro = false;
        public static Stats stats;
        public static Dialogue Dialogue;
        public static Loading Loading;
        #endregion

        #region Private variables
        private GraphicsDeviceManager graphics;
        private SpriteBatch levelSpriteBatch;
        private SpriteBatch osdSpriteBatch;
        private SpriteFont debugFont;
        private SpriteBatch testSB; //testing only
        private Texture2D testBG; //testing only

        private Map map;
        private Camera camera;
        private KeyboardState keyboardState;
        
        
        //"O" - toggle debug
        public static bool debugEnabled = false; //public because... eh.
        private bool oEnabled = false;

        //"P" - play next frame (in debug mode)
        private bool nextFrame = false;
        private bool pEnabled = false; //for the P button

        //"L" - reset player
        private bool lEnabled = false;

        //"ESC" - end level/game
        private bool escEnabled = false;

        private string playerName = "";
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

            playerName = StringHelper.GenerateName(1); //only generate player name once, our player will be the same all the time.
            CreateMap(true);

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
            //testBG = new MountainsBG(GraphicsDevice, testSB, WIDTH, HEIGHT).Texture; //remove this
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
            if (Dialogue != null)
            {
                ProcessDialogueInputs();
            }
            else
            {
                ProcessInputs();

                //For step-by-step physics
                if ((debugEnabled && nextFrame) || !debugEnabled)
                {
                    map.Update(gameTime, keyboardState);
                    nextFrame = false;
                }

                if (map.ReachedExit)
                {
                    stats = new Stats(map.Player.Stats);
                    CreateMap(true, false); //TODO: create end screen
                    StringHelper.Reset();
                }

                if (endIntro) //switch from intro to normal game
                {
                    endIntro = false;
                    Loading = new Loading(GraphicsDevice, map, "Loading", 200);
                    CreateMap(false);
                }
            }

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
            Dialogue?.Draw(gameTime, levelSpriteBatch); //Draw dialogue if enabled

            Loading?.Draw(gameTime, levelSpriteBatch); //Draw loading screen if enabled

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
            osdSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            if (debugEnabled)
            {
                osdSpriteBatch.DrawString(debugFont, "DEBUG ENABLED", new Vector2(10, 10), Color.Red);
                osdSpriteBatch.DrawString(debugFont, "Player X: " + map.Player.Position.X + "     Player Y: " + map.Player.Position.Y + "\nDirection: " + (map.Player.Direction == 1 ? "right" : "left") + "\nCurrentTile: " + map.Player.CurrentTile + "\nHP: " + map.Player.HP.ToString() + "/" + map.Player.MaxHP.ToString() + ", Speed: " + map.Player.Speed.ToString() + "\nStrength: " + map.Player.Strength.ToString() + ", Defense: " + map.Player.Defense.ToString(), new Vector2(10, 30), Color.Red);
            }

            if (map.quests != null)
            {
                Vector2 questPosition = (debugEnabled ? new Vector2(10, 120) : new Vector2(10, 10));
                string status = map.quests.QuestsStatus();
                GraphicsHelper.DrawRectangle(map.quests.Background, new Color(180, 180, 180));
                osdSpriteBatch.Draw(map.quests.Background, new Rectangle((int)questPosition.X - 5, (int)questPosition.Y - 5, map.quests.Background.Width, map.quests.Background.Height), Color.White * 0.8f);
                osdSpriteBatch.DrawString(font, status, questPosition, ColorHelper.ChangeColorBrightness(ColorHelper.InvertColor(map.GetTileByIndex(map.Player.CurrentTile).Palette[0]), -0.6f));
            }

            var tu = map.GetTileByIndex(map.Player.CurrentTile);
            Color invColor = ColorHelper.BlackWhiteContrasting(tu.Palette[1]);
            osdSpriteBatch.DrawString(font, "Current tile: " + map.Player.CurrentTile, new Vector2(100, 660), invColor, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
            osdSpriteBatch.DrawString(font, "Item: " + ((map.Player.HeldItem == null) ? "none" : map.Player.HeldItem.Properties.Name), new Vector2(930, 660), invColor, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
            osdSpriteBatch.DrawString(font, "WSAD - move    J - attack    K - pick/put item    L - reset    L-SHIFT - slow movement   ESC - end level", new Vector2(10, 700), invColor, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
            osdSpriteBatch.End();
        }

        private void CreateMap(bool createScreenInstead, bool isIntro = true) {
            if (createScreenInstead)
            {
                map = new Screen(GraphicsDevice, camera, WIDTH, HEIGHT, playerName, isIntro);
            }

            else
                map = new Map(GraphicsDevice, camera, WIDTH * 10, HEIGHT, playerName);
            
        }

        private void ProcessInputs() {
            keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                if (escEnabled)
                {
                    var palette = map.GetTileByIndex(map.Player.CurrentTile).Palette;
                    Dialogue = new Dialogue(GraphicsDevice, map, "Do you want to quit the game?", DialogueActionEscape);
                    escEnabled = false;
                }
            }
            else
                escEnabled = true;

            if (keyboardState.IsKeyDown(Keys.L))
            {
                if (lEnabled) {
                    lEnabled = false;
                    map.ResetPlayer();
                }
            }
            else
                lEnabled = true;

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
        }

        private void ProcessDialogueInputs()
        {
            keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.J))
            {
                Dialogue.Action?.Invoke();
                Dialogue = null;
            }

            if (keyboardState.IsKeyDown(Keys.K) || (keyboardState.IsKeyDown(Keys.Escape) && escEnabled))
            {
                Dialogue.CancelAction?.Invoke();
                Dialogue = null;
                escEnabled = false;
            }

            if (!keyboardState.IsKeyDown(Keys.Escape))
                escEnabled = true;
        }

        private void DialogueActionEscape()
        {
            if (map.GetType() == typeof(Screen))
            {
                if (((Screen)map).IsIntro)
                    endIntro = true;
                else
                    Exit();
            }

            else
                map.ReachedExit = true;
        }
        #endregion
    }
}