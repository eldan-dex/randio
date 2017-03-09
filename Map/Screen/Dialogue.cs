using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2
{
    //Confirmation dialogue shown when ending a Map/Screen or quitting the game
    class Dialogue
    {
        #region Public variables  
        public string Text { get; protected set; }
        public Rectangle Coords { get; protected set; }
        public Texture2D Background { get; protected set; }
        public Action Action { get; protected set; }
        public Action CancelAction { get; protected set; }
        #endregion

        #region Private variables
        protected GraphicsDevice device;
        protected Map map;
        protected Color textColor;
        #endregion

        #region Public methods
        //Inherited ctor
        public Dialogue()
        { 
        }

        //Default ctor
        public Dialogue(GraphicsDevice graphicsDev, Map map, string text, Action action, Action cancelAction = null)
        {
            this.map = map;
            device = graphicsDev;
            Text = text;
            Action = action;
            CancelAction = cancelAction;

            CreateCoords();
            CreateGraphics();
        }

        //Displays the prompt in the center of the screen
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(Background, Coords, Color.White);
            spriteBatch.DrawString(Game.font, Text, new Vector2(Coords.X + 20, Coords.Y + 20), textColor);
            spriteBatch.DrawString(Game.font, "YES - [J]       NO - [K]", new Vector2(Coords.X + 60, Coords.Y + 60), textColor);
            spriteBatch.End();
        }
        #endregion

        #region Protected methods
        //Sets proper coordinates for the message to be centered
        protected void CreateCoords()
        {
            Coords = new Rectangle(Game.WIDTH / 2 - 260, Game.HEIGHT / 2 - 60, 520, 120);
        }
        #endregion

        #region Private methods
        //Initialize message colors (depends on current Tile and it's palette)
        private void CreateGraphics()
        {
            var palette = map.GetTileByIndex(map.Player.CurrentTile).Palette;

            Background = new Texture2D(device, Coords.Width, Coords.Height);
            GraphicsHelper.FillRectangle(Background, palette[2]);
            GraphicsHelper.OutlineRectangle(Background, palette[1], 6);

            textColor = palette[1];
        }
        #endregion
    }
}
