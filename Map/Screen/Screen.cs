using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Randio_2
{
    class Screen : Map
    {
        #region Public variables  
        public string Text { get; private set; }
        public EventManager<Screen> TextAnimationMgr { get; private set; }
        public string ShownText = ""; //publicly editable because of Event requirements
        public int TextInterval { get; private set; }
        #endregion

        #region Private variables
        private bool isIntro;
        #endregion

        #region Public methods
        public Screen(GraphicsDevice graphicsDev, Camera camera, int width, int height, string playerName, bool isIntro) : base(width, height)
        {
            this.camera = camera;
            Width = width;
            Height = height;
            this.isIntro = isIntro;

            CreateGraphics(graphicsDev);

            CreatePlayer(graphicsDev, playerName);
            CreateEventManagers();

            TextAnimationMgr = new EventManager<Screen>();

            if (isIntro)
            {
                SetText("Lorem ipsum dolor sit amet.\nGame will start when this text finishes printing out.\nSmall lag will occur...", 100);
            }
            else
            {
                SetText("Game ended.\nThis is a placeholder for displaying game statistics.\nReset the game by restarting the program (I have to implement resetting yet).", 100);
            }
        }

        public override void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            Player.Update(gameTime, keyboardState);

            tiles[0].Update(gameTime);

            UpdateEvents();
            TextAnimationMgr.Update();

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

            spriteBatch.DrawString(Game.font, ShownText, new Vector2(100, 100), Color.Black);

            Player.Draw(gameTime, spriteBatch);
        }

        public void SetText(string text, int letterInterval)
        {
            Text = text;
            TextInterval = letterInterval;
            ShownText = "";
            TextAnimationMgr.AddEvent(new Event<Screen>(letterInterval, AddLetterA, this));
        }
        #endregion

        #region Private methods
        private void CreateGraphics(GraphicsDevice graphicsDevice)
        {
            tiles = new List<Tile>();
            tiles.Add(new Tile(graphicsDevice, this, (Tile.TileType)998, new Rectangle(0, 0, Game.WIDTH, Height), 0));
        }

        private void AddLetterA(Screen screen)
        {
            if (screen.ShownText.Length < screen.Text.Length)
            {
                screen.ShownText = screen.Text.Substring(0, screen.ShownText.Length + 1);
                screen.TextAnimationMgr.AddEvent(new Event<Screen>(screen.TextInterval, AddLetterA, this));
            }
            else if (isIntro) //debug: this will not be here in the final version - game will start when a "portal" zone appears and is entered by player
            {
                Game.endIntro = true; //end the intro screen and start game itself
            }
        }
        #endregion
    }
}
