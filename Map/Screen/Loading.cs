using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2
{
    class Loading
    {
        #region Public variables  
        public string Text { get; private set; }
        public Rectangle Coords { get; private set; }
        public Texture2D Background { get; private set; }
        #endregion

        #region Private variables
        private GraphicsDevice device;
        private Map map;
        private Color textColor;
        #endregion

        #region Public methods
        public Loading(GraphicsDevice graphicsDev, Map map, string text)
        {
            device = graphicsDev;
            Text = text;
            CreateCoords();
            this.map = map;
            CreateGraphics();
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(Background, Coords, Color.White);
            spriteBatch.DrawString(Game.font, "RANDIO", new Vector2(Coords.X + 190, Coords.Y + 20), textColor, 0f, Vector2.Zero, 1.4f, SpriteEffects.None, 0f); //adjust coords
            spriteBatch.DrawString(Game.font, Text, new Vector2(Coords.X + 70, Coords.Y + 60), textColor);
            spriteBatch.End();
        }
        #endregion

        #region Private methods
        private void CreateCoords()
        {
            Coords = new Rectangle(Game.WIDTH / 2 - 260, Game.HEIGHT / 2 - 60, 520, 120);
        }

        private void CreateGraphics()
        {
            var palette = map.GetTileByIndex(map.Player.CurrentTile).Palette;

            Background = new Texture2D(device, Coords.Width, Coords.Height);
            GraphicsHelper.DrawRectangle(Background, palette[4]);
            GraphicsHelper.OutlineRectangle(Background, palette[3], 6);

            textColor = palette[3];
        }
        #endregion
    }
}
