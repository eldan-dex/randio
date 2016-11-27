using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2 {
    class Entity {

        //Public variables
        //********************************************************************************//
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

        public Texture2D Texture { get; protected set; }
        public int CurrentTile { get; protected set; } //public for debugging purposes
        public int Width { get; private set; }
        public int Height { get; private set; }


        //Private/Protected variables
        //********************************************************************************//
        //Instance variables
        protected Map map;


        //Will be generated, not hardcoded
        //These values will be entity defaults, every entity will then adjust them as needed
        //TODO: edit these to be entity defaults

        // Constants for controling horizontal movement
        private const float MoveAcceleration = 13000.0f;
        private const float MaxMoveSpeed = 1750.0f;
        private const float GroundDragFactor = 0.48f;
        private const float AirDragFactor = 0.58f;

        // Constants for controlling vertical movement
        private const float MaxJumpTime = 0.35f;
        private const float JumpLaunchVelocity = -3500.0f;
        private const float GravityAcceleration = 3400.0f;
        private const float MaxFallSpeed = 550.0f;
        private const float JumpControlPower = 0.14f;

        //Current state
        protected float movement;
        protected bool isJumping;
        private float jumpTime;
        private bool isOnGround;  
        private bool wasJumping;
        private float oldBottom;


        //Public methods
        //********************************************************************************//
        public Entity(Map map, Vector2 position, int currentTile, int width, int height) {
            this.map = map;
            this.position = position;
            this.CurrentTile = currentTile;
            Width = width;
            Height = height;

            velocity = Vector2.Zero;
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

            //reset movement
            movement = 0.0f;
            isJumping = false;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
            SpriteEffects effect = SpriteEffects.None;
            if (Velocity.X > 0)
                effect = SpriteEffects.FlipHorizontally;

            spriteBatch.Draw(Texture, position, null, Color.White, 0.0f, Vector2.Zero, 1.0f, effect, 0.0f);
        }


        //Private methods
        //********************************************************************************//
        #region Physics stuff (ApplyPhysics, CheckJump, DoCollisionsXY)

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

            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            //Move along the X axis
            position.X += velocity.X * elapsed;
            position.X = (float)Math.Round(position.X);

            //Limit the player to stay on the map horizontally
            if (position.X < 0)
                position.X = 0;
            else if (position.X > map.Width)
                position.X = map.Width;

            //X axis collisions
            DoCollisionsXY(true);

            //Move along the Y axis
            position.Y += velocity.Y * elapsed;
            position.Y = (float)Math.Round(position.Y);

            //Y axis collisions
            DoCollisionsXY(false);

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
        private void DoCollisionsXY(bool doCollisionX, Tile otherTile = null) {
            Rectangle bounds = BoundingRectangle;
            Tile tile = map.GetTileForX((int)position.X);
            Vector2 playerTilePos = map.GlobalToTileCoordinates(position);

            int wblocks = tile.Coords.Width / Block.Width;
            int hblocks = tile.Coords.Height / Block.Height;

            //Math.Min(sth, block count in that dimension-1) limits the indexes for searching adjacent blocks. It player gets out of bounds, he'll be considered as being on the last tile in that dimension
            int leftBlockX = Math.Min((int)Math.Floor(playerTilePos.X / Block.Width), wblocks - 1); //get the X coordinate for neighbouring blocks on the left
            int rightBlockX = Math.Min(leftBlockX + (int)Math.Floor((double)Width / Block.Width), wblocks - 1);

            int topBlockY = Math.Max(Math.Min((int)Math.Floor(playerTilePos.Y / Block.Height), hblocks - 1), 0); //Math.Max so that we don't check for blocks above the top edge (there are none)
            int bottomBlockY = Math.Min(topBlockY + (int)Math.Floor((double)Height / Block.Height), hblocks - 1);

            //We don't believe we're grounded until we are proved wrong, prevents doublejumping
            isOnGround = false;

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
                                    bounds = BoundingRectangle;
                                    jumpTime = MaxJumpTime; //we reached the apex of our jump                        
                                }
                            }

                        }
                    }
                }
            }

            oldBottom = bounds.Bottom;
        }
    #endregion

        //Check whether we moved out of our current tile and adjust the currentTile variable
        private void CheckTile() {
            Tile current = map.GetTileByIndex(CurrentTile);
            if (position.X > current.Coords.Right)
                ++CurrentTile;
            else if (position.X < current.Coords.Left)
                --CurrentTile;
        }
    }
}
