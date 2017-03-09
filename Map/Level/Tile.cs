using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace Randio_2 {
    //Tile - main building part of the map - Background, Blocks, 
    class Tile {
        #region Public variables
        //Environment types (different algorithm for each) + Screen (plain color without blocks)
        public enum TileType {
            LSystem,
            City,
            Mountains,
            Screen = 998,
            Invalid = 999
        }
        public const int TileTypeCount = 3;

        public Background Background { get; private set; }
        public TileType Type { get; private set; }
        public Texture2D TileTexture { get; private set; }
        public Texture2D BlockTexture { get; private set; }
        public Texture2D BlockTopmostTexture { get; private set; }
        public Rectangle Coords { get; private set; }
        public Block[,] Blocks { get; private set; }
        public int Index { get; private set; }
        public List<NPC> NPCs { get; private set; }
        public int GroundLevel = 15;
        public Color[] Palette { get; private set; }
        #endregion

        #region Private variables
        private Map map;
        private int wblocks;
        private int hblocks;
        #endregion

        #region Public methods
        //Default ctor
        public Tile(GraphicsDevice graphicsDevice, Map map, TileType type, Rectangle coords, int index) {
            this.map = map;

            Type = type;
            Coords = coords;
            Index = index;

            wblocks = coords.Width / Block.Width;
            hblocks = coords.Height / Block.Height;

            //Generate a color palette and randomize it's colors
            int gens = AlgorithmHelper.GetRandom(16, 33);
            int gskip = AlgorithmHelper.GetRandom(0, gens - 8);
            Palette = ColorHelper.Generate(gens).Skip(gskip).ToArray();
            
            //Darken all palette colors by 50%
            for (int p = 0; p < Palette.Length; ++p)
            {
                Palette[p] = ColorHelper.ChangeColorBrightness(Palette[p], -0.5f);
            }

            //Initialize the background
            if (Type == TileType.LSystem)
                Background = new LSystemBG(graphicsDevice, new SpriteBatch(graphicsDevice), Coords.Width, Coords.Height, Palette);
            else if (Type == TileType.City)
                Background = new CityBG(graphicsDevice, new SpriteBatch(graphicsDevice), Coords.Width, Coords.Height, Palette);
            else if (Type == TileType.Mountains)
                Background = new MountainsBG(graphicsDevice, new SpriteBatch(graphicsDevice), Coords.Width, Coords.Height, Palette);
            else if (Type == TileType.Screen)
                Background = new ScreenBG(graphicsDevice, new SpriteBatch(graphicsDevice), Coords.Width, Coords.Height, Palette);

            CreateTileTexture(graphicsDevice);
            CreateBlockTexture(graphicsDevice);
            CreateBlocks();

            //Don't generate NPCs automatically when a Screen is displayed
            if (Type != TileType.Screen)
                CreateEntities(graphicsDevice);

        }

        //Draws Tile background and Blocks
        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Draw(TileTexture, Coords, Color.White);
            DrawBlocks(spriteBatch);
        }
        #endregion

        #region Private methods
        //Sets the tile texture
        private void CreateTileTexture(GraphicsDevice graphicsDevice)
        {
            TileTexture = Background.Texture;
        }

        //Set block textures
        private void CreateBlockTexture(GraphicsDevice graphicsDevice) {
            BlockTexture = Background.BlockTexture;
            BlockTopmostTexture = Background.BlockTopmostTexture;
        }

        //Creates 1-10 NPCs on this tile
        private void CreateEntities(GraphicsDevice graphicsDevice) {
            //temporary testing code
            int npcCount = AlgorithmHelper.GetRandom(1, 11);
            for (int i = 0; i < npcCount; ++i) {
                int w = AlgorithmHelper.GetRandom(24, 57); //16-49 small, 24-57 medium
                int h = AlgorithmHelper.GetRandom(24, 57); //32-65 big
                Vector2 position = new Vector2(Coords.X + AlgorithmHelper.GetRandom(0, map.Width - w + 1), AlgorithmHelper.GetRandom(0, map.Height - h + 1));

                int additionalBase = AlgorithmHelper.GetRandom(Index, 2 * Index);
                int addHP = AlgorithmHelper.GetRandom(additionalBase / 2, 2 * additionalBase);
                float addStr = AlgorithmHelper.GetRandom((float)additionalBase / 4, additionalBase);
                float addDef = AlgorithmHelper.GetRandom((float)additionalBase / 4, additionalBase);
                float addSpd = AlgorithmHelper.GetRandom(0, (float)additionalBase / 100); //balance additional speed

                NPC npc = new NPC(graphicsDevice, map, position, Index, this, w, h, addHP, addStr, addDef, addSpd);

                map.NPCs.Add(npc);
            }        
        }

        //Randomly places blocks on the tile. No check iwhether 
        private void CreateBlocks() {
            Blocks = new Block[wblocks, hblocks];
            bool[,] blockGrid = new bool[wblocks, hblocks];

            //Prepare grid (currently a random dumb layout, replace with a proper algorithm)
            for (int w = 0; w < wblocks; ++w) {
                for (int h = 0; h < hblocks; ++h) {
                    blockGrid[w, h] = false;

                    //Generate solid ground
                    if (h > GroundLevel)
                    {
                        blockGrid[w, h] = true;
                        continue;
                    }

                    //Only add blocks other than ground when not a Screen tile
                    if (Type != TileType.Screen)
                    {
                        if (w < 4 || w > wblocks - 5)
                        {
                            blockGrid[w, hblocks - 3] = true;
                            continue;
                        }

                        int tmp = AlgorithmHelper.GetRandom(0, 15);
                        if (tmp > 3 && tmp < 7)
                            blockGrid[w, h] = true;
                        if (tmp == 1 && w < wblocks - 2)
                            blockGrid[w + 1, h] = true;
                        if (tmp == 5 && h < hblocks - 2)
                            blockGrid[w, h + 1] = true;
                    }
                }
            }

            //Generate textures according to the grid
            bool borderAbove;
            bool borderLeft;
            bool borderRight;
            bool borderBelow;


            for (int w = 0; w < wblocks; ++w) {
                for (int h = 0; h < hblocks; ++h) {
                    if (blockGrid[w, h]) {

                        //if there is a block above this one
                        if (h > 0 && blockGrid[w, h - 1])
                            borderAbove = false;
                        else
                            borderAbove = true;

                        //if there is a block below this one
                        if (h < hblocks - 1 && blockGrid[w, h + 1])
                            borderBelow = false;
                        else
                            borderBelow = true;

                        //if there is a block left of this one
                        if (w > 0 && blockGrid[w - 1, h])
                            borderLeft = false;
                        else
                            borderLeft = true;

                        //if there is a block right of this one
                        if (w < wblocks - 1 && blockGrid[w + 1, h])
                            borderRight = false;
                        else
                            borderRight = true;


                        //Assign texture accordingly
                        //If block is topmost, use a different texture
                        Texture2D texture = borderAbove ? GraphicsHelper.CopyTexture(BlockTexture) : GraphicsHelper.CopyTexture(BlockTopmostTexture); //need to COPY THE TEXTURE HERE
                        if (Background.OutlineBlocks)
                            GraphicsHelper.OutlineRectangleSide(texture, Color.LightGray, 4, borderLeft, borderAbove, borderRight, borderBelow);

                        Blocks[w, h] = new Block(texture, borderAbove);

                    }
                }
            }
        }

        //Draw Blocks to screen
        private void DrawBlocks(SpriteBatch spriteBatch) {
            //Draw blocks
            for (int y = 0; y < Coords.Height/Block.Height; ++y) {
                for (int x = 0; x < Coords.Width/Block.Width; ++x) {
                    // If there is a visible tile in that position
                    Block block = Blocks[x, y];
                    if (block != null) {
                        int nX = (int)(Coords.X + x * Block.Size.X);
                        int nY = (int)(Coords.Y + y * Block.Size.Y);
                        // Draw it in screen space.
                        var texture = block.Texture == null ? block.TopmostTexture : block.Texture; //draw topmost blocks with a different texture (if needed)
                        spriteBatch.Draw(texture, new Rectangle(nX, nY, Block.Width, Block.Height), Color.White); //or use this.BlockTexture?

                        if (Game.DebugEnabled) //Draws block coordinates on the block if debug mode is enabled
                            spriteBatch.DrawString(Game.font, x + "," + y, new Vector2(nX+2, nY+3), Color.Black, 0f, Vector2.Zero, 0.4f, SpriteEffects.None, 0);
                    }
                }
            }
        }
        #endregion
    }
}
