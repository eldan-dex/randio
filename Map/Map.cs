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
        public bool ReachedExit { get; set; }
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public int TileCount { get { return (tiles==null) ? 1 : tiles.Count; } }
        public List<NPC> NPCs { get; protected set; }

        public EventManager<Entity> entityEvents;
        public QuestManager quests;
        public List<Zone> questZones;
        public Zone exitZone;
        public Screen mapScreen; //screen for intro/outro stuff
        #endregion

        #region Private variables
        protected List<Tile> tiles;
        protected Camera camera;
        protected List<Item> items;
        #endregion

        #region Public methods
        public Map(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public Map(GraphicsDevice graphicsDevice, Camera camera, int width, int height, string playerName) {
            this.camera = camera;
            Width = width;
            Height = height;

            NPCs = new List<NPC>();

            CreatePlayer(graphicsDevice, playerName);
            CreateTiles(graphicsDevice);
            CreateEventManagers();
            CreateItems(graphicsDevice);
            CreateQuests(graphicsDevice);
            CreateExitZone(graphicsDevice);
        }

        public virtual void Update(GameTime gameTime, KeyboardState keyboardState) {
            Player.Update(gameTime, keyboardState);
            UpdateNPCs(gameTime);
            UpdateEvents();
            UpdateItems(gameTime);
            UpdateQuests();
            CheckExitZone();

            MoveCamera();

            if (CheckOutOfMap((int)Player.Position.Y) == -1) {
                //player fell down, reset player
                ResetPlayer();
            }
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
            List<NPC> visibleNPCs = new List<NPC>();
            foreach (Tile t in GetVisibleTiles())
                t.Draw(spriteBatch);

            foreach (Zone z in questZones)
            {
                z.Draw(spriteBatch);
            }

            exitZone?.Draw(spriteBatch);

            foreach (NPC n in GetVisibleNPCs())
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
            result.AddRange(NPCs);

            return result;
        }

        //returns only items placed on ground (ignores those held by entities)
        public List<Item> GetAllItems()
        {
            var result = new List<Item>();

            if (items != null)
            {
                foreach (Item i in items)
                    if (i.IsPlaced)
                        result.Add(i);
            }

            return result;
        }

        public bool IsBlock(Vector2 globalCoords)
        {
            int x = (int)globalCoords.X;
            if (x < 0)
                x = 0;

            var tile = GetTileForX(x);

            if (tile == null)
                return false;

            var tileCoords = GlobalToTileCoordinates(new Vector2(x, globalCoords.Y), tile.Index);
            int tileIndexX = Math.Max(0, (int)Math.Round(tileCoords.X / Block.Width));
            int tileIndexY = Math.Max(0, (int)Math.Round(tileCoords.Y / Block.Height));

            if (tileIndexX >= tile.Blocks.GetLength(0) || tileIndexY >= tile.Blocks.GetLength(1)) //hackaround
                return false;

            return tile.Blocks[tileIndexX, tileIndexY] != null;
        }
        #endregion

        #region Private methods
        protected void CreatePlayer(GraphicsDevice graphicsDevice, string name) {
            Player = new Player(graphicsDevice, this, Vector2.Zero, name); //generate position
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
                if (camera.Position.X < Width-Game.WIDTH)
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
            int nextType = -1;

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
                //Generate two similar tiles next to each other
                Tile.TileType type;
                if (nextType == -1)
                {
                    type = (Tile.TileType)AlgorithmHelper.GetRandom(0, Tile.TileTypeCount);
                    nextType = (int)type;
                }
                else
                {
                    type = (Tile.TileType)nextType;
                    nextType = -1;
                }

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

            int count = AlgorithmHelper.GetRandom(1, 4);
            List<Quest.QuestType> usedTypes = new List<Quest.QuestType>();
            for (int i = 0; i < count; ++i)
            {
                Quest.QuestType type;
                do
                    type = (Quest.QuestType)AlgorithmHelper.GetRandom(0, Quest.QuestTypeCount);
                while (usedTypes.Contains(type));
                usedTypes.Add(type);

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
                    int enemyCount = AlgorithmHelper.GetRandom(1, 5);
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
                    int itemCount = AlgorithmHelper.GetRandom(1, 4); //so that item names would fit into view -.-

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

                    name += " to " + "a green area in tile " + GetTileForX(newZone.Coords.X).Index.ToString();
                }

                else if (type == Quest.QuestType.ReachBlock)
                {
                    name = "Reach orange areas located in these directions: ";
                    zones = new List<Zone>();
                    int pointCount = AlgorithmHelper.GetRandom(1, 5); //todo: balance
                    for (int j = 0; j < pointCount; ++j)
                    {
                        var newZone = GetNewZone(device, Color.Orange);
                        zones.Add(newZone);
                        questZones.Add(newZone);

                        name += GetTileForX(newZone.Coords.X).Index.ToString();
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
                        if (rand == 1 && tile.Blocks[x, y - 1] == null && tile.Blocks[x, y] != null && (tile.Blocks[x - 1, y - 1] == null || tile.Blocks [x + 1, y - 1] == null))
                        {
                            valX.Add(x);
                            valY.Add(y - 1);
                        }
                        else if (rand == 2 && tile.Blocks[x, y + 1] == null && tile.Blocks[x, y + 2] != null && (tile.Blocks[x - 1, y + 1] == null || tile.Blocks[x + 1, y + 1] == null))
                        {
                            valX.Add(x);
                            valY.Add(y + 1);
                        }
                        else if (rand == 3 && tile.Blocks[x - 1, y] == null && tile.Blocks[x - 1, y + 1] != null && tile.Blocks[x - 2, y] == null)
                        {
                            valX.Add(x - 1);
                            valY.Add(y);
                        }
                        else if (rand == 4 && tile.Blocks[x + 1, y] == null && tile.Blocks[x + 1, y + 1] != null && tile.Blocks[x + 2, y] == null)
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

        protected Zone GetCloseZone(GraphicsDevice device, Color zoneColor)
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
                    for (int y = 1; y < tile.GroundLevel; ++y)
                    {
                        int rand = AlgorithmHelper.GetRandom(0, (xblocks * yblocks) / 10);
                        if (rand % 42 == 0 && tile.Blocks[x, y] != null)
                        {
                            rand = AlgorithmHelper.GetRandom(0, 5);
                            if (rand == 1 && tile.Blocks[x, y - 1] == null && (tile.Blocks[x - 1, y - 1] == null || tile.Blocks[x + 1, y - 1] == null))
                            {
                                selX = x;
                                selY = y - 1;
                            }
                            else if (rand == 2 && tile.Blocks[x, y + 1] == null && (tile.Blocks[x - 1, y + 1] == null || tile.Blocks[x + 1, y + 1] == null))
                            {
                                selX = x;
                                selY = y + 1;
                            }
                            else if (rand == 3 && tile.Blocks[x - 1, y] == null && tile.Blocks[x - 2, y] == null)
                            {
                                selX = x - 1;
                                selY = y;
                            }
                            else if (rand == 4 && tile.Blocks[x + 1, y] == null && tile.Blocks[x + 2, y] == null)
                            {
                                selX = x + 1;
                                selY = y;
                            }
                        }
                    }
                }
            }

            return new Zone(device, new Rectangle(tile.Coords.X + selX * Block.Width, tile.Coords.Y + selY * Block.Height, Block.Width, Block.Height), zoneColor);
        }

        private void CreateItems(GraphicsDevice device)
        {
            items = new List<Item>();
            int count = AlgorithmHelper.GetRandom(1, 2 * TileCount); //how many items there will be in the game
            int nextX = AlgorithmHelper.GetRandom(0, Width / count);
            for (int i = 0; i < count; ++i)
            {
                //set nextY by looking for a space without any tiles
                var tile = GetTileForX(nextX);
                var tileX = (int)GlobalToTileCoordinates(new Vector2(nextX, 0), tile.Index).X;
                int tileIndexX = tileX / Block.Width; //tweak this, might not be accurate
                int tileBase = tile.Coords.Left;
                List<int> availableY = new List<int>();
                for(int y = 0; y < tile.GroundLevel; ++y)
                {
                    if (tile.Blocks[tileIndexX, y] == null)
                        availableY.Add(y);  
                }
                if (availableY.Count > 0)
                {
                    int selectedY = availableY[AlgorithmHelper.GetRandom(0, availableY.Count)];
                    Item.ItemType type = (Item.ItemType)AlgorithmHelper.GetRandom(0, Item.TypeCount);

                    Item item = new Item(this, device, type, i, new Vector2(tileBase + tileIndexX*Block.Width, selectedY*Block.Height), tile.Index, 16, 16);
                    items.Add(item);
                }

                nextX += AlgorithmHelper.GetRandom(16, Width / count);
            }
        }

        protected void CreateExitZone(GraphicsDevice device)
        {
            exitZone = GetCloseZone(device, Color.Red);
        }

        //Updates Tiles and NPCs on them
        protected void UpdateNPCs(GameTime gameTime) {
            List<NPC> toRemove = new List<NPC>();

            foreach (NPC n in NPCs) { 
                n.Update(gameTime);
                if (CheckOutOfMap((int)n.Position.Y) == -1 || !n.Alive)
                    toRemove.Add(n);
            }

            foreach (NPC n in toRemove)
            {
                if (NPCs.Contains(n))
                    NPCs.Remove(n);
            }
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

        //If player reached the exit zone, exit game and show outro screen
        private void CheckExitZone()
        {
            var block = exitZone.Coords;
            var newPlayerRect = GeometryHelper.TileToGlobalCoordinates(Player.BoundingRectangle, GetTileByIndex(Player.CurrentTile));
            if (GeometryHelper.GetIntersectionDepth(block, newPlayerRect) != Vector2.Zero)
                ReachedExit = true;
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

        private NPC[] GetVisibleNPCs()
        {
            var visibleTiles = GetVisibleTiles();
            var result = new List<NPC>();

            foreach (NPC n in NPCs)
            {
                if (n.Alive)
                {
                    foreach (Tile visible in visibleTiles)
                    {
                        if (visible.Index == n.CurrentTile)
                            result.Add(n);
                    }
                }
            }
            
            return result.ToArray();
        }
        #endregion
    }
}
