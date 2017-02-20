using System;
using System.Collections.Generic;
using System.Linq;
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
        public bool IsIntro { get; private set; }
        #endregion

        #region Private variables
        private GraphicsDevice device;

        private List<Zone> movementTestZones;
        private bool pickedUpItem = false;
        private bool lastStage = false;
        #endregion

        #region Public methods
        public Screen(GraphicsDevice graphicsDev, Camera camera, int width, int height, string playerName, bool isIntro) : base(width, height)
        {
            device = graphicsDev;
            this.camera = camera;
            Width = width;
            Height = height;
            IsIntro = isIntro;

            NPCs = new List<NPC>();

            CreateGraphics(graphicsDev);
            CreatePlayer(graphicsDev, playerName);
            CreateEventManagers();
            TextAnimationMgr = new EventManager<Screen>();

            items = new List<Item>();
            questZones = new List<Zone>();

            if (isIntro)
            {
                SetText("First, try moving around...\nA set of doors appeared around you. Try to reach them!", 100);
                movementTestZones = new List<Zone>();
                movementTestZones.Add(GetCloseScreenZone(graphicsDev, Color.Green));
                movementTestZones.Add(GetCloseScreenZone(graphicsDev, Color.Green));
                movementTestZones.Add(GetCloseScreenZone(graphicsDev, Color.Green));
            }
            else
            {
                SetText("Game ended.\n\nGame statistics:\nEnemies Killed: " + Game.stats.EnemiesKilled + "\nDamage Sustained: " + Game.stats.DamageSustained + "\nTimes Dead: " + Game.stats.TimesDead + "\nQuests Completed: " + Game.stats.QuestsCompleted + "\nGame Duration: " + new DateTime(DateTime.Now.Subtract(Game.stats.gameStarted).Ticks).ToString("HH:mm:ss") + "\n\nJump into the red door to continue your adventure. Pres ESC to quit the game.", 100);
                exitZone = GetCloseScreenZone(device, Color.Red);
            }
        }

        public override void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            Player.Update(gameTime, keyboardState);

            UpdateNPCs(gameTime);

            UpdateItems(gameTime);

            UpdateEvents();
            TextAnimationMgr.Update();

            if (CheckOutOfMap((int)Player.Position.Y) == -1)
            {
                //player fell down, reset player
                ResetPlayer();
            }

            if (exitZone != null && CheckZone(exitZone))
            {
                Game.Dialogue = new Dialogue(device, this, "Start your adventure now?", delegate { exitZone = null; Game.endIntro = true; }, delegate { ResetPlayer(); });
            }

            if (movementTestZones != null)
            {
                foreach (Zone z in movementTestZones)
                {
                    if (CheckZone(z))
                        z.Deactivate();
                }

                if (!movementTestZones.Any(x=> x.Active == true))
                {
                    movementTestZones = null;
                    SetText("Good.\nNow, let's try something more fun!\nThere are two action keys. One picks up / puts down items...\nwhile the other one attacks your enemies.\nFind out which one is which... and try not to die.", 100);
                    NPCs.Add(new NPC(device, this, new Vector2(400, 400), 0, tiles[0], 32, 32));
                    items.Add(new Item(this, device, Item.ItemType.Weapon, 0, new Vector2(600, 600), 0, 16, 16, true, null, new ItemProperties("ITEM")));
                }
            }

            if (Player.HeldItem != null)
                pickedUpItem = true;

            if (!lastStage && pickedUpItem && NPCs.Count == 0) //0 NPCs if item has been held means the NPC is dead
            {
                lastStage = true;
                SetText("Well done!\nYou seem ready, " + Player.Name + ".\nYou can start your adventure by jumping in the red door.\n...\nGood luck!", 100);
                exitZone = GetCloseScreenZone(device, Color.Red);
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);

            spriteBatch.DrawString(Game.font, ShownText, new Vector2(100, 100), ColorHelper.InvertColor(GetTileByIndex(Player.CurrentTile).Palette[1]));

            if (movementTestZones != null)
            {
                foreach (Zone z in movementTestZones)
                    if (z.Active)
                        z.Draw(spriteBatch);
            }

            exitZone?.Draw(spriteBatch);
        }

        public void SetText(string text, int letterInterval)
        {
            Text = text;
            TextInterval = letterInterval;
            ShownText = "";
            TextAnimationMgr.AddEvent(new Event<Screen>(letterInterval, AddLetter, this));
        }
        #endregion

        #region Private methods
        private void CreateGraphics(GraphicsDevice graphicsDevice)
        {
            tiles = new List<Tile>();
            tiles.Add(new Tile(graphicsDevice, this, (Tile.TileType)998, new Rectangle(0, 0, Game.WIDTH, Height), 0));
        }

        private void AddLetter(Screen screen)
        {
            if (screen.ShownText.Length < screen.Text.Length)
            {
                screen.ShownText = screen.Text.Substring(0, screen.ShownText.Length + 1);
                screen.TextAnimationMgr.AddEvent(new Event<Screen>(screen.TextInterval, AddLetter, this));
            }
        }

        private bool CheckZone(Zone zone)
        {
            if (!zone.Active)
                return false;

            var block = zone.Coords;
            var newPlayerRect = GeometryHelper.TileToGlobalCoordinates(Player.BoundingRectangle, GetTileByIndex(Player.CurrentTile));
            return GeometryHelper.GetIntersectionDepth(block, newPlayerRect) != Vector2.Zero;
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
