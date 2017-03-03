using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2
{
    //"Loading" message, displayed when level is being generated. Inherits Dialogue
    class Loading : Dialogue
    {
        #region Public methods
        //Default ctor
        public Loading(GraphicsDevice graphicsDev, Map map, string text) : base()
        {
            device = graphicsDev;
            Text = text;
            CreateCoords();
            this.map = map;
            CreateGraphics();
        }

        //Draws the message in the center of the screen
        public new void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(Background, Coords, Color.White);
            spriteBatch.DrawString(Game.font, "RANDIO", new Vector2(Coords.X + 190, Coords.Y + 20), textColor, 0f, Vector2.Zero, 1.4f, SpriteEffects.None, 0f); //adjust coords
            spriteBatch.DrawString(Game.font, Text, new Vector2(Coords.X + 70, Coords.Y + 60), textColor);
            spriteBatch.End();
        }
        #endregion

        #region Private methods
        //Initialize message colors (depends on current Tile and it's palette)
        private void CreateGraphics()
        {
            var palette = map.GetTileByIndex(map.Player.CurrentTile).Palette;

            Background = new Texture2D(device, Coords.Width, Coords.Height);
            GraphicsHelper.FillRectangle(Background, palette[4]);
            GraphicsHelper.OutlineRectangle(Background, palette[3], 6);

            textColor = palette[3];
        }
        #endregion
    }
}
