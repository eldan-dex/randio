using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Randio_2 {
    class Map {
        #region Public variables  
        public Player Player { get; protected set; }
        public bool ReachedExit { get; protected set; }
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public int TileCount { get { return tiles.Count; } }
        //public List<NPC> NPCs { get; private set; }
        public EventManager<Entity> entityEvents;
        public QuestManager quests;
        public List<Zone> questZones;
        public Screen mapScreen; //screen for intro/outro stuff
        #endregion

        #region Private variables
        protected List<Tile> tiles;
        protected Camera camera;
        private List<Item> items;
        #endregion

        #region Public methods
        public Map(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public Map(GraphicsDevice graphicsDevice, Camera camera, int width, int height) {
            this.camera = camera;
            Width = width;
            Height = height;

            CreatePlayer(graphicsDevice);
            CreateTiles(graphicsDevice);
            CreateEventManagers();
            CreateItems(graphicsDevice);
            CreateQuests(graphicsDevice);
        }

        public virtual void Update(GameTime gameTime, KeyboardState keyboardState) {
            Player.Update(gameTime, keyboardState);
            UpdateTiles(gameTime);
            UpdateEvents();
            UpdateItems(gameTime);
            UpdateQuests();

            MoveCamera();

            if (CheckOutOfMap((int)Player.Position.Y) == -1) {
                //player fell down, reset player
                ResetPlayer();
            }
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
            List<NPC> visibleNPCs = new List<NPC>();
            foreach (Tile t in GetVisibleTiles()) {
                t.Draw(spriteBatch);

                //Draw Tile NPCs (not a part of Tile.Draw because we need to have GameTime available)
                foreach (NPC n in t.NPCs)
                    visibleNPCs.Add(n);
            }

            foreach (Zone z in questZones)
            {
                z.Draw(spriteBatch);
            }

            foreach (NPC n in visibleNPCs)
                n.Draw(gameTime, spriteBatch);

            Player.Draw(gameTime, spriteBatch);

            foreach (Item i in items)
                i.Draw(gameTime, spriteBatch);
        }

        //Check where given Y coordinate lies relative to the map
        public int CheckOutOfMap(int y) {
            if (y < 0) //y is above the map
                return 1;

            else if (y > Height) //y is below the map
                return -1;

            else return 0; //y is within the map
        }

        //returns a tile which contains the given X coordinate
        public Tile GetTileForX(int x) => tiles.FirstOrDefault(t => t.Coords.Left <= x && t.Coords.Right > x);

        //returns tile with the corresponding index
        public Tile GetTileByIndex(int index) {
            if (index >= 0 && index < tiles.Count)
                return tiles[index];
            return null;
        }

        public void ResetPlayer()
        {
            Player.Reset();
            CameraToPlayer();
        }

        //translate global position into an offset from the left boundary of the given tile
        public Vector2 GlobalToTileCoordinates(Vector2 global, int tileIndex)
        {
            var tile = GetTileByIndex(tileIndex);

            if (tile == null)
                throw new NotSupportedException("Invalid tileIndex value!");

            return new Vector2(global.X - tile.Coords.X, global.Y);
        }

        public List<Entity> GetAllEntites() {
            List<Entity> result = new List<Entity>();

            result.Add(Player);
            foreach (Tile t in tiles)
                result.AddRange(t.NPCs);

            return result;
        }

        //returns only items placed on ground (ignores those held by entities)
        public List<Item> GetAllItems()
        {
            var result = new List<Item>();

            foreach (Item i in items)
                if (i.IsPlaced)
                    result.Add(i);

            return result;
        }
        #endregion

        #region Private methods
        protected void CreatePlayer(GraphicsDevice graphicsDevice) {
            Player = new Player(graphicsDevice, this, Vector2.Zero); //generate position
            CameraToPlayer();
        }

        protected void CameraToPlayer() {
            camera.CenterXTo(new Rectangle((int)Player.Position.X, (int)Player.Position.Y, Player.Width, Player.Height));
        }

        protected void MoveCamera() {
            float leftEdge = camera.Position.X;
            float rightEdge = leftEdge + Game.WIDTH;

            //TODO: nastavit kamere pevne X souradnice podle vzdalenosti hrace od okraje

            if (Player.Position.X - leftEdge < Player.SafeMargin) {
                //move camera left if possible
                if (camera.Position.X > 0)
                    camera.Position = new Vector2(Player.Position.X - Player.SafeMargin, 0);
            }
            else if (rightEdge - (Player.Position.X + Player.Width) < Player.SafeMargin) {
                //move camera right is possible
                if (camera.Position.X < Width)
                    camera.Position = new Vector2(Player.Position.X + Player.Width + Player.SafeMargin - Game.WIDTH, 0);
            }
        }

        private void CreateTiles(GraphicsDevice graphicsDevice) {
            if (Width % Block.Width > 0 || Height % Block.Height > 0)
                throw new ArgumentException("Map dimensions must be divisible by Block dimensions!");

            tiles = new List<Tile>();

            //Temporary algorithm, will be upgraded
            int minWidth = Game.WIDTH;
            int maxWidth = 3 * Game.WIDTH;
            int totalWidth = 0;
            int tileIndex = 0;

            while (totalWidth < Width) {
                //Generate a random width for the next tile, but keep it divisible by Block.Width
                int newWidth = AlgorithmHelper.GetRandom(minWidth / Block.Width, maxWidth / Block.Width) * Block.Width;

                //If last tile wouldn't be able to fit, extend this one instead
                int testWidth = totalWidth + newWidth;
                if (Width - testWidth < minWidth) {
                    //Don't make the last tile too big, otherwise MonoGame might crash
                    if (Width - totalWidth <= 4096)
                        newWidth = Width - totalWidth;
                    else
                        newWidth = 4096; //This breaks the purprose of the whole algorithm, because the next tile might be too small. But it fixes the crashing.
                }

                //Generate TileType
                var type = (Tile.TileType)AlgorithmHelper.GetRandom(0, Tile.TileTypeCount);

                //Create and add Tile
                tiles.Add(new Tile(graphicsDevice, this, type, new Rectangle(totalWidth, 0, newWidth, Height), tileIndex));

                totalWidth += newWidth;
                ++tileIndex;
            }
        }

        protected void CreateEventManagers()
        {
            entityEvents = new EventManager<Entity>();
        }

        private void CreateQuests(GraphicsDevice device)
        {
            quests = new QuestManager(this);
            questZones = new List<Zone>();

            int count = AlgorithmHelper.GetRandom(1, 5);
            for (int i = 0; i < count; ++i)
            {
                Quest quest;
                Quest.QuestType type = (Quest.QuestType)AlgorithmHelper.GetRandom(0, Quest.QuestTypeCount);

                string name = "";
                string description = "";
                List<Entity> targets = null;
                List<Item> itemFetchList = null;
                List<Zone> zones = null;

                if (type == Quest.QuestType.KillTargets)
                {
                    name = "Kill ";
                    targets = new List<Entity>();
                    var entities = GetAllEntites();
                    int enemyCount = AlgorithmHelper.GetRandom(1, 4); //todo: balance
                    for (int j = 0; j < enemyCount; ++j)
                    {
                        var target = entities[AlgorithmHelper.GetRandom(0, entities.Count)];
                        targets.Add(target);

                        //Append target name to quest name
                        name += target.Name;
                        if (j < enemyCount - 1)
                            name += ", ";
                    }
                }

                else if (type == Quest.QuestType.FetchIitems)
                {
                    name = "Bring ";
                    itemFetchList = new List<Item>();
                    zones = new List<Zone>();
                    int itemCount = AlgorithmHelper.GetRandom(1, 4); //todo: balance

                    var newZone = GetNewZone(device, Color.Green);
                    zones.Add(newZone);
                    questZones.Add(newZone);

                    for (int j = 0; j < itemCount; ++j)
                    {
                        var item = items[AlgorithmHelper.GetRandom(0, items.Count)];

                        while (itemFetchList.Contains(item))
                        {
                            if (itemFetchList.Count == itemCount)
                            {
                                j = itemCount;
                                break;
                            }
                            item = items[AlgorithmHelper.GetRandom(0, items.Count)];
                        }
                        
                        itemFetchList.Add(item);

                        //Append target name to quest name
                        name += item.Properties.Name;
                        if (j < itemCount - 1)
                            name += ", ";
                    }

                    name += " to " + "the green area."; //todo: improve destination marking - all "green areas" look the same atm
                }

                else if (type == Quest.QuestType.ReachBlock)
                {
                    name = "Reach areas with these colors: ";
                    zones = new List<Zone>();
                    int pointCount = AlgorithmHelper.GetRandom(1, 4); //todo: balance
                    for (int j = 0; j < pointCount; ++j)
                    {
                        var newZone = GetNewZone(device, Color.Orange);
                        zones.Add(newZone);
                        questZones.Add(newZone);

                        name += "Orange";
                        if (j < pointCount - 1)
                            name += ", ";
                    }
                }

                quests.AddQuest(new Quest(this, type, name, description, targets, itemFetchList, zones));
            }
        }

        protected Zone GetNewZone(GraphicsDevice device, Color zoneColor)
        {
            Tile tile = tiles[AlgorithmHelper.BiasedRandom(0, tiles.Count-1, 1.3)];
            int xblocks = tile.Coords.Width / Block.Width;
            int yblocks = tile.Coords.Height / Block.Height;

            List<int> valX = new List<int>();
            List<int> valY = new List<int>();

            for (int x = 1; x < xblocks; ++x)
            {
                for (int y = 1; y < tile.GroundLevel; ++y)
                {
                    int rand = AlgorithmHelper.GetRandom(0, (xblocks * yblocks) / 10);
                    if (rand % 42 == 0 && tile.Blocks[x, y] != null)
                    {
                        rand = AlgorithmHelper.GetRandom(0, 5);
                        if (rand == 1 && tile.Blocks[x, y - 1] == null && (tile.Blocks[x - 1, y - 1] == null || tile.Blocks [x + 1, y - 1] == null))
                        {
                            valX.Add(x);
                            valY.Add(y - 1);
                        }
                        else if (rand == 2 && tile.Blocks[x, y + 1] == null && (tile.Blocks[x - 1, y + 1] == null || tile.Blocks[x + 1, y + 1] == null))
                        {
                            valX.Add(x);
                            valY.Add(y + 1);
                        }
                        else if (rand == 3 && tile.Blocks[x - 1, y] == null && tile.Blocks[x - 2, y] == null)
                        {
                            valX.Add(x - 1);
                            valY.Add(y);
                        }
                        else if (rand == 4 && tile.Blocks[x + 1, y] == null && tile.Blocks[x + 2, y] == null)
                        {
                            valX.Add(x + 1);
                            valY.Add(y);
                        }
                    }
                }
            }

            //todo: dangerous, possible infinite loop. although very improbable
            if (valX.Count == 0)
                return GetNewZone(device, zoneColor);

            int rnd = AlgorithmHelper.GetRandom(0, valX.Count);
            return new Zone(device, new Rectangle(tile.Coords.X + valX[rnd] * Block.Width, tile.Coords.Y + valY[rnd] * Block.Height, Block.Width, Block.Height), zoneColor);
        }

        private void CreateItems(GraphicsDevice device)
        {
            //just for testing
            items = new List<Item>();
            items.Add(new Item(this, device, Item.ItemType.Flop, new Vector2(100, 550), 0, 16, 16));
            items.Add(new Item(this, device, Item.ItemType.Armor, new Vector2(300, 450), 0, 16, 16));
            items.Add(new Item(this, device, Item.ItemType.Weapon, new Vector2(700, 500), 0, 16, 16, properties: new ItemProperties("TEST WEAPON NAME", strength: 10)));
        }

        //Updates Tiles and NPCs on them
        private void UpdateTiles(GameTime gameTime) {
            var visibleTiles = GetVisibleTiles();
            foreach (Tile tile in visibleTiles)
                tile.Update(gameTime);
        }

        protected void UpdateEvents()
        {
            entityEvents.Update();
        }

        protected void UpdateItems(GameTime gameTime)
        {
            foreach (Item i in items)
                i.Update(gameTime);
        }

        protected void UpdateQuests()
        {
            quests.Update();
        }

        private Tile[] GetVisibleTiles() {
            List<Tile> visibleTiles = new List<Tile>();

            var pX = Player.Position.X;
            if (pX < 0)
                pX = 0;

            var leftBound = pX - Game.WIDTH + Player.SafeMargin;
            var rightBound = pX + Player.Width + Game.WIDTH - Player.SafeMargin;

            //Bounds cannot be outside the map
            if (leftBound < 0)
                leftBound = 0;
            if (rightBound > Width)
                rightBound = Width;

            //Add unique tiles on the right and left end of screen (implying player is centered)
            var tile = GetTileForX((int)leftBound);
            if (tile != null)
                visibleTiles.Add(tile);

            Tile current = GetTileForX((int)pX);
            if (!visibleTiles.Contains(current))
                visibleTiles.Add(current);

            Tile other = GetTileForX((int)rightBound);

            //hackaround, because GetTileForX returns null on the last coordinate
            if (rightBound == Width)
                other = GetTileByIndex(tiles.Count - 1);

            if (!visibleTiles.Contains(other))
                visibleTiles.Add(other);

            return visibleTiles.ToArray();
        }
        #endregion
    }
}
