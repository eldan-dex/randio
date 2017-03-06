using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2
{
    //Helper class for LSystemBG, implements L-System basics
    class LSystem
    {
        #region Public variables
        public string State { get; private set; }
        public int Step { get; private set; }
        public double Angle { get; private set; }
        #endregion

        #region Private variables
        private List<Rule> rules;
        #endregion

        #region Public methods
        //Default ctor
        public LSystem(string template, int stepLength, double angle, List<Rule> rules)
        {
            State = template;
            Step = stepLength;
            Angle = angle;
            this.rules = rules;
        }

        //Copying ctor
        public LSystem(LSystem other)
        {
            State = other.State;
            Step = other.Step;
            Angle = other.Angle;
            rules = other.rules;
        }

        //Adds a rule to the ruleset
        public void AddRule(Rule newRule)
        {
            rules.Add(newRule);
        }


        //Applies a rule to the axiom
        public string ApplyRule(string element)
        {
            foreach (Rule rule in rules)
                if (rule.Element == element)
                    return rule.Replace;
            return element; //or empty string?
        }

        //Iterates over the system, applying all rules to the axom
        public void Iterate()
        {
            string newState = "";
            foreach (char c in State)
            {
                newState += ApplyRule(c.ToString());
            }
            State = newState;
        }

        //Iterates n times
        public void Iterate(int times)
        {
            for (int i = 0; i < times; ++i)
                Iterate();
        }

        //Rules
        public class Rule
        {
            public string Element { get; private set; }
            public string Replace { get; private set; }

            public Rule(string element, string replace)
            {
                Element = element;
                Replace = replace;
            }
        }
        #endregion
    }

    //Turtle used to draw L-Systems step by step
    class Turtle
    {
        #region Private variables
        SpriteBatch spriteBatch;
        GraphicsDevice device;
        Texture2D lineTexture;
        Moment now;
        Moment defaultMoment;
        Stack<Moment> time;
        Color currentColor;
        Color[] palette;
        #endregion

        #region Public methods
        //Derfault ctor
        public Turtle(GraphicsDevice device, SpriteBatch spriteBatch, Vector2 startingPosition, double startingAngle, Color currentColor, Color[] palette)
        {
            this.palette = palette;
            this.device = device;
            this.spriteBatch = spriteBatch;
            this.currentColor = currentColor;
            defaultMoment = new Moment(startingPosition, startingAngle);

            lineTexture = new Texture2D(device, 1, 1);
            lineTexture.SetData(new Color[] { Color.White });
        }

        //Sets turtle default values
        public void SetDefaults(Vector2 currentPosition, double angle)
        {
            defaultMoment.currentPosition = currentPosition;
            defaultMoment.angle = angle;
        }

        //Draws the whole L-System to a new RenderTarget2D
        public RenderTarget2D DrawSystem(LSystem System, int canvasWidth, int canvasHeight)
        {
            RenderTarget2D target = new RenderTarget2D(device, canvasWidth, canvasHeight);
            device.Clear(Color.Black);
            return DrawSystem(System, target);
        }

        //Draws the whole L-System to an existing RenderTarget2D
        public RenderTarget2D DrawSystem(LSystem System, RenderTarget2D target)
        {
            time = new Stack<Moment>();
            now = new Moment(defaultMoment);

            Random r = new Random((int)DateTime.Now.Ticks);

            foreach (char c in System.State)
            {
                if (c == 'F' || c == 'G' || c == 'H')
                    MoveTurtle(System.Step, target);

                else if (c == '0')
                    currentColor = palette[2];

                else if (c == '1')
                    currentColor = palette[3];

                else if (c == '2')
                    currentColor = palette[4];

                else if (c == '3')
                    currentColor = palette[5];

                else if (c == '4')
                    currentColor = palette[6];

                else if (c == '+')
                    now.angle -= System.Angle;

                else if (c == '-')
                    now.angle += System.Angle;

                else if (c == '[')
                    time.Push(new Moment(now));

                else if (c == ']')
                    now = time.Pop();

            }

            return target;
        }
        #endregion

        #region Private methods
        //Executes one turtle step
        private void MoveTurtle(int length, RenderTarget2D target)
        {
            Vector2 newPos = now.currentPosition + (length * GeometryHelper.AngleToVector(GeometryHelper.DegToRad(now.angle)));
            GraphicsHelper.DrawSquareFromVector(spriteBatch, lineTexture, now.currentPosition, newPos, currentColor, length);
            now.currentPosition = newPos;
        }
        #endregion

        #region Nested classes
        //Turtle proeprties in a given moment, allows for "teleportation" backwards
        class Moment
        {
            #region Public variables
            public Vector2 currentPosition;
            public double angle;
            #endregion

            #region Public methods
            public Moment()
            {
            }

            public Moment(Moment other)
            {
                currentPosition = other.currentPosition;
                angle = other.angle;
            }

            public Moment(Vector2 position, double angle)
            {
                currentPosition = position;
                this.angle = angle;
            }
            #endregion
        }
        #endregion
    }
}
