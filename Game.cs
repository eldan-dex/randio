using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Randio_2 {
    //Main Game class. Contains all the basic logic (Initialize, Update, Draw, most important objects, etc.)
    class Game : Microsoft.Xna.Framework.Game {
        #region Public variables
        public const int WIDTH = 1280;
        public const int HEIGHT = 720;
        public static SpriteFont font;
        
        public static Stats stats;
        public static Dialogue Dialogue;
        public static Loading Loading;

        public static bool DebugEnabled = false;
        public static bool endIntro = false; //hack
        #endregion

        #region Private variables
        private GraphicsDeviceManager graphics;
        private SpriteBatch levelSpriteBatch;
        private SpriteBatch osdSpriteBatch;
        private Map map;
        private Camera camera;
        private KeyboardState keyboardState;

        private string playerName = "";


        //"O" - toggle debug
        private bool oEnabled = false;

        //"P" - play next frame (in debug mode)
        private bool nextFrame = false;
        private bool pEnabled = false;

        //"L" - reset player
        private bool lEnabled = false;

        //"ESC" - end level/game
        private bool escEnabled = false;
        #endregion

        #region Public methods
        public Game() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        #endregion

        #region Protected methods
        //Initializes renderer, camera and creates the first map object (intro screen)
        protected override void Initialize() {
            graphics.PreferredBackBufferWidth = WIDTH; //X
            graphics.PreferredBackBufferHeight = HEIGHT; //Y
            graphics.ApplyChanges();

            camera = new Camera(GraphicsDevice.Viewport);

            playerName = StringHelper.GenerateName(1); //only generate player name once, our player will have the same name during the whole game
            CreateMap(true);

            base.Initialize();
        }

        //Initializes spritebatches and font
        protected override void LoadContent() {
            levelSpriteBatch = new SpriteBatch(GraphicsDevice);
            osdSpriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("font"); //this should be loaded dynamically
        }

        //Main update routine. Updates keyboardStatus and calls update methods of all game objects.
        //Handles switching between "screens" (intro/game/outro)
        protected override void Update(GameTime gameTime) {
            //When a Dialogue is shown, game freezes and we focus only on dialogue action buttons
            if (Dialogue != null)
                ProcessDialogueInputs();

            else
            {
                ProcessInputs();

                //Debugging mode enables step-by-step physics
                if ((DebugEnabled && nextFrame) || !DebugEnabled)
                {
                    map.Update(gameTime, keyboardState);
                    nextFrame = false;
                }

                //Intro screen ended, start game
                if (endIntro)
                {
                    endIntro = false;
                    CreateMap(false);
                }

                //Player reached end of game map, show outro screen
                if (map.ReachedExit)
                {
                    stats = new Stats(map.Player.Stats);
                    CreateMap(true, false);
                    StringHelper.Reset();
                }
            }

            base.Update(gameTime);
        }

        //Main draw routine. Calls draw methods of all game objects
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //Draws background, blocks, entities, items, etc.
            DrawLevel(gameTime);

            //Draws all shown text
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

            map.Draw(gameTime, levelSpriteBatch);

            levelSpriteBatch.End();
        }

        private void DrawOSDs()
        {
            osdSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            //Draw debug info
            if (DebugEnabled)
            {
                osdSpriteBatch.DrawString(font, "DEBUG ENABLED", new Vector2(10, 10), Color.Red);
                osdSpriteBatch.DrawString(font, "Player X: " + map.Player.Position.X + "     Player Y: " + map.Player.Position.Y + "\nDirection: " + (map.Player.Direction == 1 ? "right" : "left") + "\nCurrentTile: " + map.Player.CurrentTile + "\nHP: " + map.Player.HP.ToString() + "/" + map.Player.MaxHP.ToString() + ", Speed: " + map.Player.Speed.ToString() + "\nStrength: " + map.Player.Strength.ToString() + ", Defense: " + map.Player.Defense.ToString(), new Vector2(10, 30), Color.Red);
            }

            //Draw quests
            if (map.Quests != null)
            {
                Vector2 questPosition = (DebugEnabled ? new Vector2(0, 120) : new Vector2(0, 10));
                string status = map.Quests.QuestsStatus();
                GraphicsHelper.FillRectangle(map.Quests.Background, new Color(180, 180, 180));
                osdSpriteBatch.Draw(map.Quests.Background, new Rectangle((int)questPosition.X - 5, (int)questPosition.Y - 5, map.Quests.Background.Width, map.Quests.Background.Height), Color.White * 0.8f);
                osdSpriteBatch.DrawString(font, status, questPosition, ColorHelper.ChangeColorBrightness(ColorHelper.InvertColor(map.GetTileByIndex(map.Player.CurrentTile).Palette[0]), -0.6f));
            }

            //Draw button info, current tile and current item
            var tu = map.GetTileByIndex(map.Player.CurrentTile);
            Color invColor = ColorHelper.BlackWhiteContrasting(tu.Palette[1]);
            osdSpriteBatch.DrawString(font, "Current tile: " + map.Player.CurrentTile, new Vector2(100, 660), invColor, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
            osdSpriteBatch.DrawString(font, "Item: " + ((map.Player.HeldItem == null) ? "none" : map.Player.HeldItem.Properties.Name), new Vector2(930, 660), invColor, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
            osdSpriteBatch.DrawString(font, "  WSAD - move   J - attack   K - pick/put item   L - reset   L-SHIFT - slowmove   ESC - end level", new Vector2(10, 700), invColor, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
            osdSpriteBatch.End();
        }

        //Initializes a new map (intro/game/outro)
        private void CreateMap(bool createScreenInstead, bool isIntro = true) {
            //Screen = intro/outro
            if (createScreenInstead)
                map = new Screen(GraphicsDevice, camera, WIDTH, HEIGHT, playerName, isIntro);

            //Map = game itself
            else
                map = new Map(GraphicsDevice, camera, WIDTH * 10, HEIGHT, playerName);
            
        }

        //Processes all keyboard inputs
        private void ProcessInputs() {
            keyboardState = Keyboard.GetState();

            //ESC
            //Intro - skip intro
            //Game - exit game, show outro
            //Outro - terminate program
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                if (escEnabled)
                {
                    var palette = map.GetTileByIndex(map.Player.CurrentTile).Palette;
                    bool startDialogue = map.GetType() == typeof(Screen) && ((Screen)map).IsIntro;
                    Dialogue = new Dialogue(GraphicsDevice, map, (startDialogue ? "Start your adventure now?" : "Do you want to quit the game?"), DialogueActionEscape);
                    escEnabled = false;
                }
            }
            else
                escEnabled = true;

            //L - reset player to the beginning of the current tile
            if (keyboardState.IsKeyDown(Keys.L))
            {
                if (lEnabled) {
                    lEnabled = false;
                    map.ResetPlayer();
                }
            }
            else
                lEnabled = true;

            //O - enable/disable debug mode
            if (keyboardState.IsKeyDown(Keys.O)) {
                if (oEnabled)
                {
                    oEnabled = false;
                    DebugEnabled = !DebugEnabled;
                }
            }
            else
                oEnabled = true;
            
            //Only available in debug mode
            //P - enables game updates (physics) for one cycle
            //Arrow keys - move the camera manually
            if (DebugEnabled) {
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

        //Processes keyboard inputs when a dialogue is shown
        private void ProcessDialogueInputs()
        {
            keyboardState = Keyboard.GetState();

            //J - accept
            if (keyboardState.IsKeyDown(Keys.J))
            {
                Dialogue.Action?.Invoke();
                Dialogue = null;
            }

            //K, ESC - decline
            if (keyboardState.IsKeyDown(Keys.K) || (keyboardState.IsKeyDown(Keys.Escape) && escEnabled))
            {
                Dialogue.CancelAction?.Invoke();
                Dialogue = null;
                escEnabled = false;
            }

            if (!keyboardState.IsKeyDown(Keys.Escape))
                escEnabled = true;
        }

        //When ESC is pressed and the "skip/quit" dialogue is confirmed, performs the desired action
        private void DialogueActionEscape()
        {
            if (map.GetType() == typeof(Screen))
            {
                //Intro - skip to game
                if (((Screen)map).IsIntro)
                {
                    endIntro = true;
                    Loading = new Loading(GraphicsDevice, map, "Game is now loading...");
                }
                else
                    //Outro - terminate program
                    Exit();
            }

            else
                //Game - show outro screen
                map.ReachedExit = true;
        }
        #endregion
    }
}