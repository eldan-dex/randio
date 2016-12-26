﻿using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Randio_2 {
    class Map {
        #region Public variables  
        public Player Player { get; private set; }
        public bool ReachedExit { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int TileCount { get { return tiles.Count; } }
        //public List<NPC> NPCs { get; private set; }
        public EventManager<Entity> entityEvents;
        #endregion

        #region Private variables
        private List<Tile> tiles;
        private Camera camera;
        #endregion

        #region Public methods
        public Map(GraphicsDevice graphicsDevice, Camera camera, int width, int height) {
            this.camera = camera;
            Width = width;
            Height = height;
            CreatePlayer(graphicsDevice);
            CreateTiles(graphicsDevice);

            entityEvents = new EventManager<Entity>();
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState) {
            Player.Update(gameTime, keyboardState);
            UpdateTiles(gameTime);

            MoveCamera();

            entityEvents.Update();

            if (CheckOutOfMap((int)Player.Position.Y) == -1) {
                //player fell down, reset player
                ResetPlayer();
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
            List<NPC> visibleNPCs = new List<NPC>();
            foreach (Tile t in GetVisibleTiles()) {
                t.Draw(spriteBatch);

                //Draw Tile NPCs (not a part of Tile.Draw because we need to have GameTime available)
                foreach (NPC n in t.NPCs)
                    visibleNPCs.Add(n);
            }

            foreach (NPC n in visibleNPCs)
                n.Draw(gameTime, spriteBatch);

            Player.Draw(gameTime, spriteBatch);
        }

        //Check where given Y coordinate lies relative to the map
        public int CheckOutOfMap(int y) {
            if (y < 0) //y is above the map
                return 1;

            else if (y > Height) //y is below the map
                return -1;

            else return 0; //y is within the map
        }

        //returns a tile which contains the given X coordinate
        public Tile GetTileForX(int x) => tiles.FirstOrDefault(t => t.Coords.Left <= x && t.Coords.Right > x);

        //returns tile with the corresponding index
        public Tile GetTileByIndex(int index) {
            if (index >= 0 && index < tiles.Count)
                return tiles[index];
            return null;
        }

        public void ResetPlayer()
        {
            Player.Reset();
            CameraToPlayer();
        }

        //translate global position into an offset from the left boundary of the given tile
        public Vector2 GlobalToTileCoordinates(Vector2 global, int tileIndex)
        {
            var tile = GetTileByIndex(tileIndex);

            if (tile == null)
                throw new NotSupportedException("Invalid tileIndex value!");

            return new Vector2(global.X - tile.Coords.X, global.Y);
        }

        public List<Entity> GetAllEntites() {
            List<Entity> result = new List<Entity>();

            result.Add(Player);
            foreach (Tile t in tiles)
                result.AddRange(t.NPCs);

            return result;
        }
        #endregion

        #region Private methods
        private void CreatePlayer(GraphicsDevice graphicsDevice) {
            Player = new Player(graphicsDevice, this, Vector2.Zero); //generate position
            CameraToPlayer();
        }

        private void CameraToPlayer() {
            camera.CenterXTo(new Rectangle((int)Player.Position.X, (int)Player.Position.Y, Player.Width, Player.Height));
        }

        private void MoveCamera() {
            float leftEdge = camera.Position.X;
            float rightEdge = leftEdge + Game.WIDTH;

            if (Player.Position.X - leftEdge < Player.SafeMargin) {
                //move camera left if possible
                if (camera.Position.X > 5)
                    camera.Position -= new Vector2(5, 0);
            }
            else if (rightEdge - (Player.Position.X + Player.Width) < Player.SafeMargin) {
                //move camera right is possible
                if (camera.Position.X < Width)
                    camera.Position += new Vector2(5, 0);
            }
        }

        private void CreateTiles(GraphicsDevice graphicsDevice) {
            if (Width % Block.Width > 0 || Height % Block.Height > 0)
                throw new ArgumentException("Map dimensions must be divisible by Block dimensions!");

            tiles = new List<Tile>();

            //Temporary algorithm, will be upgraded
            int minWidth = Game.WIDTH;
            int maxWidth = 3 * Game.WIDTH;
            int totalWidth = 0;
            int tileIndex = 0;

            while (totalWidth < Width) {
                //Generate a random width for the next tile, but keep it divisible by Block.Width
                int newWidth = AlgorithmHelper.GetRandom(minWidth / Block.Width, maxWidth / Block.Width) * Block.Width;

                //If last tile wouldn't be able to fit, extend this one instead
                int testWidth = totalWidth + newWidth;
                if (Width - testWidth < minWidth) {
                    //Don't make the last tile too big, otherwise MonoGame might crash
                    if (Width - totalWidth <= 4096)
                        newWidth = Width - totalWidth;
                    else
                        newWidth = 4096; //This breaks the purprose of the whole algorithm, because the next tile might be too small. But it fixes the crashing.
                }

                //Generate TileType
                var type = (Tile.TileType)AlgorithmHelper.GetRandom(0, Tile.TileTypeCount);

                //Create and add Tile
                tiles.Add(new Tile(graphicsDevice, this, type, new Rectangle(totalWidth, 0, newWidth, Height), tileIndex));

                totalWidth += newWidth;
                ++tileIndex;
            }
        }

        //Updates Tiles and NPCs on them
        private void UpdateTiles(GameTime gameTime) {
            var visibleTiles = GetVisibleTiles();
            foreach (Tile tile in visibleTiles)
                tile.Update(gameTime);
        }

        private Tile[] GetVisibleTiles() {
            List<Tile> visibleTiles = new List<Tile>();

            var pX = Player.Position.X;
            if (pX < 0)
                pX = 0;

            var leftBound = pX - Game.WIDTH + Player.SafeMargin;
            var rightBound = pX + Player.Width + Game.WIDTH - Player.SafeMargin;

            //Bounds cannot be outside the map
            if (leftBound < 0)
                leftBound = 0;
            if (rightBound > Width)
                rightBound = Width;

            //Add unique tiles on the right and left end of screen (implying player is centered)
            var tile = GetTileForX((int)leftBound);
            if (tile != null)
                visibleTiles.Add(tile);

            Tile current = GetTileForX((int)pX);
            if (!visibleTiles.Contains(current))
                visibleTiles.Add(current);

            Tile other = GetTileForX((int)rightBound);

            //hackaround, because GetTileForX returns null on the last coordinate
            if (rightBound == Width)
                other = GetTileByIndex(tiles.Count - 1);

            if (!visibleTiles.Contains(other))
                visibleTiles.Add(other);

            return visibleTiles.ToArray();
        }
        #endregion
    }
}
