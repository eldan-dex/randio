using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Randio_2 {
    //Game map - contains player, NPCs, individual tiles with backgrounds and blocks, items and everyting else seen in the game
    class Map {
        #region Public variables  
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public int TileCount { get { return (tiles == null) ? 1 : tiles.Count; } }
        public bool ReachedExit { get; set; }
        public Player Player { get; protected set; }  
        public List<NPC> NPCs { get; protected set; }
        public EventManager<Entity> EntityEvents { get; protected set; }
        public QuestManager Quests { get; protected set; }
        #endregion

        #region Protected variables
        protected List<Tile> tiles;
        protected Camera camera;
        protected List<Item> items;
        protected Zone exitZone;
        protected List<Zone> questZones;
        #endregion

        #region Public methods
        //Default ctor, used when inherited by Screen
        public Map(int width, int height)
        {
            Width = width;
            Height = height;
        }

        //Standard ctor used when creating a regular Map instance
        public Map(GraphicsDevice graphicsDevice, Camera camera, int width, int height, string playerName) {
            this.camera = camera;
            Width = width;
            Height = height;

            NPCs = new List<NPC>();
            EntityEvents = new EventManager<Entity>();

            CreatePlayer(graphicsDevice, playerName);
            CreateTiles(graphicsDevice);
            CreateItems(graphicsDevice);
            CreateQuests(graphicsDevice);

            //Game is now loaded, disable the loading screen
            if (Game.Loading != null)
                Game.Loading = null;
        }

        //Updates player, NPCs, Events, Items, Quests and moves the camera
        public virtual void Update(GameTime gameTime, KeyboardState keyboardState) {
            Player.Update(gameTime, keyboardState);
            UpdateNPCs(gameTime);
            UpdateEvents();
            UpdateItems(gameTime);
            UpdateQuests();

            MoveCamera();

            //Reset player if player left the game area (fell down, etc)
            if (CheckOutOfMap((int)Player.Position.Y) == -1) {
                ResetPlayer();
            }
        }

        //Draws all tiles, quest zones, NPCs, player, items and exit zones (only in intro/outro)
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
            foreach (Tile t in GetVisibleTiles())
                t.Draw(spriteBatch);

            foreach (Zone z in questZones)
                z.Draw(spriteBatch);

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

        //Returns a tile which contains the given X coordinate
        public Tile GetTileForX(int x)
            => tiles.FirstOrDefault(t => t.Coords.Left <= x && t.Coords.Right > x);

        //Returns a tile with the corresponding index
        public Tile GetTileByIndex(int index) {
            if (index >= 0 && index < tiles.Count)
                return tiles[index];
            return null;
        }

        //Resets the player and camera to the start of the current tile
        public void ResetPlayer()
        {
            Player.Reset();
            CameraToPlayer();
        }

        //Translate the global position into an offset from the left boundary of the given tile
        public Vector2 GlobalToTileCoordinates(Vector2 global, int tileIndex)
        {
            var tile = GetTileByIndex(tileIndex);

            if (tile == null)
                throw new NotSupportedException("Invalid tileIndex value!");

            return new Vector2(global.X - tile.Coords.X, global.Y);
        }

        //Translate tile coordinates to global coordinates
        public Rectangle TileToGlobalCoordinates(Rectangle tileCoords, Tile tile)
        {
            if (tileCoords == null || tile == null)
                return new Rectangle(); //exception?
            return new Rectangle(tileCoords.X + tile.Coords.X, tileCoords.Y + tile.Coords.Y, tileCoords.Width, tileCoords.Height);
        }


        //Returns all NPCs and Player
        public List<Entity> GetAllEntites() {
            List<Entity> result = new List<Entity>();

            //result.Add(Player);
            result.AddRange(NPCs);

            return result;
        }

        //Returns items placed on ground (ignores those held by entities)
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
    
        //Returns true when a block is present on given coordinates
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
        //Initializes player and resets camera to the proper spot
        protected void CreatePlayer(GraphicsDevice graphicsDevice, string name) {
            Player = new Player(graphicsDevice, this, Vector2.Zero, name);
            CameraToPlayer();
        }

        //Centers the camera to the player
        protected void CameraToPlayer() {
            camera.CenterXTo(new Rectangle((int)Player.Position.X, (int)Player.Position.Y, Player.Width, Player.Height));
        }

        //Camera "follows" the player - when the player is within some given distance from an edge of the screen, camera moves to keep him in frame
        protected void MoveCamera() {
            float leftEdge = camera.Position.X;
            float rightEdge = leftEdge + Game.WIDTH;

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

        //Initializes all tiles we see ingame
        private void CreateTiles(GraphicsDevice graphicsDevice) {
            tiles = new List<Tile>();

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

                //Picka  random TileType
                var type = (Tile.TileType)AlgorithmHelper.GetRandom(0, Tile.TileTypeCount);

                //Create and add Tile
                tiles.Add(new Tile(graphicsDevice, this, type, new Rectangle(totalWidth, 0, newWidth, Height), tileIndex));

                totalWidth += newWidth;
                ++tileIndex;
            }
        }

        //Initializes quests
        private void CreateQuests(GraphicsDevice device)
        {
            Quests = new QuestManager(this);
            questZones = new List<Zone>();

            //How many quests will this level have (1-3)
            int count = AlgorithmHelper.GetRandom(1, 4);
            List<Quest.QuestType> usedTypes = new List<Quest.QuestType>();
            for (int i = 0; i < count; ++i)
            {
                //Only add one of each quest type
                Quest.QuestType type;
                do
                    type = (Quest.QuestType)AlgorithmHelper.GetRandom(0, Quest.QuestTypeCount);
                while (usedTypes.Contains(type));
                usedTypes.Add(type);

                string name = "";
                List<Entity> targets = null;
                List<Item> itemFetchList = null;
                List<Zone> zones = null;

                //Kill quest - picks 1-4 NPCs and player's task is to kill them all
                if (type == Quest.QuestType.KillTargets)
                {
                    name = "Kill ";
                    targets = new List<Entity>();
                    var entities = GetAllEntites();
                    int enemyCount = AlgorithmHelper.GetRandom(1, Math.Min(5, entities.Count));
                    for (int j = 0; j < enemyCount; ++j)
                    {
                        Entity target;
                        do
                            target = entities[AlgorithmHelper.GetRandom(0, entities.Count)];
                        while (targets.Contains(target));

                        targets.Add(target);
                    }
                }

                //Fetch quest - picks 1 or 2 items which player has to put down inside a specific area
                else if (type == Quest.QuestType.FetchIitems)
                {
                    name = "Bring ";
                    itemFetchList = new List<Item>();
                    zones = new List<Zone>();
                    int itemCount = AlgorithmHelper.GetRandom(1, Math.Min(3, items.Count)); //so that item names would fit into view -.-
                    var newZone = GetNewZone(device, Color.Green);
                    zones.Add(newZone);
                    questZones.Add(newZone);

                    for (int j = 0; j < itemCount; ++j)
                    {
                        Item item;
                        do
                            item = items[AlgorithmHelper.GetRandom(0, items.Count)];
                        while (itemFetchList.Contains(item));
                        
                        itemFetchList.Add(item);
                    }
                }

                //Reach quest - randomly select 1 - 4 empty blocks and task the player with reaching them
                else if (type == Quest.QuestType.ReachBlock)
                {
                    name = "Reach orange areas located in these tiles: ";
                    zones = new List<Zone>();
                    int pointCount = AlgorithmHelper.GetRandom(1, 5);
                    for (int j = 0; j < pointCount; ++j)
                    {
                        var newZone = GetNewZone(device, Color.Orange);
                        zones.Add(newZone);
                        questZones.Add(newZone);
                    }
                }

                Quests.AddQuest(new Quest(this, type, name, targets, itemFetchList, zones));
            }

            CreateQuestBackground(device, usedTypes);
        }

        //Computes proper size for the half-transparent white background that makes quest text more readable
        protected void CreateQuestBackground(GraphicsDevice device, List<Quest.QuestType> usedTypes)
        {
            int longest = 0;
            Quests.Update();
            foreach (Quest q in Quests.Quests)
            {
                if (q.Name.Length > longest)
                    longest = q.Name.Length;
            }
            Quests.Background = new Texture2D(device, longest * 18 + 20, Quests.Quests.Count * 20 + 3);
        }


        //Returns a new Zone placed randomly on the map
        protected Zone GetNewZone(GraphicsDevice device, Color zoneColor, Tile selectedTile = null)
        {
            Tile tile = (selectedTile == null) ? tiles[AlgorithmHelper.BiasedRandom(0, tiles.Count - 1, 1.3)] : selectedTile;
            int xblocks = tile.Coords.Width / Block.Width;
            int yblocks = tile.Coords.Height / Block.Height;

            List<int> valX = new List<int>();
            List<int> valY = new List<int>();

            for (int x = 1; x < xblocks; ++x)
            {
                for (int y = 1; y < tile.GroundLevel+1; ++y)
                {
                    int rand = AlgorithmHelper.GetRandom(0, (xblocks * yblocks) / 20);
                    if (rand % 24 <= 3 && tile.Blocks[x, y] != null && x < tile.Blocks.GetLength(0) && y < tile.Blocks.GetLength(1))
                    {
                        rand = AlgorithmHelper.GetRandom(0, 5);
                        //Allways check whether adjacent tiles are empty -> whether the the Zone is to be created is not completely enclosed by blocks
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

            if (valX.Count == 0)
                return GetNewZone(device, zoneColor);

            int rnd = AlgorithmHelper.GetRandom(0, valX.Count);
            return new Zone(device, new Rectangle(tile.Coords.X + valX[rnd] * Block.Width, tile.Coords.Y + valY[rnd] * Block.Height, Block.Width, Block.Height), zoneColor);
        }

        //Initialize items
        private void CreateItems(GraphicsDevice device)
        {
            items = new List<Item>();
            //Find a spot where no block is present to create the item
            int count = AlgorithmHelper.GetRandom(1, 2 * TileCount);
            int nextX = AlgorithmHelper.GetRandom(0, Width / count);
            for (int i = 0; i < count; ++i)
            {
                var tile = GetTileForX(nextX);
                var tileX = (int)GlobalToTileCoordinates(new Vector2(nextX, 0), tile.Index).X;
                int blockIndexX = tileX / Block.Width;
                int tileBaseX = tile.Coords.Left;
                List<int> availableY = new List<int>();

                for(int y = 0; y < tile.GroundLevel; ++y)
                {
                    if (tile.Blocks[blockIndexX, y] == null)
                        availableY.Add(y);  
                }
                if (availableY.Count > 0)
                {
                    int selectedY = availableY[AlgorithmHelper.GetRandom(0, availableY.Count)];
                    //Random item type
                    Item.ItemType type = (Item.ItemType)AlgorithmHelper.GetRandom(0, Item.TypeCount);

                    Item item = new Item(this, device, type, i, new Vector2(tileBaseX + blockIndexX*Block.Width, selectedY*Block.Height), tile.Index, 16, 16);
                    items.Add(item);
                }
                //Move a little further and repeat
                nextX += AlgorithmHelper.GetRandom(16, Width / count);
            }
        }

        //Initializes an exit zone (for leaving intro/outro screens)
        protected void CreateExitZone(GraphicsDevice device)
        {
            exitZone = GetNewZone(device, Color.Red, GetTileByIndex(0));
        }

        //Updates Tiles and all NPCs on them
        protected void UpdateNPCs(GameTime gameTime) {
            List<NPC> toRemove = new List<NPC>();

            foreach (NPC n in NPCs) { 
                n.Update(gameTime);

                //Reset any entity which falls down from the map
                if (CheckOutOfMap((int)n.Position.Y) == -1 && n.Alive)
                    n.Reset();

                if(!n.Alive)
                    toRemove.Add(n);
            }

            //Removes dead NPCs
            foreach (NPC n in toRemove)
            {
                if (NPCs.Contains(n))
                    NPCs.Remove(n);
            }
}

        //Updates all events linked to entities (damage animations, attack timers)
        protected void UpdateEvents()
        {
            EntityEvents.Update();
        }

        //Updates items (physics, etc.)
        protected void UpdateItems(GameTime gameTime)
        {
            foreach (Item i in items)
                i.Update(gameTime);
        }

        //Updates quests (completion, status texts)
        protected void UpdateQuests()
        {
            Quests.Update();
        }
        
        //Returns current tile and tiles around it if there is a possibility that they might be visible too
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

            //GetTileForX returns null on the last coordinate, we konw that this is the last tile
            if (rightBound == Width)
                other = GetTileByIndex(tiles.Count - 1);

            if (!visibleTiles.Contains(other))
                visibleTiles.Add(other);

            return visibleTiles.ToArray();
        }
    
        //Returns all NPCs currently present on visible tiles
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
