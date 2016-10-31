using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Randio.Graphics;

namespace Randio {
    class Map {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public List<Tile> Tiles { get; private set; }
        public List<Block> Blocks { get; private set; }

        private GraphicsDevice device;

        public Map(GraphicsDevice graphicsDevice, int width, int height) {
            device = graphicsDevice;
            Tiles = new List<Tile>();
            Width = width;
            Height = height;        
            InitTiles();
            InitBlocks();
        }

        //TODO: Maybe return whole Tile instead of just TileType?
        public Tile GetTile(int X) {
            var pos = 0;
            foreach (Tile tile in Tiles) {
                if (pos == X || pos < X && pos + tile.Width > X)
                    return tile;
            }
            throw new MissingMemberException("No tile at this x coordinate!");
        }

        //TODO: rewrite this with rulesets
        private void InitTiles() {
            Random rnd = new Random((int)DateTime.Now.Ticks);

            //TODO: sizes from ruleset
            var minTileWidth = 300;
            var maxTileWidth = 800;
            if (Width / 4 > minTileWidth)
                maxTileWidth = Width / 4;
            var totalWidth = 0;

            while (totalWidth < Width) {
                //If we're generating the last tile, try to adjust maxTileWidth so that it would fit
                if (Width - totalWidth < maxTileWidth) {
                    maxTileWidth = Width - totalWidth;
                    if (minTileWidth > maxTileWidth)
                        minTileWidth = maxTileWidth;
                }

                //Generate a random width for the next tile
                var newWidth = rnd.Next(minTileWidth, maxTileWidth);

                //If last tile wouldn't be able to fit, extend this one instead
                totalWidth += newWidth;
                if (Width - totalWidth < minTileWidth)
                    newWidth += Width - totalWidth;

                //Create and add tile
                var type = (Tile.TileType)rnd.Next(0, 4); //pick a random TileType
                Tiles.Add(new Tile(device, newWidth, Height, type));
            }
        }

        //Generate all blocks, ensure a valid path exists from start to end
        private void InitBlocks() {
            //placeholder, one block
            int x = 10;
            int y = 10;
            int width = 32;
            int height = 32;
            Blocks = new List<Block>();
            Blocks.Add(new Block(device, GetTile(x), x, y, width, height));
        }
    }
}
