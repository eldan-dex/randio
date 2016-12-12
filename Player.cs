using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Randio_2 {
    class Player : Entity {

        //Public variables
        //********************************************************************************//
        public Vector2 Origin { get; private set; }
        public int SafeMargin { get; private set; } //how close to the border can player go before the camera starts moving;
        public const int Width = 32;
        public const int Height = 32;


        //Private variables
        //********************************************************************************//
        // Input configuration
        private const Keys jumpButton = Keys.W; //keys will be assigned randomly
        private const Keys leftButton = Keys.A;
        private const Keys rightButton = Keys.D;
        private const Keys crouchButton = Keys.S;


        //Public methods
        //********************************************************************************//
        public Player(GraphicsDevice graphicsDevice, Map map, Vector2 position) : base(map, position, 0, Width, Height) {        
            CurrentTile = 0;
            Origin = position;
            Texture = CreateTexture(graphicsDevice, Width, Height);
            SafeMargin = 240; //temporary

            InitStats();
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState) {
            GetInput(keyboardState);
            Update(gameTime);
        }

        public void Reset() {
            //Respawn player on the tile where he died
            //This might not be desirable in the finished game

            position = Origin + new Vector2(map.GetTileByIndex(CurrentTile).Coords.Left, 0);
            //reset everything else too
        }


        //Private methods
        //********************************************************************************//
        private Texture2D CreateTexture(GraphicsDevice graphicsDevice, int width, int height) {
            var texture = new Texture2D(graphicsDevice, width, height);

            //Temporary placeholder code, will call texture generation here
            GraphicsHelper.DrawRectangle(texture, Color.Blue);
            GraphicsHelper.OutlineRectangle(texture, Color.Green, 2);

            return texture;
        }

        private void InitStats()
        {
            //atm player is a little faster and jumps higher and faster than an average entity
            //TODO: are these good default values for player?
            MaxMoveSpeed = 1750.0f;
            MaxJumpTime = 0.35f;
            JumpLaunchVelocity = -3500.0f;
    }

        private void GetInput(KeyboardState keyboardState) {
            movement = 0;
            if (Math.Abs(movement) < 0.5f)
                movement = 0.0f;

            if (keyboardState.IsKeyDown(leftButton))
                movement = -1.0f;
            else if (keyboardState.IsKeyDown(rightButton))
                movement = 1.0f;

            isJumping = keyboardState.IsKeyDown(jumpButton);
        }
    }
}
