using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2
{
    //Zone - block marked as important (red + green doors, reach quests, drop item quests)
    class Zone
    {
        #region Public variables
        public Texture2D Texture { get; private set; }
        public Rectangle Coords { get; private set; }
        public bool Active { get; private set; }
        #endregion

        #region Public methods 
        //Default ctor
        public Zone(GraphicsDevice device, Rectangle coords, Color zoneOutline)
        {
            Active = true;
            Texture = new Texture2D(device, coords.Width, coords.Height);
            Coords = coords;
            GraphicsHelper.FillRectangle(Texture, Color.Transparent);
            GraphicsHelper.OutlineRectangle(Texture, zoneOutline, 6);
        }

        //Draws the current zone
        public void Draw(SpriteBatch spriteBatch)
        {
            if (Active)
                spriteBatch.Draw(Texture, Coords, Color.White);
        }

        //Disables the zone (zone is not being drawn anymore)
        public void Deactivate()
        {
            Active = false;
        }
        #endregion
    }
}
