using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2 {
    class NPC : Entity {
        //Public variables
        //********************************************************************************//
        public enum NPCBehaviour {
            GoToPlayer,
            IgnorePlayer,
            RunAwayFromPlayer
        }

        public Tile ParentTile { get; private set; }
        public NPCBehaviour Behaviour { get; private set; }
        public bool PlayerInRange { get; private set; }
        public Vector2 SightRange { get; private set; }

        //Public methods
        //********************************************************************************//
        public NPC(GraphicsDevice graphicsDevice, Map map, Vector2 position, int parentTile, int width, int height) : base(map, position, parentTile, width, height) {
            ParentTile = map.GetTileByIndex(parentTile);
            Behaviour = CreateBehaviour();
            Texture = CreateTexture(graphicsDevice);
            InitNPC();
        }

        new public void Update(GameTime gameTime) {
            AIMovement();
            AIScan();
            base.Update(gameTime);
        }

        //Public methods
        //********************************************************************************//
        private Texture2D CreateTexture(GraphicsDevice graphicsDevice) {
            var texture = new Texture2D(graphicsDevice, Width, Height);

            //Temporary placeholder code, will call texture generation here
            Color color;
            if (Behaviour == NPCBehaviour.GoToPlayer)
                color = Color.Violet;
            else if (Behaviour == NPCBehaviour.RunAwayFromPlayer)
                color = Color.Lime;
            else
                color = Color.Yellow;

            GraphicsHelper.DrawRectangle(texture, color);
            GraphicsHelper.OutlineRectangle(texture, Color.Brown, 2);

            return texture;
        }

        private NPCBehaviour CreateBehaviour() {
            Random rnd = AlgorithmHelper.GetNewRandom();
            return (NPCBehaviour)rnd.Next(0, 3);
        }

        private void InitNPC() {
            SightRange = new Vector2(640, 368); //TODO: adjust range

            //TODO: edit stats on a per-entity basis
        }

        //All AI behaviour
        private void AIMovement() {
            if (PlayerInRange && Behaviour == NPCBehaviour.GoToPlayer)
                AITrackPlayer();

            else if (PlayerInRange && Behaviour == NPCBehaviour.RunAwayFromPlayer)
                AIRunAway();

            else
                AIIdle();
        }

        private void AITrackPlayer() {
            Vector2 playerPos = map.Player.Position;

            if (playerPos.X < Position.X) {
                //go left
                movement = -1f;
            }
            else if (playerPos.X > Position.X) {
                //go right
                movement = 1f;
            }

            if (playerPos.Y < Position.Y) {
                //go up
                if (!isJumping)
                    isJumping = true;
            }
            else if (playerPos.Y > Position.Y) {
                //go down

            }

        }

        private void AIIdle() { //Idle
            if (!isJumping)
                isJumping = true;
        }

        private void AIRunAway() { //Basically inverted AITrackPlayer()
            Vector2 playerPos = map.Player.Position;

            if (playerPos.X < Position.X) {
                //go right
                movement = 1f;
            }
            else if (playerPos.X > Position.X) {
                //go left
                movement = -1f;
            }

            if (playerPos.Y < Position.Y) {
                //go down
            }
            else if (playerPos.Y > Position.Y) {
                //go up
                isJumping = true;
            }

        }

        private void AIScan() {
            Vector2 distance = map.Player.Position - Position;
            if (Math.Abs(distance.X) <= SightRange.X && Math.Abs(distance.Y) <= SightRange.Y)
                PlayerInRange = true;
            else
                PlayerInRange = false;
        }
    }
}
