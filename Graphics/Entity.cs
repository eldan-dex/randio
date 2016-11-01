using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio.Graphics {
    class Entity {
        public static Texture2D GeneratePlayer(GraphicsDevice device, int width, int height) {
            Texture2D tx = new Texture2D(device, width, height);
            Helper.FillRectangle(tx, Color.Blue);
            return tx;
        }
    }
}
