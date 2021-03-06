﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Randio_2
{
    //Intro/Outro screen - based on Map but with animated text, custom map objects and mechanics
    class Screen : Map
    {
        #region Public variables  
        public string Text { get; private set; }
        public EventManager<Screen> TextAnimationMgr { get; private set; }
        public int TextInterval { get; private set; }
        public bool IsIntro { get; private set; }
        public string ShownText = ""; //publicly editable because of Event requirements
        #endregion

        #region Private variables
        private GraphicsDevice device;
        private List<Zone> movementTestZones;
        private bool pickedUpItem = false;
        private bool lastStage = false;
        #endregion

        #region Public methods
        //Default ctor
        public Screen(GraphicsDevice graphicsDev, Camera camera, int width, int height, string playerName, bool isIntro) : base(width, height)
        {
            this.camera = camera;
            device = graphicsDev;   
            Width = width;
            Height = height;
            IsIntro = isIntro;

            CreateGraphics(graphicsDev);
            CreatePlayer(graphicsDev, playerName);

            EntityEvents = new EventManager<Entity>();
            TextAnimationMgr = new EventManager<Screen>();
            NPCs = new List<NPC>();
            items = new List<Item>();
            questZones = new List<Zone>();

            if (isIntro)
            {
                SetText("Hello and welcome to RANDIO!\nThis is a game based on randomness and procedural generation.\nEverything you see here is generated procedurally with some degree of\nrandomness.\n\nYou will now learn how to play the game.\nFirst, try to reach all the green doors, that appeared around you.", 50);
                movementTestZones = new List<Zone>();
                movementTestZones.Add(GetScreenZone(graphicsDev, Color.Green));
                movementTestZones.Add(GetScreenZone(graphicsDev, Color.Green));
                movementTestZones.Add(GetScreenZone(graphicsDev, Color.Green));
            }
            else
            {
                SetText("Game ended.\n\nGame statistics:\nEnemies Killed: " + Game.stats.EnemiesKilled + "\nDamage Sustained: " + Game.stats.DamageSustained + "\nTimes Dead: " + Game.stats.TimesDead + "\nQuests Completed: " + Game.stats.QuestsCompleted + "\nGame Duration: " + new DateTime(DateTime.Now.Subtract(Game.stats.gameStarted).Ticks).ToString("HH:mm:ss") + "\n\nJump into the red door to continue your adventure.\nPres ESC to quit the game entirely.", 50);
                exitZone = GetScreenZone(device, Color.Red);
            }
        }

        //Like Map.Update, updates player, NPCs, items, events and displayed text. Also advances the intro/outro as needed and checks for task completion
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

            //Player entered the "red door"
            if (exitZone != null && CheckZone(exitZone))
            {
                Game.Dialogue = new Dialogue(device, this, "Start your adventure now?", delegate { exitZone = null; Game.endIntro = true; Game.Loading = new Loading(device, this, "Game is now loading..."); }, delegate { ResetPlayer(); });
            }

            //Player is trying to reach all "green doors"
            if (movementTestZones != null)
            {
                foreach (Zone z in movementTestZones)
                {
                    if (CheckZone(z))
                        z.Deactivate();
                }

                //Success, create next task
                if (!movementTestZones.Any(x=> x.Active == true))
                {
                    movementTestZones = null;
                    SetText("Good.\nNow there is an Item and an NPC (which can move around and attack you).\nUse your action keys to pick the item up (K) and attack the NPC (J)!", 50);
                    NPCs.Add(new NPC(device, this, new Vector2(400, 400), 0, tiles[0], 32, 32));
                    items.Add(new Item(this, device, Item.ItemType.Weapon, 0, new Vector2(600, 600), 0, 16, 16, true, null, new ItemProperties("ITEM")));
                }
            }

            if (Player.HeldItem != null)
                pickedUpItem = true;

            //Item picked up and NPC is dead, show red door and last part of text
            if (!lastStage && pickedUpItem && NPCs.Count == 0) //0 NPCs if item has been held means that the NPC is dead
            {
                lastStage = true;
                SetText("Well done!\nYou seem ready, " + Player.Name + ".\nYour objective in the game is to complete all the quests.\nIf you can't or don't want to, press ESC to end the current map and see\ngame statistics.\n\nTo start the game, jump through the red door.", 50);
                exitZone = GetScreenZone(device, Color.Red);
            }
        }

        //Calls Map.Draw and then draws text + green and red doors
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

        //Set text to be displayed with interval between printing new letters
        public void SetText(string text, int letterInterval)
        {
            Text = text;
            TextInterval = letterInterval;
            ShownText = "";
            TextAnimationMgr.AddEvent(new Event<Screen>(letterInterval, AddLetter, this));
        }
        #endregion

        #region Private methods
        //Initializes a new Screen tile
        private void CreateGraphics(GraphicsDevice graphicsDevice)
        {
            tiles = new List<Tile>();
            tiles.Add(new Tile(graphicsDevice, this, (Tile.TileType)998, new Rectangle(0, 0, Game.WIDTH, Height), 0));
        }

        //Displays next available letter and adds an event to do this again after an interval has passed
        private void AddLetter(Screen screen)
        {
            if (screen.ShownText.Length < screen.Text.Length)
            {
                screen.ShownText = screen.Text.Substring(0, screen.ShownText.Length + 1);
                screen.TextAnimationMgr.AddEvent(new Event<Screen>(screen.TextInterval, AddLetter, this));
            }
        }

        //Check player-zone collision
        private bool CheckZone(Zone zone)
        {
            if (!zone.Active)
                return false;

            var block = zone.Coords;
            var newPlayerRect = TileToGlobalCoordinates(Player.BoundingRectangle, GetTileByIndex(Player.CurrentTile));
            return GeometryHelper.GetIntersectionDepth(block, newPlayerRect) != Vector2.Zero;
        }

        //Creates a zone anywhere in the current tile
        private Zone GetScreenZone(GraphicsDevice device, Color zoneColor)
        {
            var tile = GetTileByIndex(0);
            int xblocks = tile.Coords.Width / Block.Width;
            int yblocks = tile.Coords.Height / Block.Height;

            int selX = -1;
            int selY = -1;

            while (selX == -1)
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
