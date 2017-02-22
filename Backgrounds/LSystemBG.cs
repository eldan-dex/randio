using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2
{
    class LSystemBG : Background
    {
        #region Private variables
        private LSystem defaultSystem;
        private Turtle turtle;
        #endregion

        #region Public methods
        public LSystemBG(GraphicsDevice device, SpriteBatch batch, int width, int height, Color[] palette)
        {
            this.palette = palette;
            CreateBackgroundTexture(device, batch, width, height);
            CreateBlockTexture(device, batch, Block.Width, Block.Height);
            BlockTopmostTexture = BlockTexture;
        }

        public void CreateBackgroundTexture(GraphicsDevice device, SpriteBatch batch, int width, int height)
        {
            Texture = new RenderTarget2D(device, width, height);
            List<LSystem.Rule> rules = new List<LSystem.Rule>();
            rules.Add(new LSystem.Rule("F", "1FF-[2-F+F-F]+[2+F-F+F]")); //basic tree
 
            defaultSystem = new LSystem("F", 10, 22, rules);
            turtle = new Turtle(device, batch, new Vector2(200, 640), -90, Color.White, palette);

            device.SetRenderTarget(Texture);
            device.Clear(palette[0]);
            batch.Begin();

            RenderTarget2D target = new RenderTarget2D(device, width, height);

            //Pregenerate trees
            List<LSystem> trees = new List<LSystem>();
            for (int i = 0; i < 6; ++i)
            {
                var currentSystem = new LSystem(defaultSystem);
                currentSystem.Iterate(i);
                trees.Add(currentSystem);
            }

            //create clouds
            int count = Math.Max(1, AlgorithmHelper.GetRandom(width / 512, width / 96));
            int nextX = AlgorithmHelper.GetRandom(0, width / count);
            int nextY;

            for (int i = 1; i < count; ++i)
            {
                nextY = AlgorithmHelper.GetRandom(0, 100);
                int w = AlgorithmHelper.GetRandom(32, 512);
                int h = AlgorithmHelper.GetRandom(32, 128);

                Texture2D cloud = new Texture2D(device, w, h);
                GraphicsHelper.DrawRectangle(cloud, palette[7]);
                GraphicsHelper.OutlineRectangle(cloud, ColorHelper.ChangeColorBrightness(palette[5], -0.2f), 8);

                batch.Draw(cloud, new Rectangle(nextX, nextY, w, h), Color.White);

                nextX += AlgorithmHelper.GetRandom(0, (int)(w * 1.5));
                if (nextX > width)
                    nextX -= AlgorithmHelper.GetRandom(w, (int)(width / 1.5));
            }

            count = Math.Max(1, AlgorithmHelper.GetRandom(width / 256, width / 64));
            nextX = AlgorithmHelper.GetRandom(0, width/count);
            for (int i = 0; i < count; ++i)
            {
                int size = AlgorithmHelper.GetRandom(1, trees.Count-1);  

                while (nextX + 100 * size > width)
                    --size;

                if (size == trees.Count - 2 && AlgorithmHelper.GetRandom(0, 10) > 6)
                    size = size + 1;

                if (size < 0)
                    break;

                if (i == 0 && nextX < size * 20)
                    nextX = size * 20;

                var currentSystem = trees[size];
                turtle.SetDefaults(new Vector2(nextX, 640), -90);
                turtle.DrawSystem(currentSystem, target);
                nextX += 50 * size;
            }

            batch.Draw(target, new Rectangle(0, 0, width, height), Color.White);

            batch.End();
            device.SetRenderTarget(null);
        }

        public void CreateBlockTexture(GraphicsDevice device, SpriteBatch batch, int width, int height)
        {
            //implement block texture generation
            var tex = new Texture2D(device, width, height);
            GraphicsHelper.DrawRectangle(tex, palette[1]);
            BlockTexture = tex;
        }
        #endregion
    }
}