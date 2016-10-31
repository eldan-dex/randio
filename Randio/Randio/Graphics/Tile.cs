using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio.Graphics {
    class Tile {
        public enum TileType {
            City,
            Nature,
            Sky,
            Abstract,
            Invalid = 999
        }

        public TileType Type { get; private set; }
        public Texture2D TileTexture { get; private set; }
        public Texture2D BlockTexture { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        private GraphicsDevice device;
        
        public Tile(GraphicsDevice graphicsDevice, int width, int height, TileType type) {
            device = graphicsDevice;
            Type = type;
            TileTexture = new Texture2D(graphicsDevice, width, height);
            BlockTexture = new Texture2D(graphicsDevice, 32, 32); //32x32 textures... too small? too big? idk.
            Width = width;
            Height = height;

            FillTileTexture();
        }

        //TODO: fill textures with actual data
        private void FillTileTexture() {
            //Create Tile texture
            //Placeholder
            Helper.FillRectangle(TileTexture, Color.White);
            Helper.OutlineRectangle(TileTexture, Color.Black, 2);

            //add placeholder text saying which tile type this is
        }

        private void FillBlockTexture() {
            //Create Block texture
            //Placeholder
            Helper.FillRectangle(BlockTexture, Color.Red);
        }

        public void Draw(SpriteBatch spriteBatch, int X) {
            //To be implemented
            //X is passed dynamically, Y, Width, Height are static
            spriteBatch.Draw(TileTexture, new Rectangle(X, 0, Width, Height), Color.White);
        }
    }
}
