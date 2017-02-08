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
            CreateScreenExitZone(graphicsDev);

            TextAnimationMgr = new EventManager<Screen>();

            if (isIntro)
            {
                SetText("Game will start when this text finishes printing out.\nSmall lag will occur...\nOr you can jump into the red square, that works too.", 100);
            }
            else
            {
                SetText("Game ended.\nGame statistics:\nEnemies Killed: " + Game.stats.EnemiesKilled + "\nDamage Sustained: " + Game.stats.DamageSustained + "\nTimes Dead: " + Game.stats.TimesDead + "\nQuests Completed: " + Game.stats.QuestsCompleted + "\nGame Duration: " + (DateTime.Now - Game.stats.gameStarted).TotalHours + ":" + (DateTime.Now - Game.stats.gameStarted).TotalMinutes + ":" + (DateTime.Now - Game.stats.gameStarted).TotalSeconds + "\n\nResetting to be implemented.", 100);
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

            CheckExitZone();
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            List<NPC> visibleNPCs = new List<NPC>();
            tiles[0].Draw(spriteBatch);

            //Draw Tile NPCs
            foreach (NPC n in tiles[0].NPCs)
                n.Draw(gameTime, spriteBatch);

            spriteBatch.DrawString(Game.font, ShownText, new Vector2(100, 100), Color.Black);

            exitZone.Draw(spriteBatch);

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

        private void CheckExitZone()
        {
            var block = exitZone.Coords;
            var newPlayerRect = GeometryHelper.TileToGlobalCoordinates(Player.BoundingRectangle, GetTileByIndex(Player.CurrentTile));
            if (GeometryHelper.GetIntersectionDepth(block, newPlayerRect) != Vector2.Zero)
                Game.endIntro = true;
        }

        protected void CreateScreenExitZone(GraphicsDevice device)
        {
            exitZone = GetCloseScreenZone(device, Color.Red);
        }

        protected Zone GetCloseScreenZone(GraphicsDevice device, Color zoneColor)
        {
            var tile = GetTileByIndex(0);
            int xblocks = tile.Coords.Width / Block.Width;
            int yblocks = tile.Coords.Height / Block.Height;

            int selX = -1;
            int selY = -1;

            while (selX == -1) //debug: possible infinite loop, but should never happen
            {
                for (int x = 1; x < xblocks; ++x)
                {
                    for (int y = tile.GroundLevel+1; y < tile.GroundLevel+2; ++y)
                    {
                        int rand = AlgorithmHelper.GetRandom(0, (xblocks * yblocks) / 10);
                        if (rand % 12 == 0 && tile.Blocks[x, y] != null)
                        {
                            rand = AlgorithmHelper.GetRandom(0, 3);
                            if (rand == 0 && tile.Blocks[x, y - 1] == null)
                            {
                                selX = x;
                                selY = y - 1;
                            }
                            else if (rand == 1 && tile.Blocks[x, y - 2] == null)
                            {
                                selX = x;
                                selY = y - 2;
                            }
                            else if (rand == 2 && tile.Blocks[x, y - 3] == null)
                            {
                                selX = x;
                                selY = y - 3;
                            }
                            return new Zone(device, new Rectangle(tile.Coords.X + selX * Block.Width, tile.Coords.Y + selY * Block.Height, Block.Width, Block.Height), zoneColor);
                        }
                    }
                }
            }
            return null;
        }
        #endregion
    }
}
