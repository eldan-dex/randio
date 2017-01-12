using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2 {
    class Block {
        #region Public variables
        public Texture2D Texture = null;
        public Texture2D TopmostTexture = null;
        public const int Width = 40; //32 small, 48 big
        public const int Height = 40; //32 small, 48 big
        public static readonly Vector2 Size = new Vector2(Width, Height);
        #endregion

        #region Public methods 
        public Block(Texture2D texture, bool topmost) {
            if (topmost)
                TopmostTexture = texture;
            else
                Texture = texture;
        }
        #endregion
    }
}
