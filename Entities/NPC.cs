﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2 {
    class NPC : Entity {
        #region Public variables
        public enum NPCBehaviour {
            GoToPlayer,
            IgnorePlayer,
            RunAwayFromPlayer
        }

        public Tile ParentTile { get; private set; }
        public NPCBehaviour Behaviour { get; private set; }
        public bool PlayerInRange { get; private set; }
        public Vector2 SightRange { get; private set; }

        public bool IsBoss { get; private set; }
        #endregion

        #region Private variables
        int lastDirection = 0;
        #endregion

        #region Public methods
        public NPC(GraphicsDevice graphicsDevice, Map map, Vector2 position, int parentTile, int width, int height, int additionalHP = 0, float additionalStrength = 0, float additionalDefense = 0, float additionalSpeed = 0) : base(map, position, parentTile, width, height) {
            ParentTile = map.GetTileByIndex(parentTile);
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

        new public void Update(GameTime gameTime) {
            AIMovement();
            AIScan();
            AIInteract();
            base.Update(gameTime);
        }
        #endregion

        #region Private methods
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

            if (IsBoss)
                outlineWidth = 4;
            else
                outlineWidth = 3;

            return texture;
        }

        private NPCBehaviour CreateBehaviour() {
            return (NPCBehaviour)AlgorithmHelper.GetRandom(0, 3);
        }

        private void InitNPC() {
            SightRange = new Vector2(640, 368); //TODO: adjust range
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
            int ran = AlgorithmHelper.GetRandom(0, 27);
            if (ran == 0)
                lastDirection = 1;

            else if (ran == 1)
                lastDirection = -1;

            else if (ran > 23)
                lastDirection = 0;

            movement = lastDirection * 0.7f;
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

        private void AIInteract()
        {
            if (CanAttack)
            {
                var dist = GeometryHelper.VectorDistance(Position, map.Player.Position);
                if ((Direction == -1 && map.Player.Position.X < Position.X && dist <= Range + map.Player.Width) || (Direction == 1 && map.Player.Position.X >= Position.X && dist <= Range + Width))
                {
                    map.Player.TakeDamage(this, Strength);
                    CanAttack = false;
                    map.entityEvents.AddEvent(new Event<Entity>(500, delegate (Entity e) { e.CanAttack = true; }, this));
                }
            }
        }

        //todo: allow npcs to pick/drop items?
        #endregion
    }
}