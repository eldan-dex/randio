using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2 {
    class Entity {

        #region Public variables
        public Vector2 Position //this is the GLOBAL position (relative to the entire map)
        {
            get { return position; }
        }
        protected Vector2 position;

        public Vector2 Velocity
        {
            get { return velocity; }
        }
        protected Vector2 velocity;
  
        public Rectangle BoundingRectangle //this is the LOCAL bounding rectangle (relative to the current tile)
        {
            get
            {
                Tile t = map.GetTileByIndex(CurrentTile);
                float newX = Position.X - t.Coords.Left; //this should work
                return new Rectangle((int)newX, (int)Position.Y, Width, Height);
            }
        }

        public bool Alive { get; private set; }
        public Texture2D Texture { get; protected set; }
        public int CurrentTile { get; protected set; } //public for debugging purposes
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public int Direction { get; protected set; } // 1 = right, 0 = left
        public string Name { get; protected set; }
        public Item HeldItem { get; protected set; }
        public int Range { get; protected set; } = 16; //How far can this entity's interaction (attacks, etc) reach

        //Combat stats - defaults
        public int DefaultHP { get; protected set; } = 10;
        public int MaxHP { get; protected set; }
        public float DefaultStrength { get; protected set; } = 1;
        public float DefaultDefense { get; protected set; } = 0;
        public float DefaultSpeed { get; protected set; } = 1;

        //Combat stats - final (accounting for all modifiers)
        public int HP { get; protected set; }
        public float Strength { get; protected set; }
        public float Defense { get; protected set; }
        public float Speed { get; protected set; }

        public Color OverwriteColor = Color.White; //used for effects - damage, etc.
        public Color OutlineColor = Color.Green; //HP indicator

        public bool IsPlayer = false;
        #endregion

        #region Private/Protected variables
        //Instance variables
        protected Map map;


        //Will be generated, not hardcoded
        //These values are entity defaults, every entity should adjust them as needed
        //TODO: check whether these values are suitable to be entity defaults

        // Constants for controling horizontal movement
        protected float MoveAcceleration = 13000.0f;
        protected float MaxMoveSpeed = 1500.0f;
        protected float GroundDragFactor = 0.48f;
        protected float AirDragFactor = 0.58f;

        // Constants for controlling vertical movement
        protected float MaxJumpTime = 0.25f;
        protected float JumpLaunchVelocity = -3000.0f;
        protected float GravityAcceleration = 3400.0f;
        protected float MaxFallSpeed = 550.0f;
        protected float JumpControlPower = 0.14f;

        //Current state
        protected float movement;
        protected bool isJumping;
        private float jumpTime;
        private bool isOnGround;  
        private bool wasJumping;
        private float oldBottom;

        //Outline (HP indicator) width
        protected int outlineWidth = 2;
        #endregion

        #region Public methods
        public Entity(Map map, Vector2 position, int currentTile, int width, int height) {
            this.map = map;
            this.position = position;
            CurrentTile = currentTile;
            Width = width;
            Height = height;
            velocity = Vector2.Zero;
            Alive = true;

            //Set combat properties to defaults
            ResetProperties();

            //Start with full HP
            HP = DefaultHP;
            MaxHP = DefaultHP;
        }

        public void Update(GameTime gameTime) {
            if (isOnGround) {
                if (Math.Abs(Velocity.X) - 0.02f > 0) {
                    //player is moving [GFX]
                }
                else {
                    //player is still [GFX]
                }
            }

            ApplyPhysics(gameTime);
            CheckTile();

            //Keep easily accessible direction info. If player is still, remember last direction
            if (Velocity.X > 0)
                Direction = 1;
            else if (Velocity.X < 0)
                Direction = -1;

            //reset movement
            movement = 0.0f;
            isJumping = false;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
            SpriteEffects effect = SpriteEffects.None;
            if (Direction < 0)
                effect = SpriteEffects.FlipHorizontally;

            Vector2 namePos = new Vector2(Position.X - (Name.Length / 2) * 4, Position.Y - 22);

            spriteBatch.DrawString(Game.font, Name, namePos, Color.Red);
            spriteBatch.Draw(Texture, position, null, OverwriteColor, 0.0f, Vector2.Zero, 1.0f, effect, 0.0f);
        }

        public void TakeDamage(Entity source, float damage)
        {
            var realDamage = (int)(damage - Defense);
            if (realDamage > 0)
            {
                HP -= realDamage;

                //when damaged, entity flashes red for 200ms
                OverwriteColor = Color.Red;
                map.entityEvents.AddEvent(new Event<Entity>(200, delegate (Entity e) { e.OverwriteColor = Color.White; }, this));
                UpdateOutlineColor();
            }
            else
            {
                //if attack was deflected, entity flashes gray for 200ms
                OverwriteColor = Color.DarkGray;
                map.entityEvents.AddEvent(new Event<Entity>(200, delegate (Entity e) { e.OverwriteColor = Color.White; }, this));
            }

            if (HP <= 0)
            {
                Alive = false; //now OutOfMap check will recognise this NPC as dead and will remove it.
            }
        }

        public void UpdateOutlineColor()
        {
            int difference = MaxHP - HP;
            float upperPerc = (float)difference / MaxHP;
            float lowerPerc = (float)1 - upperPerc;

            int newR = (int)(255 * upperPerc);
            int newG = (int)(255 * lowerPerc);

            OutlineColor = new Color(newR, newG, 0);
            GraphicsHelper.OutlineRectangle(Texture, OutlineColor, outlineWidth);
        }

        public List<Entity> GetEntitiesInSight(int xRange, int yRange)
        {
            List<Entity> inSight = new List<Entity>();
            List<Entity> allEntites = map.GetAllEntites();

            foreach (Entity e in allEntites)
            {
                if (e != this && Math.Abs(e.Position.X - Position.X) <= xRange && Math.Abs(e.Position.Y - Position.Y) <= yRange)
                {
                    inSight.Add(e);
                }
            }

            return inSight;
        }

        public Entity GetFirstEntityInSight(int direction, int range)
        {
            Entity found = null;
            var entities = GetEntitiesInSight(range * 3, range * 3);
            List<Entity> foundEntities = new List<Entity>();

            if (direction == 1) //right
            {
                int closest = int.MaxValue;
                foreach (Entity e in entities)
                {
                    if (e.Position.X >= Position.X && Math.Abs(e.Position.Y - Position.Y) <= range * 3) //range/2? really?
                    {
                        int distance = (int)(Math.Abs(e.Position.X - position.X) + Math.Abs(e.Position.Y - position.Y));
                        if (distance < closest)
                        {
                            closest = distance;
                            found = e;
                        }
                    }
                }
            }
            else //left
            {
                int closest = int.MaxValue;
                foreach (Entity e in entities)
                {
                    if (e.Position.X <= Position.X && Math.Abs(e.Position.Y - Position.Y) <= range * 3)
                    {
                        int distance = (int)(Math.Abs(e.Position.X - position.X) + Math.Abs(e.Position.Y - position.Y));
                        if (distance < closest) //aaa, so much duplicity
                        {
                            closest = distance;
                            found = e;
                        }
                    }
                }
            }

            return found;
        }

        public List<Item> GetItemsInSight(int xRange, int yRange)
        {
            List<Item> inSight = new List<Item>();
            List<Item> allItems = map.GetAllItems();

            foreach (Item i in allItems)
            {
                if (Math.Abs(i.Position.X - Position.X) <= xRange && Math.Abs(i.Position.Y - Position.Y) <= yRange)
                {
                    inSight.Add(i);
                }
            }

            return inSight;
        }

        public Item GetFirstItemInSight(int direction, int range)
        {
            Item found = null;
            var items = map.GetAllItems();
            List<Item> foundItems = new List<Item>();

            if (direction == 1) //right
            {
                int closest = int.MaxValue;
                foreach (Item i in items)
                {
                    if (i.Position.X >= Position.X && Math.Abs(i.Position.X - Position.X) + Math.Abs(i.Position.Y - Position.Y) <= range * 3) //range/2? really?
                    {
                        int distance = (int)(Math.Abs(i.Position.X - position.X) + Math.Abs(i.Position.Y - position.Y));
                        if (distance < closest)
                        {
                            closest = distance;
                            found = i;
                        }
                    }
                }
            }
            else //left
            {
                int closest = int.MaxValue;
                foreach (Item i in items)
                {
                    if (i.Position.X <= Position.X && Math.Abs(i.Position.X - Position.X) + Math.Abs(i.Position.Y - Position.Y) <= range * 3)
                    {
                        int distance = (int)(Math.Abs(i.Position.X - position.X) + Math.Abs(i.Position.Y - position.Y));
                        if (distance < closest) //aaa, so much duplicity. Now squared!
                        {
                            closest = distance;
                            found = i;
                        }
                    }
                }
            }

            return found;
        }

        public void FillHP()
        {
            HP = MaxHP;
        }
        #endregion

        #region Private methods
        #region Physics (ApplyPhysics, CheckJump, TerrainCollisionsXY, EntityCollisionsXY)

        //Move the entity according to physics
        private void ApplyPhysics(GameTime gameTime) {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 lastPosition = position;

            velocity.X += movement * MoveAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);
            velocity.Y = CheckJump(velocity.Y, gameTime);

            if (isOnGround)
                velocity.X *= GroundDragFactor;
            else
                velocity.X *= AirDragFactor;

            var actualMaxMoveSpeed = MaxMoveSpeed * Speed; //to enable speed-enhancing items

            velocity.X = MathHelper.Clamp(velocity.X, -actualMaxMoveSpeed, actualMaxMoveSpeed);

            //Move along the X axis
            position.X += velocity.X * elapsed;
            position.X = (float)Math.Round(position.X);

            //Limit the player to stay on the map horizontally
            if (position.X < 0)
                position.X = 0;
            else if (position.X+Width > map.Width)
                position.X = map.Width-Width;

            //X axis collisions
            TerrainCollisionsXY(true);
            EntityCollisionsXY(true);

            //Move along the Y axis
            position.Y += velocity.Y * elapsed;
            position.Y = (float)Math.Round(position.Y);

            //Y axis collisions
            TerrainCollisionsXY(false);
            EntityCollisionsXY(false);

            if (position.X == lastPosition.X)
                velocity.X = 0;

            if (position.Y == lastPosition.Y)
                velocity.Y = 0;
        }

        //If player is jumping, set the velocity accordingly
        private float CheckJump(float velocity, GameTime gameTime) {
            if (isJumping) {
                if ((!wasJumping && isOnGround) || jumpTime > 0.0f) {
                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    //player is jumping [GFX]
                }

                if (jumpTime > 0.0f && jumpTime < MaxJumpTime)
                    velocity = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));

                else
                    jumpTime = 0.0f;
            }
            else
                jumpTime = 0.0f;

            wasJumping = isJumping;

            return velocity;
        }

        //Check for collisions on one axis (X or Y) at a time and resolve them
        private void TerrainCollisionsXY(bool doCollisionX, Tile nextTile = null) {
            Rectangle bounds = BoundingRectangle;
            Tile tile = map.GetTileForX((int)position.X);

            if (nextTile != null)
                tile = nextTile;

            if (tile == null)
                return;

            Vector2 entityTilePos = map.GlobalToTileCoordinates(position, tile.Index); //TODO: this might cause a problem with next-tile collision checking -> translate this too, or rewrite checking to use global positioning?

            if (IsPlayer)
                IsPlayer = IsPlayer;

            //add a condition when player is on tile edge - check next/prev tiles edge line

            int wblocks = tile.Coords.Width / Block.Width;
            int hblocks = tile.Coords.Height / Block.Height;

            //Math.Min(sth, block count in that dimension-1) limits the indexes for searching adjacent blocks. If entity gets out of bounds, it'll be considered as being on the last tile in that dimension
            int leftBlockX = Math.Min((int)Math.Floor(entityTilePos.X / Block.Width), wblocks - 1); //get the X coordinate for neighbouring blocks on the left
            int rightBlockX = Math.Min(leftBlockX + (int)Math.Ceiling((double)Width / Block.Width), wblocks - 1);

            if (leftBlockX < 0)
                leftBlockX = 0;

            //Just to make sure we really don't collide, in case the entity is smaller than one block
            if (rightBlockX <= leftBlockX)
                rightBlockX = leftBlockX + 1;

            if (rightBlockX >= wblocks)
                rightBlockX = wblocks - 1;

            int topBlockY = Math.Max(Math.Min((int)Math.Floor(entityTilePos.Y / Block.Height), hblocks - 1), 0); //Math.Max so that we don't check for blocks above the top edge (there are none)
            int bottomBlockY = Math.Min(topBlockY + (int)Math.Ceiling((double)Height / Block.Height) + 1, hblocks - 1);

            //Just to make sure we really don't collide, in case the entity is smaller than one block
            if (bottomBlockY == topBlockY)
                ++bottomBlockY;

            if (bottomBlockY >= hblocks)
                bottomBlockY = hblocks - 1;

            //We don't believe we're grounded until we are proved wrong, prevents doublejumping
            isOnGround = false;

            for (int y = topBlockY; y <= bottomBlockY; ++y) {
                for (int x = leftBlockX; x <= rightBlockX; ++x) {
                    if (tile.Blocks[x, y] != null) {
                        Rectangle otherElement = new Rectangle(x * Block.Width, y * Block.Height, Block.Width, Block.Height);

                        Vector2 depth = GeometryHelper.GetIntersectionDepth(bounds, otherElement);
                        if (depth != Vector2.Zero) {

                            if (IsPlayer)
                                IsPlayer = IsPlayer;

                            //In the first call, we check for X-axis collisions
                            if (doCollisionX) {
                                position = new Vector2(Position.X + depth.X, Position.Y);
                                bounds = BoundingRectangle;
                            }

                            //In the second call, we do the Y-axis
                            else {
                                //We hit a box underneath us, we're grounded
                                if ((bounds.Top < otherElement.Top) && (bounds.Bottom > otherElement.Top)) {
                                    isOnGround = true;
                                    position = new Vector2(Position.X, Position.Y + depth.Y);
                                    bounds = BoundingRectangle;
                                }

                                //We hit a box above us, we're jumping
                                else if ((bounds.Bottom > otherElement.Bottom) && (bounds.Top < otherElement.Bottom)) {
                                    position = new Vector2(Position.X, Position.Y + depth.Y);
                                    jumpTime = MaxJumpTime; //we reached the apex of our jump        
                                    bounds = BoundingRectangle;
                                }
                            }

                        }
                    }
                }
            }

            if (rightBlockX == wblocks -1 && nextTile == null)
            {
                var next = map.GetTileByIndex(CurrentTile + 1);
                if (next != null)
                    TerrainCollisionsXY(doCollisionX, next);
            }

            oldBottom = bounds.Bottom;
        }

        //This is potentially VERY slow
        private void EntityCollisionsXY(bool doCollisionX) {
            List<Entity> collidableEntities = GetEntitiesInSight(Width, Height);

            Rectangle bounds = BoundingRectangle;
            foreach (Entity e in collidableEntities) {
                if (IsPlayer)
                    IsPlayer = IsPlayer;
                Vector2 depth = GeometryHelper.GetIntersectionDepth(bounds, e.BoundingRectangle);
                if (depth != Vector2.Zero) {

                    if (doCollisionX) {
                        position = new Vector2(Position.X + depth.X, Position.Y);
                        bounds = BoundingRectangle;
                    }
                    else {
                        if ((bounds.Top < e.BoundingRectangle.Top) && (bounds.Bottom > e.BoundingRectangle.Top)) {
                            isOnGround = true;
                            position = new Vector2(Position.X, Position.Y + depth.Y);
                            bounds = BoundingRectangle;
                        }

                        else if ((bounds.Bottom > e.BoundingRectangle.Bottom) && (bounds.Top < e.BoundingRectangle.Bottom)) {
                            position = new Vector2(Position.X, Position.Y + depth.Y);
                            bounds = BoundingRectangle;
                            jumpTime = MaxJumpTime; //we reached the apex of our jump                        
                        }
                    }
                }
            }
        }
        #endregion

        //Check whether we moved out of our current tile and adjust the currentTile variable
        private void CheckTile() {
            Tile current = map.GetTileByIndex(CurrentTile);
            if (position.X > current.Coords.Right && CurrentTile < map.TileCount)
                ++CurrentTile;
            else if (position.X < current.Coords.Left && CurrentTile > 0)
                --CurrentTile;
        }
        #endregion

        #region Protected methods
        //Sets combat proeprties to defaults
        protected void ResetProperties()
        {
            Strength = DefaultStrength;
            Defense = DefaultDefense;
            Speed = DefaultSpeed;
        }

        //Applies item modifiers to combat properties
        protected void ApplyItemProperties()
        {
            ResetProperties();
            if (HeldItem != null)
            {
                HP += HeldItem.Properties.HPBonus;
                MaxHP += HeldItem.Properties.HPBonus;
                Strength += HeldItem.Properties.StrengthBonus;
                Defense += HeldItem.Properties.ArmorBonus;
                Speed += HeldItem.Properties.SpeedBonus;

                UpdateOutlineColor();
            }
        }

        protected void DisapplyItemProperties()
        {
            ResetProperties();
            if (HeldItem != null)
            {
                HP -= HeldItem.Properties.HPBonus;
                MaxHP -= HeldItem.Properties.HPBonus;

                UpdateOutlineColor();
            }
        }
        #endregion
    }
}
