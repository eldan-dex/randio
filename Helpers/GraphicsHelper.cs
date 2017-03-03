using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2 {
    //Contains methods for graphics manipulation (creating objects, outlines, etc.)
    class GraphicsHelper
    {
        #region Public methods
        //Fills a rectangle with a single color
        public static void FillRectangle(Texture2D texture, Color fill)
        {
            Color[] color = new Color[texture.Width * texture.Height];
            texture.GetData(color);

            for (int i = 0; i < texture.Width * texture.Height; ++i)
                color[i] = fill;

            texture.SetData(color);
        }

        //Draws a rectangle outline of a given width with the given color
        public static void OutlineRectangle(Texture2D texture, Color outline, int outlineWidth)
        {
            Color[] color = new Color[texture.Width * texture.Height];
            texture.GetData(color);

            int index = 0;
            for (int y = 0; y < texture.Height; ++y)
            {
                for (int x = 0; x < texture.Width; ++x)
                {
                    if (y < outlineWidth || x < outlineWidth || y > texture.Height - outlineWidth - 1 || x > texture.Width - outlineWidth - 1)
                        color[index] = outline;
                    ++index;
                }
            }
            texture.SetData(color);
        }

        //Outlines only selected sides of a rectangle
        public static void OutlineRectangleSide(Texture2D texture, Color outline, int outlineWidth, bool leftSide, bool topSide, bool rightSide, bool bottomSide)
        {
            Color[] color = new Color[texture.Width * texture.Height];
            texture.GetData(color);

            int index = 0;
            for (int y = 0; y < texture.Height; ++y)
            {
                for (int x = 0; x < texture.Width; ++x)
                {
                    if ((topSide && y < outlineWidth) || (leftSide && x < outlineWidth) || (bottomSide && y > texture.Height - outlineWidth - 1) || (rightSide && x > texture.Width - outlineWidth - 1))
                        color[index] = outline;
                    ++index;
                }
            }
            texture.SetData(color);
        }

        //Clones a texture and returns it as a new texture
        public static Texture2D CopyTexture(Texture2D texture)
        {
            Texture2D result = new Texture2D(texture.GraphicsDevice, texture.Width, texture.Height);
            Color[] color = new Color[texture.Width * texture.Height];
            texture.GetData(color);
            result.SetData(color);
            return result;
        }

        //Used by LSystem-Turtle, draws a square based on two vectors
        public static void DrawSquareFromVector(SpriteBatch spriteBatch, Texture2D texture, Vector2 a, Vector2 b, Color color, int size)
        {
            Vector2 edge = b - a;

            //calculate rotation angle
            double angle = Math.Atan2(edge.Y, edge.X);

            spriteBatch.Draw(texture, new Rectangle((int)a.X, (int)a.Y, size, size), null, color, 0f, new Vector2(0, 0), SpriteEffects.None, 0);
        }

        //Used by Player, draws an arrow onto a texture (player direction indicator)
        public static void DrawArrow(int direction, Texture2D texture, Color arrowColor)
        {
            Color[] color = new Color[texture.Width * texture.Height];
            texture.GetData(color);

            int arrowWidth = texture.Width / 2;
            int arrowStart = texture.Width / 4;
            int arrowEnd = arrowStart + arrowWidth;

            int hMiddle = texture.Height / 2;

            int index = 0;
            for (int y = 0; y < texture.Height; ++y)
            {
                for (int x = 0; x < texture.Width; ++x)
                {
                    if (y == hMiddle && x >= arrowStart && x < arrowEnd)
                        color[index] = arrowColor;

                    else if ((y == hMiddle - 1 || y == hMiddle + 1) && x >= arrowEnd - 2 && x < arrowEnd)
                        color[index] = arrowColor;

                    else if ((y == hMiddle - 2 || y == hMiddle + 2) && x >= arrowEnd - 4 && x < arrowEnd - 2)
                        color[index] = arrowColor;

                    else if ((y == hMiddle - 2 || y == hMiddle + 2) && x >= arrowEnd - 6 && x < arrowEnd - 4)
                        color[index] = arrowColor;

                    ++index;
                }
            }
            texture.SetData(color);
        }
        #endregion
    }
}
