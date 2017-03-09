using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2 {
    //Base class for Player and NPC, implements Entity properties and physics (collisions, movement, etc)
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

        public bool Alive { get; protected set; }
        public Texture2D Texture { get; protected set; }
        public int CurrentTile { get; protected set; }
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
        public bool CanAttack = true; //used only by NPCs for attack events.
        #endregion

        #region Private/Protected variables
        protected Map map;

        //Outline (HP indicator) width
        protected int outlineWidth = 4;

        //Entity defaults, can be overriden by individual entities
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
        #endregion

        #region Public methods
        //Default ctor
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

        //Performs physics (movement, collisions) on the current entity
        public void Update(GameTime gameTime) {
            //All physics stuff is here
            ApplyPhysics(gameTime);

            //Update currentTile info
            CheckTile();

            //Keep easily accessible direction info. If player is still, remember last direction
            if (movement > 0)
                Direction = 1;
            else if (movement < 0)
                Direction = -1;

            //Reset movement
            movement = 0.0f;
            isJumping = false;
        }

        //Draws entity and it's name, flips texture accodring to entity direction (currently only has an effect on the player)
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
            SpriteEffects effect = SpriteEffects.None;
            if (Direction < 0)
                effect = SpriteEffects.FlipHorizontally;

            Vector2 namePos = new Vector2(Position.X - (Name.Length / 2) * 4, Position.Y - 22);

            spriteBatch.DrawString(Game.font, Name, namePos, Color.Red, 0f, Vector2.Zero, IsPlayer ? 1.2f : 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(Texture, position, null, OverwriteColor, 0.0f, Vector2.Zero, 1.0f, effect, 0.0f);
        }

        //Called by the attacked when this entity is hit by an attack - decreses HP, decides whether entity dies, counts combat statistics
        public void TakeDamage(Entity source, float damage)
        {
            //Defense property decreases damage taken
            var realDamage = (int)(damage - Defense);
            if (realDamage > 0)
            {
                HP -= realDamage;

                if (IsPlayer) //add to stats
                {
                    ((Player)this).Stats.DamageSustained += realDamage;
                }

                //When damaged, entity flashes red for 200ms
                OverwriteColor = Color.Red;
                map.EntityEvents.AddEvent(new Event<Entity>(200, delegate (Entity e) { e.OverwriteColor = Color.White; }, this));
                UpdateOutlineColor();
            }
            else
            {
                //If attack was deflected, entity flashes gray for 200ms
                OverwriteColor = Color.DarkGray;
                map.EntityEvents.AddEvent(new Event<Entity>(200, delegate (Entity e) { e.OverwriteColor = Color.White; }, this));
            }

            if (HP <= 0)
            {
                Alive = false; //now Map Update check will recognise this NPC as dead and will remove it.

                //Drop current item when dead
                if (HeldItem != null)
                {
                    HeldItem.PutDown(Direction);
                    DisapplyItemProperties();
                    HeldItem = null;
                }

                //Add statistics
                if (IsPlayer)
                {
                    ((Player)this).Stats.TimesDead++;
                    FillHP();
                }
                if (source.IsPlayer)
                {
                    ((Player)source).Stats.EnemiesKilled++;
                }
            }
        }

        //Respawn entity on the tile where it died
        public void Reset()
        {
            //For player, use player's default offset from the edge of the map, for entites, use a null vector
            position = (IsPlayer ? ((Player)this).Origin : Vector2.Zero) + new Vector2(map.GetTileByIndex(CurrentTile).Coords.Left, 0);
            Alive = true;
        }

        //Resets HP to it's maximum
        public void FillHP()
        {
            HP = MaxHP;
            UpdateOutlineColor();
        }

        //Updates entity outline which shows health status (100% health - green, 0% health - red. Everything else is a mix of these two colors)
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


        //Returns all entities which are within the specified width and height from this entity
        public List<Entity> GetEntitiesInSight(int xRange, int yRange)
        {
            List<Entity> inSight = new List<Entity>();
            List<NPC> allEntites = map.NPCs;

            foreach (Entity e in allEntites)
            {
                if (e != this && Math.Abs(e.Position.X - Position.X) <= xRange && Math.Abs(e.Position.Y - Position.Y) <= yRange)
                {
                    inSight.Add(e);
                }
            }

            return inSight;
        }

        //Returns the closest entity in range depending on the direction of the current entity
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
                    int distance = (int)(Math.Abs(e.Position.X - (position.X + Width)) + Math.Abs((e.Position.Y + e.Height) - (position.Y + Height)));
                    if (distance < closest)
                    {
                        closest = distance;
                        found = e;
                    }
                }
            }
            else //left
            {
                int closest = int.MaxValue;
                foreach (Entity e in entities)
                {
                    int distance = (int)(Math.Abs((e.Position.X + e.Width) - position.X) + Math.Abs((e.Position.Y + e.Height) - (position.Y + Height)));
                    if (distance < closest) //aaa, so much duplicity.
                    {
                        closest = distance;
                        found = e;
                    }
                }
            }

            return found;
        }

        //Returns all items which are in range
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

        //Returns the closest item in range depending on the direction of the current entity
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
                    int distance = (int)(Math.Abs(i.Position.X - (position.X + Width)) + Math.Abs((i.Position.Y + i.Height) - (position.Y + Height)));
                    if (i.Position.X >= position.X && distance <= range * 3)
                    {
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
                    int distance = (int)(Math.Abs((i.Position.X + i.Width) - position.X) + Math.Abs((i.Position.Y + i.Height) - (position.Y + Height)));
                    if (i.Position.X <= position.X + Width && distance <= range * 3)
                    {
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

                if (HP <= 0)
                    HP = 1;

                if (MaxHP <= 0)
                    MaxHP = 1;

                UpdateOutlineColor();
            }
        }
        #endregion

        #region Private methods
        //Check whether we moved out of our current tile and adjust the currentTile variable
        private void CheckTile()
        {
            Tile current = map.GetTileByIndex(CurrentTile);
            if (position.X > current.Coords.Right && CurrentTile < map.TileCount)
                ++CurrentTile;
            else if (position.X < current.Coords.Left && CurrentTile > 0)
                --CurrentTile;
        }

        #region Physics (ApplyPhysics, CheckJump, TerrainCollisionsXY, EntityCollisionsXY)

        //Move the entity - physics + keyboard (player) / AI (NPC)
        private void ApplyPhysics(GameTime gameTime) {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 lastPosition = position;

            //Count momental velocity
            velocity.X += movement * MoveAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);
            velocity.Y = CheckJump(velocity.Y, gameTime);

            //Factor in the terrain (air or ground)
            if (isOnGround)
                velocity.X *= GroundDragFactor;
            else
                velocity.X *= AirDragFactor;

            var actualMaxMoveSpeed = MaxMoveSpeed * Speed; //to enable speed-enhancing items

            velocity.X = MathHelper.Clamp(velocity.X, -actualMaxMoveSpeed, actualMaxMoveSpeed);

            //Move along the X axis
            position.X += velocity.X * Speed * elapsed;
            position.X = (float)Math.Round(position.X);

            //X axis collisions
            TerrainCollisionsXY(true);
            EntityCollisionsXY(true);

            //Move along the Y axis
            position.Y += velocity.Y * Speed * elapsed;
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
            Tile tile = map.GetTileForX((int)position.X);

            if (nextTile != null)
                tile = nextTile;

            if (tile == null)
                return;

            Vector2 entityTilePos = map.GlobalToTileCoordinates(position, tile.Index);
            Rectangle bounds = new Rectangle((int)entityTilePos.X, (int)entityTilePos.Y, Width, Height);

            //todo: add a condition when player is on tile edge - check next/prev tiles edge line

            int wblocks = tile.Coords.Width / Block.Width;
            int hblocks = tile.Coords.Height / Block.Height;

            //Math.Min(sth, block count in that dimension-1) limits the indexes for searching adjacent blocks. If entity gets out of bounds, it'll be considered as being on the last tile in that dimension
            int leftBlockX = Math.Min((int)Math.Floor(entityTilePos.X / Block.Width), wblocks - 1); //get the X coordinate for neighbouring blocks on the left
            int rightBlockX = Math.Min(leftBlockX + (int)Math.Ceiling((double)Width / Block.Width), wblocks - 1);

            if (leftBlockX < 0)
                leftBlockX = 0;

            //Just to make sure we really don't collide, in case the entity is smaller than one block, extend the checking range
            if (rightBlockX <= leftBlockX)
                rightBlockX = leftBlockX + 1;

            if (rightBlockX >= wblocks)
                rightBlockX = wblocks - 1;

            int topBlockY = Math.Max(Math.Min((int)Math.Floor(entityTilePos.Y / Block.Height), hblocks - 1), 0); //Math.Max so that we don't check for blocks above the top edge (there are none)
            int bottomBlockY = Math.Min(topBlockY + (int)Math.Ceiling((double)Height / Block.Height) + 1, hblocks - 1);

            //Just to make sure we really don't collide, in case the entity is smaller than one block, again...
            if (bottomBlockY == topBlockY)
                ++bottomBlockY;

            if (bottomBlockY >= hblocks)
                bottomBlockY = hblocks - 1;

            //We don't believe we're grounded until we are proved wrong, prevents doublejumping
            isOnGround = false;

            //Loops through all blocks in selected range and checks for possible collisions. If found, moves the entity out of them
            for (int y = topBlockY; y <= bottomBlockY; ++y) {
                for (int x = leftBlockX; x <= rightBlockX; ++x) {
                    if (tile.Blocks[x, y] != null) {
                        Rectangle otherElement = new Rectangle(x * Block.Width, y * Block.Height, Block.Width, Block.Height);

                        Vector2 depth = GeometryHelper.GetIntersectionDepth(bounds, otherElement);
                        if (depth != Vector2.Zero) {

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

            if (position.X < 0)
                position.X = 0;
            else if (position.X + Width > map.Width)
                position.X = map.Width - Width;

            if (rightBlockX == wblocks -1 && CurrentTile < map.TileCount && nextTile == null)
            {
                var next = map.GetTileByIndex(CurrentTile + 1);
                if (next != null)
                    TerrainCollisionsXY(doCollisionX, next);
            }
            else if (leftBlockX == 0 && CurrentTile > 0 && nextTile == null && IsPlayer)
            {
                var next = map.GetTileByIndex(CurrentTile - 1);
                if (next != null)
                    TerrainCollisionsXY(doCollisionX, next);
            }

            oldBottom = bounds.Bottom;
        }

        //Check for collisions with other entities
        private void EntityCollisionsXY(bool doCollisionX) {
            List<Entity> collidableEntities = GetEntitiesInSight(Width, Height);

            Rectangle bounds = BoundingRectangle;
            foreach (Entity e in collidableEntities)
            {
                Vector2 depth = GeometryHelper.GetIntersectionDepth(bounds, e.BoundingRectangle);
                if (depth != Vector2.Zero)
                {

                    if (doCollisionX)
                    {
                        position = new Vector2(Position.X + depth.X, Position.Y);
                        bounds = BoundingRectangle;
                    }
                    else
                    {
                        if ((bounds.Top < e.BoundingRectangle.Top) && (bounds.Bottom > e.BoundingRectangle.Top))
                        {
                            isOnGround = true;
                            position = new Vector2(Position.X, Position.Y + depth.Y);
                            bounds = BoundingRectangle;
                        }

                        else if ((bounds.Bottom > e.BoundingRectangle.Bottom) && (bounds.Top < e.BoundingRectangle.Bottom))
                        {
                            position = new Vector2(Position.X, Position.Y + depth.Y);
                            bounds = BoundingRectangle;
                            jumpTime = MaxJumpTime; //we reached the apex of our jump                        
                        }
                    }
                }
            }
            if (position.X < 0)
                position.X = 0;
            else if (position.X + Width > map.Width)
                position.X = map.Width - Width;
        }
        #endregion
        #endregion
    }
}
