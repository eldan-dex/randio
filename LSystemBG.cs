using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2
{
    class LSystemBG : Background
    {
        private LSystem defaultSystem;
        private Turtle turtle;

        public LSystemBG(GraphicsDevice device, SpriteBatch batch, int width, int height)
        {
            CreateBackgroundTexture(device, batch, width, height);
            CreateBlockTexture(device, batch, Block.Width, Block.Height);
        }

        public void CreateBackgroundTexture(GraphicsDevice device, SpriteBatch batch, int width, int height)
        {
            Texture = new RenderTarget2D(device, width, height);
            List<LSystem.Rule> rules = new List<LSystem.Rule>();
            rules.Add(new LSystem.Rule("F", "1FF-[2-F+F-F]+[2+F-F+F]"));
 
            defaultSystem = new LSystem("F", 10, 22, rules); //TODO: generate ctor args
            turtle = new Turtle(device, batch, new Vector2(200, 640), -90, Color.White);

            device.SetRenderTarget(Texture);
            device.Clear(Color.Black);
            batch.Begin();

            RenderTarget2D target = new RenderTarget2D(device, width, height);

            //TODO: either pregenerate a limited set of systems to use (but systems can be more complex)
            //      or generate systems over and over again, but keep the iteration counts low and rules short, to take less time.

            //TODO: create a proper algorithm
            int count = AlgorithmHelper.GetRandom(1, 7);
            int nextX = width / count;
            for (int i = 0; i < count; ++i)
            {
                int size = AlgorithmHelper.GetRandom(1, 5);
                var currentSystem = new LSystem(defaultSystem);
                currentSystem.Iterate(size);
                turtle.SetDefaults(new Vector2(nextX, 640), -90);
                turtle.DrawSystem(currentSystem, target);
                nextX += 150 * size;
            }

            batch.Draw(target, new Rectangle(0, 0, width, height), Color.White);

            batch.End();
            device.SetRenderTarget(null);
        }

        public void CreateBlockTexture(GraphicsDevice device, SpriteBatch batch, int width, int height)
        {
            //implement block texture generation
            var tex = new Texture2D(device, width, height);
            GraphicsHelper.DrawRectangle(tex, Color.Yellow);
            BlockTexture = tex;
        }
    }
}