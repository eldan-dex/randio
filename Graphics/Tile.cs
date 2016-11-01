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
        public Rectangle Coords { get; private set; }

        private GraphicsDevice device;
        
        //these X and Y are global, not relative to current camera position! (-> use different names?)
        public Tile(GraphicsDevice graphicsDevice, int x, int y, int width, int height, TileType type) {
            device = graphicsDevice;
            Type = type;
            TileTexture = new Texture2D(graphicsDevice, width, height);
            BlockTexture = new Texture2D(graphicsDevice, 32, 32); //32x32 textures... too small? too big? idk.
            Coords = new Rectangle(x, y, width, height);

            FillTileTexture();
        }

        //TODO: fill textures with actual data
        private void FillTileTexture() {
            //Create Tile texture
            //Placeholder
            Helper.FillRectangle(TileTexture, Color.LightYellow);
            Helper.OutlineRectangle(TileTexture, Color.Black, 2);

            //add placeholder text saying which tile type this is
        }

        private void FillBlockTexture() {
            //Create Block texture
            //Placeholder
            Helper.FillRectangle(BlockTexture, Color.Red);
        }

        public void Draw(SpriteBatch spriteBatch, int x) {
            //To be implemented
            //X is passed dynamically, Y, Width, Height are static... but implementing it like this just looks awful
            spriteBatch.Draw(TileTexture, new Rectangle(x, Coords.Y, Coords.Width, Coords.Height), Color.White);
        }
    }
}
