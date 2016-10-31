using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio {
    class Block {
        public Rectangle Position { get; private set; }
        public Texture2D Texture { get; private set; }

        private GraphicsDevice device;

        public Block(GraphicsDevice graphicsDevice, Graphics.Tile parentTile, int X, int Y, int Width, int Height, bool leftSide = true, bool topSide = true, bool rightSide = true, bool bottomSide = true) { //TODO: Rectangle?
            Position = new Rectangle(X, Y, Width, Height);
            Texture = parentTile.BlockTexture;
            device = graphicsDevice;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, Color.White);
        }
    }
}
