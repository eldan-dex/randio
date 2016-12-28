﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Randio_2 {
    //Thanks to ReyeMe for contributing this background
    class CityBG : Background {
        #region Public methods
        public CityBG( GraphicsDevice device, SpriteBatch batch, int width, int height )
        {
            CreateBackgroundTexture(device, batch, width, height);
            CreateBlockTexture(device, batch, Block.Width, Block.Height);
        }

        public void CreateBackgroundTexture( GraphicsDevice device, SpriteBatch batch, int width, int height )
        {
            Texture = new RenderTarget2D(device, width, height);
            List<House> buildings = new List<House>();

            for (int zIndex = 0; zIndex < 3; zIndex++)
            {
                int fromLeft = 0;
                while (fromLeft < width)
                {
                    //build house
                    int stories = AlgorithmHelper.GetRandom(50, 140) * (4 - zIndex);
                    House house = new House(fromLeft,height - stories, stories, zIndex);
                    int step = (house.dimensions.Width / 12) + zIndex;

                    //fill house with windows
                    for (int X = (int)(step * 1.5); X < house.dimensions.Width - step; X += (int)(step*1.5))
                        for (int Y = step; Y < house.dimensions.Height; Y += step + step/2) {
                            house.windows.Add(new Window(new Rectangle(house.dimensions.X+X, house.dimensions.Y+Y, step, step), AlgorithmHelper.GetRandom(0, 4)));
                        }

                    //add house to the city
                    buildings.Add(house);

                    //build street
                    fromLeft += house.dimensions.Width + AlgorithmHelper.GetRandom(10, 50);
                }
            }

            device.SetRenderTarget(Texture);
            device.Clear(Color.Black);
            batch.Begin();
            
            foreach (House building in buildings) {
                //draw house
                RenderTarget2D house = new RenderTarget2D(device, building.dimensions.Width, building.dimensions.Height);
                GraphicsHelper.DrawRectangle(house, zIndexToColor(building.zIndex));
                batch.Draw(house, building.dimensions, Color.White);
                foreach (Window window in building.windows) {
                    //draw windows on house
                    RenderTarget2D house_window = new RenderTarget2D(device, window.dimensions.Width, window.dimensions.Height);
                    GraphicsHelper.DrawRectangle(house_window, Color.Multiply(window.color, (0.5f * building.zIndex) + 1f));
                    batch.Draw(house_window, window.dimensions, Color.White);
                }
            }

            batch.End();
            device.SetRenderTarget(null);
        }

        public void CreateBlockTexture( GraphicsDevice device, SpriteBatch batch, int width, int height )
        {
            var texture = new Texture2D(device, width, height);
            
            BlockTexture = texture;
        }
        #endregion

        #region Private methods
        Color zIndexToColor( int zIndex ) {
            switch (zIndex) {
                case 0:
                    return new Color(2,9,38);
                case 1:
                    return new Color(3,88,119);
                case 2:
                    return new Color(0,115,130);
                default:
                    return new Color(18,117,114);
            }
        }
        #endregion

        #region Nested classes
        class House {
            public Rectangle dimensions;
            public int zIndex;     
            public List<Window> windows;

            public House( int X, int Y, int height, int zIndex ) {

                int width = zIndex*5 + 200;
                                
                this.zIndex = zIndex;
                
                dimensions = new Rectangle(X,Y,width,height);

                windows = new List<Window>();
            }
        }

        class Window {
            public Rectangle dimensions;
            public Color color;

            public Window( Rectangle dim, int rnd ) {
                dimensions = dim;
                switch (rnd) {
                    case 0:
                        color = Color.Yellow;
                        break;
                    case 1:
                        color = Color.LightBlue;
                        break;
                    case 2:
                        color = Color.Black;
                        break;
                    default:
                        color = Color.Sienna;
                        break;
                }
                
            }
        }
        #endregion
    }
}