using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2
{
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
        //Ctors
        public LSystem()
        {
            rules = new List<Rule>();
        }

        public LSystem(string template) : this()
        {
            SetTemplate(template);
        }

        public LSystem(string template, int stepLength, double angle) : this()
        {
            SetTemplate(template);
            SetStepLength(stepLength);
            SetAngle(angle);
        }

        public LSystem(string template, List<Rule> rules)
        {
            this.rules = rules;
            SetTemplate(template);
        }

        public LSystem(string template, int stepLength, double angle, List<Rule> rules) : this()
        {
            SetTemplate(template);
            SetStepLength(stepLength);
            SetAngle(angle);
            this.rules = rules;
        }

        //Copying ctor
        public LSystem(LSystem other)
        {
            SetTemplate(other.State);
            SetStepLength(other.Step);
            SetAngle(other.Angle);
            rules = other.rules;
        }

        //Preparation
        public void SetTemplate(string template)
        {
            State = template;
        }

        public void SetStepLength(int stepLength)
        {
            Step = stepLength;
        }

        public void SetAngle(double angle)
        {
            Angle = angle;
        }

        public void AddRule(Rule newRule)
        {
            rules.Add(newRule);
        }


        //Iteration
        public string ApplyRule(string element)
        {
            foreach (Rule rule in rules)
                if (rule.Element == element)
                    return rule.Replace;
            return element; //or empty string?
        }

        public void Iterate()
        {
            string newState = "";
            foreach (char c in State)
            {
                newState += ApplyRule(c.ToString());
            }
            State = newState;
        }

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
        #endregion

        #region Public methods
        public Turtle(GraphicsDevice device, SpriteBatch spriteBatch, Vector2 startingPosition, double startingAngle, Color currentColor)
        {
            this.device = device;
            this.spriteBatch = spriteBatch;
            this.currentColor = currentColor;
            defaultMoment = new Moment(startingPosition, startingAngle);

            lineTexture = new Texture2D(device, 1, 1);
            lineTexture.SetData(new Color[] { Color.White });
        }

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

                //TODO: rework color system
                else if (c == '0')
                    currentColor = Color.White;

                else if (c == '1')
                    currentColor = Color.Brown;

                else if (c == '2')
                    currentColor = Color.Green;

                else if (c == '3')
                    currentColor = Color.Red;

                else if (c == '4')
                    currentColor = Color.Blue;

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
        private void MoveTurtle(int length, RenderTarget2D target)
        {
            Vector2 newPos = now.currentPosition + (length * GeometryHelper.AngleToVector(GeometryHelper.DegToRad(now.angle)));

            //device.SetRenderTarget(target);
            //GraphicsHelper.DrawLine(spriteBatch, lineTexture, now.currentPosition, newPos, currentColor); //for drawing lines (conventional l-systems)
            GraphicsHelper.DrawSquareFromVector(spriteBatch, lineTexture, now.currentPosition, newPos, currentColor, length);
            //device.SetRenderTarget(null);

            now.currentPosition = newPos; //move next line to start inside parent?
        }
        #endregion

        #region Nested classes
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
