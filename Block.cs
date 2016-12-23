﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2 {
    class Block {
        //Public variables
        //********************************************************************************//   
        public Texture2D Texture;
        public const int Width = 48; //32 small
        public const int Height = 48; //32 small
        public static readonly Vector2 Size = new Vector2(Width, Height);

        //Public methods
        //********************************************************************************//   
        public Block(Texture2D texture) {
            Texture = texture;
        }
    }
}
