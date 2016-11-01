using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio {
    class Player {
        public int X { get; private set; }
        public int Y { get; private set; }
        public bool Grounded { get; private set; }
        public bool InMotion { get; private set; }

        public bool Direction { get; private set; } //true = Right, false = Left
        public int Speed { get; private set; }
        public int SpeedY { get; private set; }
        public int DefaultSpeed { get; private set; }

        public Texture2D Texture { get; private set; }

        private GraphicsDevice device;

        public Player(GraphicsDevice graphicsDevice, int startingX, int startingY, bool direction, int defaultSpeed) {
            device = graphicsDevice;
            X = startingX;
            Y = startingY;
            Grounded = true; // = IsOnBlock();
            InMotion = false;
            Direction = direction;
            DefaultSpeed = defaultSpeed;
            Texture = Graphics.Entity.GeneratePlayer(graphicsDevice, 24, 24);
        }

        public void SetMoveRight() {
            if (Grounded) {
                if (!Direction)
                    Direction = true;
                if (!InMotion) {
                    Speed = DefaultSpeed;
                    InMotion = true;
                }
            }
        }

        public void SetMoveLeft() {
            if (Grounded) {
                if (Direction)
                    Direction = false;
                if (!InMotion) {
                    Speed = -1 * DefaultSpeed;
                    InMotion = true;
                }
            }
        }

        //Values subject to change according to rulesets
        public void SetMoveUp() {
            if (Grounded) {
                SpeedY = DefaultSpeed / 2;
                Grounded = false;
            }
        }

        //Values subject to change according to rulesets
        public void Update() {
            if (InMotion) {
                if (Grounded)
                    X += Speed;
                else
                    X += Speed / 2;

                if (Speed > 0)
                    Speed -= 1;

                if (Speed < 0)
                    Speed += 1;
            }

            if (!Grounded) {
                Y += SpeedY;
                SpeedY -= 1;
            }

            if (Speed == 0)
                InMotion = false;

            //if (IsOnBlock()) Grounded = true; SpeedY = 0;
        }

        //Maybe hold separate widht and height values for player?
        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Draw(Texture, new Rectangle(X, Y, Texture.Width, Texture.Height), Color.White);
        }
    }
}
