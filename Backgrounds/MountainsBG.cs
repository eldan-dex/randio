using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Randio_2 {
    //Thanks to ReyeMe for contributing this background
    class MountainsBG : Background {
        #region Private variables
        //Put any global variable declarations here.
        double[,] heightLines;
        List<Point> Stars;

        #endregion

        #region Public methods
        public MountainsBG( GraphicsDevice device, SpriteBatch batch, int width, int height )
        {
            heightLines = new double[3, width / 2];
            Stars = new List<Point>();

            int Starcount = AlgorithmHelper.GetRandom(200, 500);
            for (int i = 0; i < Starcount; i++)
            {
                Point Star = new Point(AlgorithmHelper.GetRandom(0, width), AlgorithmHelper.GetRandom(0, height));
                Stars.Add(Star);
            }

            heightLines[0, 0] = AlgorithmHelper.GetRandom(height / 4, height / 2);
            heightLines[0, width / 2 - 1] = AlgorithmHelper.GetRandom(0, height / 5);

            heightLines[1, 0] = AlgorithmHelper.GetRandom(1, height / 5);
            heightLines[1, width / 2 - 1] = AlgorithmHelper.GetRandom(height / 4, height / 2);

            heightLines[2, 0] = AlgorithmHelper.GetRandom(height / 3, height / 2);
            heightLines[2, width / 2 - 1] = AlgorithmHelper.GetRandom(height / 6, height / 4);

            for (int i = 0; i < heightLines.GetLength(0); i++)
            {
                double[] heightLine = new double[width / 2];

                heightLine[0] = heightLines[i, 0];
                heightLine[width / 2 - 1] = heightLines[i, width / 2 - 1];

                Teragen(heightLine, 0, width / 2 - 1, AlgorithmHelper.GetRandom(10, 100));
                Smooth(heightLine, 5, 2);

                ArrCPYdim(heightLine, heightLines, i);
            }



            CreateBackgroundTexture(device, batch, width, height);
            CreateBlockTexture(device, batch, Block.Width, Block.Height);
            CreateTopmostBlockTexture(device, batch, Block.Width, Block.Height);
        }

        public void CreateBackgroundTexture( GraphicsDevice device, SpriteBatch batch, int width, int height )
        {
            Texture = new RenderTarget2D(device, width, height);

            //Initialize whatever you need here

            device.SetRenderTarget(Texture);
            device.Clear(Color.Black);
            batch.Begin();

            //Draw onto batch here
            Color StarWhite = new Color(210, 210, 210);
            Color StarBloom = new Color(150, 150, 150);
            foreach (Point Star in Stars)
            {
                //big star
                if (AlgorithmHelper.GetRandom(0, 5) == 4)
                {
                    RenderTarget2D BigStarSprite = new RenderTarget2D(device, 2, 2);
                    GraphicsHelper.DrawRectangle(BigStarSprite, Color.White);

                    batch.Draw(BigStarSprite, new Rectangle(Star.X - 2, Star.Y, 2, 2), StarBloom);
                    batch.Draw(BigStarSprite, new Rectangle(Star.X + 2, Star.Y, 2, 2), StarBloom);
                    batch.Draw(BigStarSprite, new Rectangle(Star.X, Star.Y - 2, 2, 2), StarBloom);
                    batch.Draw(BigStarSprite, new Rectangle(Star.X, Star.Y + 2, 2, 2), StarBloom);
                    batch.Draw(BigStarSprite, new Rectangle(Star.X, Star.Y, 2, 2), StarWhite);

                    continue;
                }

                RenderTarget2D StarSprite = new RenderTarget2D(device, 1, 1);
                GraphicsHelper.DrawRectangle(StarSprite, Color.White);

                batch.Draw(StarSprite, new Rectangle(Star.X - 1, Star.Y, 1, 1), StarBloom);
                batch.Draw(StarSprite, new Rectangle(Star.X + 1, Star.Y, 1, 1), StarBloom);
                batch.Draw(StarSprite, new Rectangle(Star.X, Star.Y - 1, 1, 1), StarBloom);
                batch.Draw(StarSprite, new Rectangle(Star.X, Star.Y + 1, 1, 1), StarBloom);
                batch.Draw(StarSprite, new Rectangle(Star.X, Star.Y, 1, 1), StarWhite);
            }

            int line = 0;
            for (int i = 0; i < heightLines.GetLength(1); i++)
            {
                int sh = (int)(heightLines[0, i] * 0.75) + 50;
                int sh2 = (int)(heightLines[1, i]) + 50;
                int sh3 = (int)(heightLines[2, i] * 1.3) + 50;

                RenderTarget2D spike = new RenderTarget2D(device, 2, sh);
                GraphicsHelper.DrawRectangle(spike, Color.White);

                batch.Draw(spike, new Rectangle(line, height - sh3, 2, sh3), new Color(0, 12, 41));
                batch.Draw(spike, new Rectangle(line, height - sh2, 2, sh2), new Color(90, 0, 91));
                batch.Draw(spike, new Rectangle(line, height - sh, 2, sh), new Color(30, 0, 91));
                line += 2;
            }

            batch.End();
            device.SetRenderTarget(null);
        }

        public void CreateBlockTexture( GraphicsDevice device, SpriteBatch batch, int width, int height )
        {
            //Base color
            var texture = new Texture2D(device, width, height);

            Color[] color = new Color[texture.Width * texture.Height];
            texture.GetData(color); //Texture size limit is 4096*4096. Textures bigger than that crash when being converted to Color[]

            //noise
            for (int i = 0; i < texture.Width * texture.Height; ++i)
            {
                /*
                float scale = AlgorithmHelper.GetRandom(75, 100) / 100f;
                color[i] = Color.FromNonPremultiplied((byte)(diffuse.R*scale), (byte)(diffuse.G * scale), (byte)(diffuse.B * scale), diffuse.A);*/
                color[i] = new Color(30, 42, 30);
            }

            texture.SetData(color);

            BlockTexture = texture;
        }

        //This function will generate a separate texture for all blocks, which are the topmost of a stack of blocks.
        public void CreateTopmostBlockTexture( GraphicsDevice device, SpriteBatch batch, int width, int height )
        {
            //Base color
            var texture = new Texture2D(device, width, height);

            Color[] color = new Color[texture.Width * texture.Height];
            texture.GetData(color); //Texture size limit is 4096*4096. Textures bigger than that crash when being converted to Color[]

            //noise
            for (int i = 0; i < texture.Width * texture.Height; ++i)
            {
                /*
                float scale = AlgorithmHelper.GetRandom(75, 100) / 100f;
                color[i] = Color.FromNonPremultiplied((byte)(diffuse.R*scale), (byte)(diffuse.G * scale), (byte)(diffuse.B * scale), diffuse.A);*/
                color[i] = new Color(0, 42, 0);
            }

            texture.SetData(color);

            BlockTopmostTexture = texture; //Leave this here for topmost bloks to be identical to normal blocks. Replace with own texture generation to make them different.
        }
        #endregion

        #region Private methods
        //Any additional methods (for this class at least) are to be placed here
        void Teragen( double[] heightmap, int start, int end, double rnd )
        {
            if (end - start <= 1) return;
            int width = heightmap.Length;

            heightmap[(start + end) / 2] = (heightmap[start] + heightmap[end]) / 2 + ((Math.PI) * rnd / 2);

            rnd = AlgorithmHelper.GetRandom(10, 100);

            Teragen(heightmap, (start + end) / 2, end, rnd);
            Teragen(heightmap, start, (start + end) / 2, rnd);
        }

        void Smooth( double[] heightmap, int range, int iteration )
        {
            for (int j = 0; j < iteration; j++)
                for (int i = range / 2; i < heightmap.Length - 1 - range / 2; i++)
                {
                    double avr = 0;
                    for (int k = -range / 2; k < range / 2; k++)
                        avr += heightmap[i + k];

                    heightmap[i] = avr / range;
                }

        }

        void ArrCPYdim( double[] source, double[,] destination, int dimension )
        {
            for (int i = 0; i < source.Length; i++)
                destination[dimension, i] = source[i];
        }
        #endregion

        #region Nested classes
        //If you need to crate any additional classes, nest them here.
        #endregion
    }
}


/* 
 * For geometry, use the GeometryHelper class
 * Drawing can be simplified by making use of what's in the GrapicsHelper
 * If you needed to work with text, StringHelper is here for you
 * And AlgorithmHelper is here to provide you with random and stuff
 * - for the sake of consistency, don't spawn new Random instances. Use AlgorithmHelper.GetRandom(int min, int max) or GetBiasedRandom (if you want your randoms to be more dense towards one end of the spectrum)
 * Feel free to add your own methods to the helper classes, if you think they might be useful for more than just creating this background
 * For reference, check ShapesBG or LSystemBG
 * Good luck!
 */
