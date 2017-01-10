using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2
{
    class Zone
    {
        #region Public variables
        public Texture2D Texture;
        public Rectangle Coords;
        #endregion

        #region Public methods 
        public Zone(GraphicsDevice device, Rectangle coords, Color zoneOutline)
        {
            Texture = new Texture2D(device, coords.Width, coords.Height);
            Coords = coords;
            GraphicsHelper.DrawRectangle(Texture, Color.Transparent);
            GraphicsHelper.OutlineRectangle(Texture, zoneOutline, 3);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Coords, Color.White);
        }
        #endregion
    }
}
