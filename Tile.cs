﻿using System;
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
            GraphicsHelper.OutlineRectangleSide(TileTexture, Color.Black, 4, true, false, true, false);
        }

        private void CreateBlockTexture(GraphicsDevice graphicsDevice) {
            BlockTexture = new Texture2D(graphicsDevice, Block.Width, Block.Height);

            //Fill only, outlining is done based on connected neighbours
            GraphicsHelper.FillRectangle(BlockTexture, Color.Red);
        }

        private void CreateEntities() {
            //temporary
            Entities = new List<Entity>();
            Entities.Add(new Entity(this));
        }

        private void CreateBlocks() {
            Blocks = new Block[wblocks, hblocks];
            bool[,] blockGrid = new bool[wblocks, hblocks];

            Random rnd = AlgorithmHelper.GetNewRandom();

            //Prepare grid (currently a random dumb layout, replace with a proper algorithm)
            for (int w = 0; w < wblocks; ++w) {
                for (int h = 0; h < hblocks; ++h) {
                    blockGrid[w, h] = false;

                    if (w < 3 || w > wblocks - 3) {
                        blockGrid[w, 20] = true;
                        continue;
                    }

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
            bool isAbove;
            bool isLeft;
            bool isRight;
            bool isBelow;

            for (int w = 0; w < wblocks; ++w) {
                for (int h = 0; h < hblocks; ++h) {
                    if (blockGrid[w, h]) {
                        //isAbove
                        if (h > 0 && blockGrid[w, h - 1])
                            isAbove = true;
                        else
                            isAbove = false;

                        //isBelow
                        if (h < hblocks - 1 && blockGrid[w, h + 1])
                            isBelow = true;
                        else
                            isBelow = false;

                        //isLeft
                        if (w > 0 && blockGrid[w - 1, h])
                            isLeft = true;
                        else
                            isLeft = false;

                        //isRight
                        if (w < wblocks - 1 && blockGrid[w + 1, h])
                            isRight = true;
                        else
                            isRight = false;


                        //Assign texture accordingly  
                        Texture2D texture = BlockTexture;
                        GraphicsHelper.OutlineRectangleSide(texture, Color.Black, 2, isLeft, isAbove, isRight, isBelow);

                        Blocks[w, h] = new Block(texture);

                    }
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
