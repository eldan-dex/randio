using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Randio {
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game {
        GraphicsDeviceManager graphics;
        SpriteBatch backgroundSpriteBatch;
        SpriteBatch foregroundSpriteBatch;
        Map map;
        Player player;

        public Game() {
            graphics = new GraphicsDeviceManager(this);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            graphics.PreferredBackBufferWidth = 1280; //X
            graphics.PreferredBackBufferHeight = 720; //Y
            graphics.ApplyChanges();

            //WFC.ExpandBitmap(new System.Drawing.Bitmap("City.png"), 3, 100, 100, true,  2, -1);
            map = new Map(GraphicsDevice, 12800, 720); //TODO: Generate map size randomly
            player = new Player(GraphicsDevice, 10, 10, true, 5);


            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            backgroundSpriteBatch = new SpriteBatch(GraphicsDevice);
            foregroundSpriteBatch = new SpriteBatch(GraphicsDevice);

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
            KeyboardState state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.Escape))
                Exit();

            if (state.IsKeyDown(Keys.Left))
                player.SetMoveLeft();

            if (state.IsKeyDown(Keys.Right))
                player.SetMoveRight();

            if (state.IsKeyDown(Keys.Up))
                player.SetMoveUp();

            player.Update();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.White);
            backgroundSpriteBatch.Begin();
            map.DrawVisibleTiles(backgroundSpriteBatch, player.X);
            backgroundSpriteBatch.End();

            foregroundSpriteBatch.Begin();
            player.Draw(foregroundSpriteBatch);
            foregroundSpriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
