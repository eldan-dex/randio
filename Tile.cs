using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2 {
    class Tile {
        //Public variables
        //********************************************************************************//
        public enum TileType {
            Shapes,
            LSystem,
            Invalid = 999
        }
        public const int TileTypeCount = 2;

        public TileType Type { get; private set; }
        public Texture2D TileTexture { get; private set; }
        public Texture2D BlockTexture { get; private set; }
        public Rectangle Coords { get; private set; }
        public List<NPC> NPCs { get; private set; }  //separate list for enemies?
        public Block[,] Blocks { get; private set; }
        public int Index { get; private set; }


        //Private variables
        //********************************************************************************//
        private Map map;
        private int wblocks;
        private int hblocks;


        //Public methods
        //********************************************************************************//
        public Tile(GraphicsDevice graphicsDevice, Map map, TileType type, Rectangle coords, int index) {
            this.map = map;

            Type = type;
            Coords = coords;
            Index = index;

            wblocks = coords.Width / Block.Width;
            hblocks = coords.Height / Block.Height;

            CreateTileTexture(graphicsDevice);
            CreateBlockTexture(graphicsDevice);

            CreateBlocks();

            CreateEntities(graphicsDevice); //separate into CreateNPCs and CreateEnemies?

        }

        public void Update(GameTime gameTime) {
            List<NPC> toRemove = new List<NPC>();
            foreach (NPC n in NPCs) {
                n.Update(gameTime);
                if (map.CheckOutOfMap((int)n.Position.Y) == -1) {
                    toRemove.Add(n);
                }
            }

            foreach (NPC n in toRemove) {
                if (NPCs.Contains(n))
                    NPCs.Remove(n);
            }
        }

        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Draw(TileTexture, Coords, Color.White);
            DrawBlocks(spriteBatch);
        }


        //Private methods
        //********************************************************************************//
        private void CreateTileTexture(GraphicsDevice graphicsDevice) {
            if (Type == TileType.Shapes)
                TileTexture = new ShapesBG(graphicsDevice, new SpriteBatch(graphicsDevice), Coords.Width, Coords.Height).Texture;

            else if (Type == TileType.LSystem)
                TileTexture = new LSystemBG(graphicsDevice, new SpriteBatch(graphicsDevice), Coords.Width, Coords.Height).Texture;

            //add more background types

            else //this is never supposed to execute. testing purproses only
            {
                //TileTexture = new Texture2D(graphicsDevice, Coords.Width, Coords.Height);
                //GraphicsHelper.DrawRectangle(TileTexture, Color.Gray);
                throw new NotImplementedException("CreateTileTexture got invalid TileType");
            }
            GraphicsHelper.OutlineRectangleSide(TileTexture, Color.Black, 4, true, false, true, false);
        }

        private void CreateBlockTexture(GraphicsDevice graphicsDevice) {
            BlockTexture = new Texture2D(graphicsDevice, Block.Width, Block.Height);

            //Fill only, outlining is done based on connected neighbours
            GraphicsHelper.DrawRectangle(BlockTexture, Color.Red);
        }

        private void CreateEntities(GraphicsDevice graphicsDevice) {
            //temporary
            NPCs = new List<NPC>();

            Random rnd = AlgorithmHelper.GetNewRandom();
            //temporary testing code
            int npcCount = rnd.Next(1, 17);
            for (int i = 0; i < npcCount; ++i) {
                int w = rnd.Next(16, 49);
                int h = rnd.Next(16, 49);
                Vector2 position = new Vector2(Coords.X + rnd.Next(0, Coords.Width - w + 1), rnd.Next(0, map.Height - h + 1));
                NPC npc = new NPC(graphicsDevice, map, position, Index, w, h);

                NPCs.Add(npc);
            }

            
        }

        private void CreateBlocks() {
            Blocks = new Block[wblocks, hblocks];
            bool[,] blockGrid = new bool[wblocks, hblocks];

            Random rnd = AlgorithmHelper.GetNewRandom();

            //Prepare grid (currently a random dumb layout, replace with a proper algorithm)
            for (int w = 0; w < wblocks; ++w) {
                for (int h = 0; h < hblocks; ++h) {
                    blockGrid[w, h] = false;

                    if (w < 4 || w > wblocks - 5) {
                        blockGrid[w, hblocks-4] = true;
                        continue;
                    }

                    //generate solid ground
                    if (h > 18)
                    {
                        blockGrid[w, h] = true;
                        continue;
                    }

                    //todo: holes in the ground (trenches, etc)

                    int tmp = rnd.Next(0, 12);
                    if (tmp > 3 && tmp < 6)
                        blockGrid[w, h] = true;
                    if (tmp == 1 && w < wblocks - 1)
                        blockGrid[w + 1, h] = true;
                    if (tmp == 5 && h < hblocks - 1)
                        blockGrid[w, h + 1] = true;
                }
            }

            //Generate textures according to the grid
            bool borderAbove;
            bool borderLeft;
            bool borderRight;
            bool borderBelow;


            //WHY DOES THIS NOT WORK?!
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
                        Texture2D texture = GraphicsHelper.CopyTexture(BlockTexture); //need to COPY THE TEXTURE HERE
                        GraphicsHelper.OutlineRectangleSide(texture, Color.LightGray, 4, borderLeft, borderAbove, borderRight, borderBelow);

                        Blocks[w, h] = new Block(texture);

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
                        //will probably be edited to reflect global position of tile (render to RenderTarget and then add the RenderTarget to global render?)
                        spriteBatch.Draw(block.Texture, new Rectangle(nX, nY, Block.Width, Block.Height), Color.White); //or use this.BlockTexture?
                    }
                }
            }
        }



    }
}
