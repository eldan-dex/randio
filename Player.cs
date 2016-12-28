using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Randio_2 {
    class Player : Entity {

        #region Public varaibles
        public Vector2 Origin { get; private set; }
        public int SafeMargin { get; private set; } //how close to the border can player go before the camera starts moving;
        public const int Width = 32; //32 small, 48big
        public const int Height = 32; //32 small, 48big
        #endregion

        #region Private variables
        // Input configuration
        private const Keys jumpButton = Keys.W; //keys will be assigned randomly
        private const Keys leftButton = Keys.A;
        private const Keys rightButton = Keys.D;
        private const Keys crouchButton = Keys.S;
        private const Keys actionButtonA = Keys.J;
        private const Keys actionButtonB = Keys.K;

        private bool AkeyDown;
        private bool BkeyDown;
        #endregion

        #region Public methods
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
        #endregion

        #region Private methods
        private Texture2D CreateTexture(GraphicsDevice graphicsDevice, int width, int height) {
            var texture = new Texture2D(graphicsDevice, width, height);

            //Temporary placeholder code, will call texture generation here
            GraphicsHelper.DrawRectangle(texture, Color.Blue);
            GraphicsHelper.DrawArrow(1, texture, Color.Black);
            GraphicsHelper.OutlineRectangle(texture, Color.Green, 2);

            return texture;
        }

        private void InitStats()
        {
            //atm player is a little faster and jumps higher and faster than an average entity
            //TODO: are these good default values for player?
            MaxMoveSpeed = 1750.0f;
            MaxJumpTime = 0.35f; //0.35f small
            JumpLaunchVelocity = -3500.0f; //-3500 small

            //atm only HP is higher than an average entity
            DefaultHP = 10;
            DefaultStrength = 1;
            DefaultDefense = 0;

            Name = StringHelper.GenerateName().ToUpper();
            IsPlayer = true;
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

            if (keyboardState.IsKeyDown(actionButtonA))
            {
                if (!AkeyDown)
                {
                    Attack();
                    AkeyDown = true;
                }
            }
            else
                AkeyDown = false;

            if (keyboardState.IsKeyDown(actionButtonB))
            {
                if (!BkeyDown)
                {
                    PickDropItem();
                    BkeyDown = true;
                }
            }
            else
                BkeyDown = false;
        }

        private void Attack()
        {
            Entity nearest = GetFirstEntityInSight(Direction, Range);
            if (nearest != null)
                nearest.TakeDamage(this, Strength);
        }

        private void PickDropItem()
        {
            Item nearest = GetFirstItemInSight(Direction, Range);

            //Take item if none is currently being held
            if (HeldItem == null)
            {
                if (nearest != null)
                    HeldItem = nearest.PickUp(this);
            }
            //Drop held item or swap items if available
            else
            {
                if (nearest == null)
                {
                    HeldItem.PutDown(Direction);
                    HeldItem = null;
                }
                else
                {
                    Item tmp = HeldItem;
                    HeldItem = nearest.PickUp(this);
                    tmp.PutDown(Direction);
                }
            }

            ApplyItemProperties();
        }
        #endregion
    }
}
