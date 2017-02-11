using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2 {
    class Tile {
        #region Public variables
        public enum TileType {
            LSystem,
            City,
            Mountains,
            Shapes, //this one is currently disabled
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
        #endregion

        #region Private variables
        private Map map;
        private int wblocks;
        private int hblocks;
        #endregion

        #region Public methods
        public Tile(GraphicsDevice graphicsDevice, Map map, TileType type, Rectangle coords, int index) {
            this.map = map;

            Type = type;
            Coords = coords;
            Index = index;

            wblocks = coords.Width / Block.Width;
            hblocks = coords.Height / Block.Height;

            if (Type == TileType.LSystem)
                Background = new LSystemBG(graphicsDevice, new SpriteBatch(graphicsDevice), Coords.Width, Coords.Height);
            else if (Type == TileType.City)
                Background = new CityBG(graphicsDevice, new SpriteBatch(graphicsDevice), Coords.Width, Coords.Height);
            else if (Type == TileType.Mountains)
                Background = new MountainsBG(graphicsDevice, new SpriteBatch(graphicsDevice), Coords.Width, Coords.Height);
            else if (Type == TileType.Screen)
                Background = new ScreenBG(graphicsDevice, new SpriteBatch(graphicsDevice), Coords.Width, Coords.Height);
            /*else if (Type == TileType.Shapes)
                Background = new ShapesBG(graphicsDevice, new SpriteBatch(graphicsDevice), Coords.Width, Coords.Height);*/
            else
            {
                throw new NotSupportedException("This is not supposed to happen. Ever.");
            }

            CreateTileTexture(graphicsDevice);
            CreateBlockTexture(graphicsDevice);
            CreateBlocks();

            if (Type != TileType.Screen)
            {
                CreateEntities(graphicsDevice); //separate into CreateNPCs and CreateEnemies?
            }
            else
            {
                NPCs = new List<NPC>();
            }

        }

        public void Update(GameTime gameTime) {
            List<NPC> toRemove = new List<NPC>();
            foreach (NPC n in NPCs)
            {
                n.Update(gameTime);
                if (map.CheckOutOfMap((int)n.Position.Y) == -1 |! n.Alive)
                {
                    toRemove.Add(n);
                }
            }

            foreach (NPC n in toRemove)
            {
                if (NPCs.Contains(n))
                    NPCs.Remove(n);
            }
        }

        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Draw(TileTexture, Coords, Color.White);
            DrawBlocks(spriteBatch);
        }
#endregion

        #region Private methods
        private void CreateTileTexture(GraphicsDevice graphicsDevice)
        {
            TileTexture = Background.Texture;
            //GraphicsHelper.OutlineRectangleSide(TileTexture, Color.Black, 4, true, false, true, false);
        }

        private void CreateBlockTexture(GraphicsDevice graphicsDevice) {
            BlockTexture = Background.BlockTexture;
            BlockTopmostTexture = Background.BlockTopmostTexture;
        }

        private void CreateEntities(GraphicsDevice graphicsDevice) {
            NPCs = new List<NPC>();

            //temporary testing code
            int npcCount = AlgorithmHelper.GetRandom(1, 17);
            for (int i = 0; i < npcCount; ++i) {
                int w = AlgorithmHelper.GetRandom(24, 57); //16-49 small, 24-57 medium
                int h = AlgorithmHelper.GetRandom(24, 57); //32-65 big
                Vector2 position = new Vector2(Coords.X + AlgorithmHelper.GetRandom(0, map.Width - w + 1), AlgorithmHelper.GetRandom(0, map.Height - h + 1));

                //BALANCE THIS
                int additionalBase = AlgorithmHelper.GetRandom(Index, 2 * Index);
                int addHP = AlgorithmHelper.GetRandom(additionalBase / 2, 2 * additionalBase);
                float addStr = AlgorithmHelper.GetRandom((float)additionalBase / 4, additionalBase);
                float addDef = AlgorithmHelper.GetRandom((float)additionalBase / 4, additionalBase);
                float addSpd = AlgorithmHelper.GetRandom(0, (float)additionalBase / 100); //balance additional speed

                NPC npc = new NPC(graphicsDevice, map, position, Index, this, w, h, addHP, addStr, addDef, addSpd);

                NPCs.Add(npc);
            }        
        }

        private void CreateBlocks() {
            Blocks = new Block[wblocks, hblocks];
            bool[,] blockGrid = new bool[wblocks, hblocks];

            //Prepare grid (currently a random dumb layout, replace with a proper algorithm)
            for (int w = 0; w < wblocks; ++w) {
                for (int h = 0; h < hblocks; ++h) {
                    blockGrid[w, h] = false;

                    //generate solid ground
                    if (h > GroundLevel)
                    {
                        blockGrid[w, h] = true;
                        continue;
                    }

                    //todo: holes in the ground (trenches, etc)

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
                        //todo: will probably be edited to reflect global position of tile (render to RenderTarget and then add the RenderTarget to global render?)
                        var texture = block.Texture == null ? block.TopmostTexture : block.Texture; //draw topmost blocks with a different texture (if needed)

                        spriteBatch.Draw(texture, new Rectangle(nX, nY, Block.Width, Block.Height), Color.White); //or use this.BlockTexture?
                        if (Game.debugEnabled)
                        spriteBatch.DrawString(Game.font, x + "," + y, new Vector2(nX+2, nY+3), Color.Black, 0f, Vector2.Zero, 0.4f, SpriteEffects.None, 0);
                    }
                }
            }
        }
        #endregion
    }
}
