using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Randio_2 {
    class GraphicsHelper
    {
        #region Public methods
        public static void DrawRectangle(Texture2D texture, Color fill)
        {
            Color[] color = new Color[texture.Width * texture.Height];
            texture.GetData(color); //Texture size limit is 4096*4096. Textures bigger than that crash when being converted to Color[]

            for (int i = 0; i < texture.Width * texture.Height; ++i)
                color[i] = fill;

            texture.SetData(color);
        }

        //stack rectangles instead of spray
        public static void DrawSplotch(Texture2D texture, Color fill)
        {
            Color[] color = new Color[texture.Width * texture.Height];
            texture.GetData(color);

            int maxDistance = (texture.Width + texture.Height) / 2;
            int centerX = texture.Width / 2;
            int centerY = texture.Height / 2;

            int currentX = 0;
            int currentY = 0;
            for (int i = 0; i < color.Length; ++i)
            {
                int centerDistance = Math.Abs(centerX - currentX) + Math.Abs(centerY - currentY);
                int probability = (int)(centerDistance / ((double)maxDistance / 100));
                if (probability > 75)
                    probability = 100;
                color[i] = AlgorithmHelper.GetRandom(0, 100) >= probability ? fill : Color.Transparent;
                ++currentX;
                if (currentX == texture.Width)
                {
                    currentX = 0;
                    ++currentY;
                }
            }

            texture.SetData(color);
        }

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

        public static void CreateDarkerOutline(Texture2D texture, int outlineWidth)
        {
            //find most prevalent color, darken by 20%, OutlineTexture
            Color[] colors = new Color[texture.Width * texture.Height];
            texture.GetData(colors);
            var freq = FindFrequentColor(colors);
            var outlineColor = Color.Lerp(freq, Color.Black, 0.5f); //check

            OutlineRectangle(texture, outlineColor, outlineWidth);
        }

        //thx to ReyeMe
        public static Color FindFrequentColor(Color[] colors)
        {
            Dictionary<Color, int> counter = new Dictionary<Color, int>();

            foreach (Color c in colors)
            {
                if (!counter.ContainsKey(c)) counter.Add(c, 1);
                else counter[c]++;
            }

            return counter.OrderBy(key => key.Value).First().Key;
        }

        public static Texture2D CopyTexture(Texture2D texture)
        {
            Texture2D result = new Texture2D(texture.GraphicsDevice, texture.Width, texture.Height);
            Color[] color = new Color[texture.Width * texture.Height];
            texture.GetData(color);
            result.SetData(color);
            return result;
        }


        //************** Stuff that actually draws something

        public static void DrawLineFromVector(SpriteBatch spriteBatch, Texture2D texture, Vector2 a, Vector2 b, Color color)
        { //color is a temporary hack, normally the color would be in the texture
            Vector2 edge = b - a;

            // calculate angle to rotate line
            double angle = Math.Atan2(edge.Y, edge.X);

            spriteBatch.Begin();
            spriteBatch.Draw(texture, new Rectangle((int)a.X, (int)a.Y, (int)edge.Length(), 2), null, color, (float)angle, new Vector2(0, 0), SpriteEffects.None, 0);
            spriteBatch.End();
        }

        public static void DrawSquareFromVector(SpriteBatch spriteBatch, Texture2D texture, Vector2 a, Vector2 b, Color color, int size)
        { //again, color is a hack
            Vector2 edge = b - a;

            // calculate angle to rotate line
            double angle = Math.Atan2(edge.Y, edge.X);

            //spriteBatch.Begin();
            spriteBatch.Draw(texture, new Rectangle((int)a.X, (int)a.Y, size, size), null, color, 0f, new Vector2(0, 0), SpriteEffects.None, 0);
            //spriteBatch.End();
        }

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

        public static List<Color> GetColorPalette()
        {
            int palette = AlgorithmHelper.GetRandom(0, 1);
            List<Color> result = new List<Color>();
            switch(palette)
            {
                case 0:
                    result.AddRange(new Color[] { Color.Black, Color.Violet, Color.LightBlue, Color.LightYellow, Color.Gray, Color.White, Color.DarkCyan });
                    break;
                case 1:
                    result.AddRange(new Color[] { Color.Black, Color.Violet, Color.LightBlue, Color.LightYellow, Color.Gray, Color.White, Color.DarkCyan });
                    break;
                case 2:
                    result.AddRange(new Color[] { Color.Black, Color.Violet, Color.LightBlue, Color.LightYellow, Color.Gray, Color.White, Color.DarkCyan });
                    break;
                default:
                    return null;
            }
            return result;
        }
        #endregion
    }
}
