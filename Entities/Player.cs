using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Randio_2 {
    //Player class, inherits Entity. Contains Player controls, properties and interactions
    class Player : Entity {

        #region Public varaibles
        public Vector2 Origin { get; private set; }
        public int SafeMargin { get; private set; } //how close to the border can player go before the camera starts moving;
        public const int PlayerWidth = 37; //3px smaller than blocks to better fit into holes and for better maneuvering
        public const int PlayerHeight = 37;
        public Stats Stats;
        #endregion

        #region Private variables
        // Input configuration
        private const Keys jumpButton = Keys.W;
        private const Keys leftButton = Keys.A;
        private const Keys rightButton = Keys.D;
        private const Keys slowButton = Keys.LeftShift;
        private const Keys actionButtonA = Keys.J;
        private const Keys actionButtonB = Keys.K;

        private bool AkeyDown;
        private bool BkeyDown;
        #endregion

        #region Public methods
        //Default ctor
        public Player(GraphicsDevice graphicsDevice, Map map, Vector2 position, string name) : base(map, position, 0, PlayerWidth, PlayerHeight) {        
            CurrentTile = 0;
            Origin = position;
            Texture = CreateTexture(graphicsDevice, Width, Height);
            SafeMargin = 280;

            Name = name.ToUpper();

            InitStats();
            //First setting the outline color must be done in child classes
            UpdateOutlineColor();
        }

        //Updates the player first based on keyboard inputs, then as a generic entity
        public void Update(GameTime gameTime, KeyboardState keyboardState) {
            if (!Alive)
                map.ResetPlayer();

            GetInput(keyboardState);
            Update(gameTime); //Entity.Update()
        }
        #endregion

        #region Private methods
        //Creates player texture (blue with an arrow in the middle)
        private Texture2D CreateTexture(GraphicsDevice graphicsDevice, int width, int height) {
            var texture = new Texture2D(graphicsDevice, width, height);

            GraphicsHelper.FillRectangle(texture, Color.Blue);
            GraphicsHelper.DrawArrow(1, texture, Color.Black);
            outlineWidth = 3;

            return texture;
        }

        //Initializes player stats (those different from generic Entity stats)
        private void InitStats()
        {
            //Player is a little faster and jumps higher and faster than an average entity
            MaxMoveSpeed = 1750.0f;
            MaxJumpTime = 0.35f;
            JumpLaunchVelocity = -3500.0f;

            //Only HP is higher than an average entity
            DefaultHP = 10;
            DefaultStrength = 1;
            DefaultDefense = 0;

            IsPlayer = true;
            Stats = new Stats();
        }

        //Parses keyboard input
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

            //Player is attacking
            if (keyboardState.IsKeyDown(actionButtonA))
            {
                if (!AkeyDown)
                {
                    if (CanAttack)
                    {
                        Attack();
                        //Prevent key-spamming
                        CanAttack = false;
                        map.EntityEvents.AddEvent(new Event<Entity>(100, delegate (Entity e) { e.CanAttack = true; }, this));
                    }
                    AkeyDown = true;
                }
            }
            else
                AkeyDown = false;

            //Item interaction
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

        //Attack action
        private void Attack()
        {
            Entity nearest = GetFirstEntityInSight(Direction, Range);
            if (nearest != null)
                nearest.TakeDamage(this, Strength);
        }

        //Item pickup/drop action
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