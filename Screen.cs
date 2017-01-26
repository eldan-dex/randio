using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Randio_2
{
    class Screen : Map
    {
        #region Public variables  
        #endregion

        #region Private variables
        #endregion

        #region Public methods
        public Screen(GraphicsDevice graphicsDev, Camera camera, int width, int height) : base(width, height)
        {
            this.camera = camera;
            Width = width;
            Height = height;

            CreateGraphics(graphicsDev);

            CreatePlayer(graphicsDev);
            CreateEventManagers();
        }

        public override void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            Player.Update(gameTime, keyboardState);

            tiles[0].Update(gameTime);

            UpdateEvents();

            if (CheckOutOfMap((int)Player.Position.Y) == -1)
            {
                //player fell down, reset player
                ResetPlayer();
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            List<NPC> visibleNPCs = new List<NPC>();
            tiles[0].Draw(spriteBatch);

            //Draw Tile NPCs
            foreach (NPC n in tiles[0].NPCs)
                n.Draw(gameTime, spriteBatch);

            Player.Draw(gameTime, spriteBatch);
        }
        #endregion

        #region Private methods
        private void CreateGraphics(GraphicsDevice graphicsDevice)
        {
            tiles = new List<Tile>();
            tiles.Add(new Tile(graphicsDevice, this, (Tile.TileType)998, new Rectangle(0, 0, Game.WIDTH, Height), 0));
        }
        #endregion
    }
}
