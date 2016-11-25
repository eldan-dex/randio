using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Randio_2 {
    class Tile {
        public enum TileType {
            City,
            Nature,
            Sky,
            Abstract,
            Invalid = 999
        }

        public TileType Type { get; private set; }
        public Texture2D TileTexture { get; private set; }
        public Texture2D BlockTexture { get; private set; }
        public Rectangle Coords { get; private set; }

        public List<Entity> Entities { get; private set; } //or private?

        public Block[,] Blocks { get; private set; }

        private int wblocks;
        private int hblocks;

        //separate list for enemies?

        public Tile(GraphicsDevice graphicsDevice, TileType type, Rectangle coords) {
            Type = type;
            Coords = coords;

            wblocks = coords.Width / Block.Width;
            hblocks = coords.Height / Block.Height;

            CreateTileTexture(graphicsDevice);
            CreateBlockTexture(graphicsDevice);

            CreateBlocks();

            CreateEntities(); //separate into CreateNPCs and CreateEnemies?

        }

        private void CreateTileTexture(GraphicsDevice graphicsDevice) {
            TileTexture = new Texture2D(graphicsDevice, Coords.Width, Coords.Height);

            GraphicsHelper.FillRectangle(TileTexture, Color.Gray);
            //GraphicsHelper.OutlineRectangle(TileTexture, Color.Black, 3);
        }

        private void CreateBlockTexture(GraphicsDevice graphicsDevice) {
            BlockTexture = new Texture2D(graphicsDevice, Block.Width, Block.Height);

            GraphicsHelper.FillRectangle(BlockTexture, Color.Red);
            GraphicsHelper.OutlineRectangle(BlockTexture, Color.Black, 2);
        }

        private void CreateEntities() {
            //temporary
            Entities = new List<Entity>();
            Entities.Add(new Entity(this));
        }

        private void CreateBlocks() {
            Blocks = new Block[wblocks, hblocks];

            Random rnd = AlgorithmHelper.GetNewRandom();

            //Temp code, replace with something that makes sense
            for (int w = 0; w < wblocks; ++w) {
                for (int h = 0; h < hblocks; ++h) {
                    if (w < 2 && h < 2)
                        continue;
                    int tmp = rnd.Next(0, 10);
                    if (tmp > 3 && tmp < 6)
                        Blocks[w, h] = new Block(BlockTexture);
                    if (tmp == 1 && w < wblocks - 1)
                        Blocks[w + 1, h] = new Block(BlockTexture);
                    if (tmp == 5 && h < hblocks - 1)
                        Blocks[w, h + 1] = new Block(BlockTexture);
                }
            }

        }

        public void Update(GameTime gameTime) {
            foreach (Entity e in Entities)
                e.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Draw(TileTexture, Coords, Color.White);
            DrawBlocks(spriteBatch);
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
