using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace Randio_2
{
    class Background
    {
        #region Public variables
        public RenderTarget2D Texture { get; protected set; }
        public Texture2D BlockTexture { get; protected set; }
        public Texture2D BlockTopmostTexture { get; protected set; }
        public bool OutlineBlocks { get; protected set; } = true;
        #endregion

        #region Protected variables
        protected Color[] palette;
        protected float darkenBy = -0.5f;
        #endregion

        #region Public methods
        public Background()
        {
        }
        #endregion
    }
}
