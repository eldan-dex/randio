using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Randio_2 {
    class Player : Entity {

        #region Public varaibles
        public Vector2 Origin { get; private set; }
        public int SafeMargin { get; private set; } //how close to the border can player go before the camera starts moving;
        public const int PlayerWidth = 37; //32 small, 48big
        public const int PlayerHeight = 37; //32 small, 48big
        #endregion

        #region Private variables
        // Input configuration
        private const Keys jumpButton = Keys.W; //keys will be assigned randomly
        private const Keys leftButton = Keys.A;
        private const Keys rightButton = Keys.D;
        private const Keys slowButton = Keys.LeftShift;
        private const Keys actionButtonA = Keys.J;
        private const Keys actionButtonB = Keys.K;

        private bool AkeyDown;
        private bool BkeyDown;
        #endregion

        #region Public methods
        public Player(GraphicsDevice graphicsDevice, Map map, Vector2 position, string name) : base(map, position, 0, PlayerWidth, PlayerHeight) {        
            CurrentTile = 0;
            Origin = position;
            Texture = CreateTexture(graphicsDevice, Width, Height);
            SafeMargin = 240; //temporary

            Name = name;
            InitStats();
            //First setting the outline color must be done in child classes
            UpdateOutlineColor();
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState) {
            if (!Alive)
            {
                map.ResetPlayer(); //todo: will have to do somethign when player dies instead of just resetting
            }

            GetInput(keyboardState);
            Update(gameTime);
        }

        public void Reset() {
            //Respawn player on the tile where he died
            //This might not be desirable in the finished game
            position = Origin + new Vector2(map.GetTileByIndex(CurrentTile).Coords.Left, 0);

            Alive = true;
            HP = MaxHP;
            UpdateOutlineColor();
        }
        #endregion

        #region Private methods
        private Texture2D CreateTexture(GraphicsDevice graphicsDevice, int width, int height) {
            var texture = new Texture2D(graphicsDevice, width, height);

            //Temporary placeholder code, will call texture generation here
            GraphicsHelper.DrawRectangle(texture, Color.Blue);
            GraphicsHelper.DrawArrow(1, texture, Color.Black);
            outlineWidth = 3;

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

            IsPlayer = true;
    }

        private void GetInput(KeyboardState keyboardState) {
            movement = 0.0f;
            var speed = 1.0f;

            if (keyboardState.IsKeyDown(slowButton))
                speed = 0.3f;

            if (keyboardState.IsKeyDown(leftButton))
                movement = -speed;
            else if (keyboardState.IsKeyDown(rightButton))
                movement = speed;

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
                {
                    HeldItem = nearest.PickUp(this);
                    ApplyItemProperties();
                }
            }
            //Drop held item or swap items if available
            else
            {
                if (nearest == null)
                {
                    HeldItem.PutDown(Direction);
                    DisapplyItemProperties();
                    HeldItem = null;
                }
                else
                {
                    Item heldTmp = HeldItem;
                    DisapplyItemProperties(); //disapply the previous item
                    HeldItem = nearest.PickUp(this);
                    heldTmp.PutDown(Direction);
                    ApplyItemProperties(); //apply the new item
                }
            }
        }
        #endregion
    }
}
