using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2 {
    //NPC class, inherits Entity. Contains NPC controls (AI), properties and interactions
    class NPC : Entity {
        #region Public variables
        //AI behaviour types
        public enum NPCBehaviour {
            GoToPlayer,
            IgnorePlayer,
            RunAwayFromPlayer
        }

        public Tile ParentTile { get; private set; }
        public NPCBehaviour Behaviour { get; private set; }
        public bool PlayerInRange { get; private set; }
        public Vector2 SightRange { get; private set; }

        //"Boss" NPCs have higher stats and are generally tougher
        public bool IsBoss { get; private set; }
        #endregion

        #region Private variables
        int horizontalDirection = 0; //-1 = left, 0 = steady, 1 = up
        int verticalDirection = 0; //-1 = down, 0 = steady, 1 = up
        #endregion

        #region Public methods
        //Default ctor
        public NPC(GraphicsDevice graphicsDevice, Map map, Vector2 position, int parentTile, Tile parentTileObject, int width, int height, int additionalHP = 0, float additionalStrength = 0, float additionalDefense = 0, float additionalSpeed = 0) : base(map, position, parentTile, width, height) {
            ParentTile = parentTileObject;
            InitNPC();
            Behaviour = CreateBehaviour();
            Texture = CreateTexture(graphicsDevice);

            DefaultHP += additionalHP;
            DefaultStrength += additionalStrength;
            DefaultDefense += additionalDefense;
            DefaultSpeed += additionalSpeed;

            //First setting the outline color must be done in child classes
            UpdateOutlineColor();
        }

        //First updates the NPC with AI controls, then updates it as a generic Entity
        new public void Update(GameTime gameTime) {
            AIMovement();
            AIScan();
            AIInteract();
            base.Update(gameTime);
        }
        #endregion

        #region Private methods
        //Create the NPC texture (color indicates behaviour type)
        private Texture2D CreateTexture(GraphicsDevice graphicsDevice) {
            var texture = new Texture2D(graphicsDevice, Width, Height);

            Color color;
            if (Behaviour == NPCBehaviour.GoToPlayer)
                color = Color.Violet;
            else if (Behaviour == NPCBehaviour.RunAwayFromPlayer)
                color = Color.Lime;
            else
                color = Color.Yellow;

            GraphicsHelper.FillRectangle(texture, color);

            if (IsBoss)
                outlineWidth = 6;
            else
                outlineWidth = 3;

            return texture;
        }

        //Picks a random behaviour type
        private NPCBehaviour CreateBehaviour() {
            return (NPCBehaviour)AlgorithmHelper.GetRandom(0, 3);
        }

        //Initializes the NPC
        private void InitNPC() {
            SightRange = new Vector2(640, 368); //todo: range can be adjusted
            IsBoss = AlgorithmHelper.GetRandom(0, 11) == 0; //currently 10% chance of being a boss

            //NPC defaults
            DefaultHP = 3;
            DefaultStrength = 1;
            DefaultDefense = 0;

            if (IsBoss)
            {
                Width += 5;
                Height += 5;

                //Boss defaults
                DefaultHP += 2;
                DefaultStrength += 1;
                DefaultDefense += 1;
            }

            Name = StringHelper.GenerateName();
        }

        //All AI behaviour
        //Moves according to behaviour type
        private void AIMovement() {
            if (PlayerInRange && Behaviour == NPCBehaviour.GoToPlayer)
                AITrackPlayer();

            else if (PlayerInRange && Behaviour == NPCBehaviour.RunAwayFromPlayer)
                AIRunAway();

            else
                AIIdle();

            AIPerformMovement();
        }

        //Decide NPC direction based on position relative to player (follows)
        private void AITrackPlayer() { //Following player
            Vector2 playerPos = map.Player.Position;

            if (playerPos.X < Position.X)
                horizontalDirection = -1; //go left

            else if (playerPos.X > Position.X)
                horizontalDirection = 1; //go right

            else 
                horizontalDirection = 0; //stay still

            var yDiff = playerPos.Y - Position.Y;
            if (yDiff < 0 && Math.Abs(yDiff) >= Block.Height/2)
                verticalDirection = 1; //go up

            else if (yDiff > 0 && Math.Abs(yDiff) >= Block.Height /2)
                verticalDirection = -1; //go down

            else
                verticalDirection = 0; //stay on the same level

        }

        //Just aimlessly wander around
        private void AIIdle() { //Idle
            int randX = AlgorithmHelper.GetRandom(0, 11);
            int randY = AlgorithmHelper.GetRandom(0, 11);
            int changeDir = AlgorithmHelper.GetRandom(0, 51);

            if (changeDir == 50) //2% chance that direction will be changed
            {
                if (randX < 3)
                    horizontalDirection = -1;
                else if (randX > 6)
                    horizontalDirection = 1;
                else
                    horizontalDirection = 0;

                if (randY < 3)
                    verticalDirection = -1;
                else if (randY > 6)
                    verticalDirection = 1;
                else
                    verticalDirection = 0;
            }
        }

        //Decide NPC direction based on position relative to player (runs away)
        private void AIRunAway() { //Basically inverted AITrackPlayer()
            Vector2 playerPos = map.Player.Position;

            if (playerPos.X < Position.X)
                horizontalDirection = 1; //go right

            else if (playerPos.X > Position.X)
                horizontalDirection = -1; //go left

            else
                horizontalDirection = 0; //stay still


            var yDiff = playerPos.Y - Position.Y;
            if (yDiff < 0 && Math.Abs(yDiff) >= Block.Height / 2)
                verticalDirection = -1; //go down

            else if (yDiff > 0 && Math.Abs(yDiff) >= Block.Height / 2)
                verticalDirection = 1; //go up

            else
                verticalDirection = 0; //stay on the same level

        }

        //Performs moement based on set direction
        private void AIPerformMovement()
        {
            bool canFallDown = (Behaviour == NPCBehaviour.RunAwayFromPlayer); //Greens don't fear holes and are happy to fall in them -.-
            isJumping = false;

            //Vertical movement
            var nextVertBlock = position + new Vector2(0, verticalDirection * Block.Height);
            if (!map.IsBlock(nextVertBlock))
            {
                if (verticalDirection == 1)
                    isJumping = true;

                else if (verticalDirection == -1)
                    canFallDown = true;
            }

            //Horizontal movement
            var nextHorizBlock = position + new Vector2(horizontalDirection * Block.Width, 0);
            if (!map.IsBlock(nextHorizBlock))
            {
                var gndBlockA = nextHorizBlock + new Vector2(0, 1 * Block.Height);
                var gndBlockB = gndBlockA + new Vector2(horizontalDirection * 3 * Block.Width, 0);

                if (map.IsBlock(gndBlockA) || canFallDown) //just go sideways
                    movement = horizontalDirection * 1f;

                else if (!map.IsBlock(gndBlockA) && map.IsBlock(gndBlockB)) //try to jump over holes
                {
                    movement = horizontalDirection * 1f;
                    isJumping = true;
                }
                    
            }

            else
            {
                isJumping = true;
                movement = horizontalDirection * 1f;
            }
        }

        //Check whether player is within range from this NPC
        private void AIScan() {
            Vector2 distance = map.Player.Position - Position;
            if (Math.Abs(distance.X) <= SightRange.X && Math.Abs(distance.Y) <= SightRange.Y)
                PlayerInRange = true;
            else
                PlayerInRange = false;
        }

        //Try attacking the player if possible
        private void AIInteract()
        {
            if (CanAttack)
            {
                var dist = GeometryHelper.VectorDistance(Position, map.Player.Position);
                if ((Direction == -1 && map.Player.Position.X <= Position.X && dist <= Range + map.Player.Width) || (Direction == 1 && map.Player.Position.X + map.Player.Width >= Position.X && dist <= Range + Width))
                {
                    map.Player.TakeDamage(this, Strength);
                    CanAttack = false;
                    map.EntityEvents.AddEvent(new Event<Entity>(500, delegate (Entity e) { e.CanAttack = true; }, this));
                }
            }
        }
        #endregion
    }
}
