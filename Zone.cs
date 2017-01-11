using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2
{
    class Zone
    {
        #region Public variables
        public Texture2D Texture { get; private set; }
        public Rectangle Coords { get; private set; }
        public bool Active { get; private set; }
        #endregion

        #region Public methods 
        public Zone(GraphicsDevice device, Rectangle coords, Color zoneOutline)
        {
            Active = true;
            Texture = new Texture2D(device, coords.Width, coords.Height);
            Coords = coords;
            GraphicsHelper.DrawRectangle(Texture, Color.Transparent);
            GraphicsHelper.OutlineRectangle(Texture, zoneOutline, 3);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Active)
                spriteBatch.Draw(Texture, Coords, Color.White);
        }

        public void Deactivate()
        {
            Active = false;
        }
        #endregion
    }
}
