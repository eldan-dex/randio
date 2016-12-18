using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Randio_2
{
    class Background
    {
        public RenderTarget2D Texture { get; protected set; }
        public Texture2D BlockTexture { get; protected set; } //TODO: will be RenderTarget2D in the future

        public Background()
        {
        }
    }
}
