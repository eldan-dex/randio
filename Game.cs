using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Randio_2 {
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game {
        GraphicsDeviceManager graphics;
        SpriteBatch levelSpriteBatch;

        private Map map;
        private Camera camera;

        private KeyboardState keyboardState;

        public const int WIDTH = 1280;
        public const int HEIGHT = 736;

        private bool nextFrame = false;
        private bool pEnabled = false;

        public Game() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

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

        private void CreateMap() {
            map = new Map(GraphicsDevice, 12800, 736); //parameters
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            levelSpriteBatch = new SpriteBatch(GraphicsDevice);

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
            if (nextFrame) {
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

            var viewMatrix = camera.GetViewMatrix();

            levelSpriteBatch.Begin(transformMatrix: viewMatrix);
            map.Draw(gameTime, levelSpriteBatch);
            levelSpriteBatch.End();

            base.Draw(gameTime);
        }

        private void ProcessInputs() {
            keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            //Step-by-step physics for debugging
            if (keyboardState.IsKeyDown(Keys.P) && pEnabled) {
                pEnabled = false;
                nextFrame = true;
            }
            else
                pEnabled = true;
            
                
            //Additional global keyboard inputs - global menu key, etc

        }
    }
}
