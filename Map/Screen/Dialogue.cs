using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Randio_2
{
    class Dialogue
    {
        #region Public variables  
        public string Text { get; private set; }
        public Rectangle Coords { get; private set; }
        public Texture2D Background { get; private set; }
        public Action Action { get; private set; }
        public Action CancelAction { get; private set; }
        #endregion

        #region Private variables
        private GraphicsDevice device;
        private Map map;
        private Color textColor;
        #endregion

        #region Public methods
        public Dialogue(GraphicsDevice graphicsDev, Map map, string text, Action action, Action cancelAction = null)
        {
            device = graphicsDev;
            Text = text;
            CreateCoords();
            this.map = map;
            Action = action;
            CancelAction = cancelAction;
            CreateGraphics();
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(Background, Coords, Color.White);
            spriteBatch.DrawString(Game.font, Text, new Vector2(Coords.X + 20, Coords.Y + 20), textColor);
            spriteBatch.DrawString(Game.font, "YES - [J]       NO - [K]", new Vector2(Coords.X + 60, Coords.Y + 60), textColor);
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
            GraphicsHelper.DrawRectangle(Background, palette[2]);
            GraphicsHelper.OutlineRectangle(Background, palette[1], 6);

            textColor = palette[1];
        }
        #endregion
    }
}
