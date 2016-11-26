using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Randio_2 {
    class Map {

        private List<Tile> tiles;
        public Player Player { get; private set; }
        public bool ReachedExit { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        private Vector2 start; //useful?

        public Map(GraphicsDevice graphicsDevice, int width, int height) {
            Width = width;
            Height = height;
            CreatePlayer(graphicsDevice);
            CreateTiles(graphicsDevice);
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState) {
            Player.Update(gameTime, keyboardState);
            UpdateEntities(gameTime);

            if (CheckOutOfMap((int)Player.Position.Y) == -1) {
                //player fell down, reset player
                Player.Reset();
                CameraToPlayer();
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
            foreach (Tile t in GetVisibleTiles())
                t.Draw(spriteBatch);

            Player.Draw(gameTime, spriteBatch);

            //draw entities?
        }

        //Public helper methods

        public int CheckOutOfMap(int y) {
            if (y < 0) //y is above the map
                return 1;

            else if (y > Height) //y is below the map
                return -1;

            else return 0; //y is on the map
        }

        //returns a tile which contains the given X coordinate
        public Tile GetTileForX(int x) {
            foreach (Tile t in tiles) {
                if (t.Coords.Left <= x && t.Coords.Right >= x)
                    return t;
            }
            return null;
        }

        //translate global position into an offset from the left boundary of the parent tile
        //untested, but should work
        public Vector2 GlobalToTileCoordinates(Vector2 global) {
            int globalOffset = 0;
            foreach (Tile t in tiles) {
                if (t.Coords.Left <= (int)global.X && t.Coords.Right >= (int)global.X)
                    break;
                else
                    globalOffset += t.Coords.Width;
            }
            return new Vector2(global.X - globalOffset, global.Y);
        }


        //Private helper methods

        private void CreatePlayer(GraphicsDevice graphicsDevice) {
            Player = new Player(graphicsDevice, this, Vector2.Zero); //generate position
            CameraToPlayer();
        }

        private void CameraToPlayer() {
            //TBI: set camera so that is centered on the player
        }

        private void CreateTiles(GraphicsDevice graphicsDevice) {
            if (Width % Block.Width > 0 || Height % Block.Height > 0)
                throw new ArgumentException("Map dimensions must be divisible by Block dimensions!");

            Random rnd = AlgorithmHelper.GetNewRandom();
            tiles = new List<Tile>();

            //Temporary algorithm, will be upgraded
            int minWidth = Game.WIDTH;
            int maxWidth = 3 * Game.WIDTH;
            int totalWidth = 0;

            while (totalWidth < Width) {
                //Generate a random width for the next tile
                int newWidth = rnd.Next(minWidth, maxWidth + 1);

                //If last tile wouldn't be able to fit, extend this one instead
                int testWidth = totalWidth + newWidth;
                if (Width - testWidth < minWidth)
                    newWidth = Width - totalWidth;

                //Generate TileType
                var type = (Tile.TileType)rnd.Next(0, 4);

                //Create and add Tile
                tiles.Add(new Tile(graphicsDevice, type, new Rectangle(totalWidth, 0, newWidth, Height)));

                totalWidth += newWidth;
            }
        }

        private void UpdateEntities(GameTime gameTime) {
            var visibleTiles = GetVisibleTiles();
            foreach (Tile tile in visibleTiles)
                tile.Update(gameTime);
        }



        private Tile[] GetVisibleTiles() {
            List<Tile> visibleTiles = new List<Tile>();

            var pX = Player.Position.X;
            var leftBound = pX - Game.WIDTH + Player.SafeBoundary;
            var rightBound = pX + Player.Width + Game.WIDTH - Player.SafeBoundary;

            //Bounds cannot be outside the map
            if (leftBound < 0)
                leftBound = 0;
            if (rightBound > Width)
                rightBound = Width;


            int currentX = (int)leftBound;
            while (currentX <= rightBound) {
                var tile = GetTileForX(currentX);
                if (tile != null) {
                    currentX += tile.Coords.Width;
                    visibleTiles.Add(tile);
                }
                else { //if no tile was found, something is wrong.
                    currentX += Width / Block.Width / 10; //Minimal tile width
                }
            }

            return visibleTiles.ToArray();
        }
    }
}
