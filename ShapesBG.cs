using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2
{
    class ShapesBG : Background
    {
        List<Shape> shapes;

        public ShapesBG(GraphicsDevice device, SpriteBatch batch, int width, int height)
        {
            CreateBackgroundTexture(device, batch, width, height);
            CreateBlockTexture(device, batch, Block.Width, Block.Height);
        }

        public void CreateBackgroundTexture(GraphicsDevice device, SpriteBatch batch, int width, int height)
        {
            Texture = new RenderTarget2D(device, width, height);
            shapes = new List<Shape>();

            device.SetRenderTarget(Texture);
            device.Clear(Color.Black);
            batch.Begin();

            Random rnd = AlgorithmHelper.GetNewRandom();
            int count = rnd.Next(0, width / 30 + 1);
            bool darkerOutline = rnd.Next(0, 2) == 1;

            for (int i = 0; i < count; ++i)
            {
                var type = (Shape.ShapeType)rnd.Next(0, 2);
                var w = rnd.Next(16, 129);
                var h = (rnd.Next(0, 2) > 0) ? w : rnd.Next(16, 97); //2:1 chance that width is similar to height
                var x = rnd.Next(1, width - w); //shapes cannot go beyond our tile on the x axis (overlapping into other tiles)
                var y = rnd.Next(1 - h / 3 * 2, height - h / 3); //but can go 2/3 of their height above or below
                var outlineWidth = rnd.Next((w + h / 2) / 50, (w + h / 2) / 20);

                shapes.Add(new Shape(device, batch, type, w, h, x, y, outlineWidth, darkerOutline));
            }

            foreach (Shape s in shapes)
            {
                batch.Draw(s.Texture, s.Coords, Color.White);
            }

            batch.End();
            device.SetRenderTarget(null);
        }

        public void CreateBlockTexture(GraphicsDevice device, SpriteBatch batch, int width, int height)
        {
            //implement block texture generation
            var tex = new Texture2D(device, width, height);
            GraphicsHelper.DrawRectangle(tex, Color.Red);
            BlockTexture = tex;
        }

        class Shape
        {
            //Ready to add more shapes if needed
            public enum ShapeType
            {
                Rectangle,
                Splotch
            }

            public int Width { get; private set; }
            public int Height { get; private set; }
            public Vector2 Position { get; private set; }
            public Rectangle Coords { get; private set; }
            public Texture2D Texture { get; private set; }
            public ShapeType Type { get; private set; }

            public Shape(GraphicsDevice device, SpriteBatch batch, ShapeType type, int width, int height, int x, int y, int outlineWidth, bool darkerOutline)
            {
                Width = width;
                Height = height;
                Position = new Vector2(x, y);
                Coords = new Rectangle(x, y, width, height);
                Texture = new Texture2D(device, width, height);
                Type = type;

                CreateTexture(device, batch, width, height, outlineWidth, darkerOutline);
            }

            private void CreateTexture(GraphicsDevice device, SpriteBatch batch, int width, int height, int outlineWidth, bool darkerOutline)
            {
                Texture = new Texture2D(device, width, height);

                //create a color palette - bg:black - fg:fluorescent colors
                //bg:white - fg:pastel colors?

                if (Type == ShapeType.Rectangle)
                {
                    GraphicsHelper.DrawRectangle(Texture, Color.Violet);
                    if (darkerOutline)
                        GraphicsHelper.CreateDarkerOutline(Texture, outlineWidth);
                    else
                        GraphicsHelper.OutlineRectangle(Texture, Color.White, outlineWidth);
                }

                else if (Type == ShapeType.Splotch)
                    GraphicsHelper.DrawSplotch(Texture, Color.Violet);
            }
        }
    }
}