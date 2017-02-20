using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Randio_2
{
    class Loading
    {
        #region Public variables  
        public string Text { get; private set; }
        public Rectangle Coords { get; private set; }
        public Texture2D Background { get; private set; }
        public EventManager<Loading> TextAnimationMgr { get; private set; }
        #endregion

        #region Private variables
        private GraphicsDevice device;
        private Map map;
        private Color textColor;
        private int dotCount = 0;
        private string currentLoadingText;
        private int interval;
        #endregion

        #region Public methods
        public Loading(GraphicsDevice graphicsDev, Map map, string text, int interval)
        {
            device = graphicsDev;
            Text = text;
            CreateCoords();
            this.map = map;
            this.interval = interval;
            TextAnimationMgr = new EventManager<Loading>();
            CreateGraphics();
            SetText(this);

        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(Background, Coords, Color.White);
            spriteBatch.DrawString(Game.font, "RANDIO", new Vector2(Coords.X + 20, Coords.Y + 20), textColor); //adjust coords
            spriteBatch.DrawString(Game.font, currentLoadingText, new Vector2(Coords.X + 60, Coords.Y + 60), textColor);
            spriteBatch.End();
        }
        #endregion

        #region Private methods
        private void SetText(Loading parameter)
        {
            if (dotCount > 10)
                dotCount = 0;

            currentLoadingText = Text;
            for (int i = 0; i < dotCount; ++i)
                currentLoadingText += ".";

            ++dotCount;

            TextAnimationMgr.AddEvent(new Event<Loading>(interval, SetText, null));
        }

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
