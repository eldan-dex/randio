using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2
{
    class Item
    {
        #region Public enums
        public enum ItemType
        {
            Flop
        }
        #endregion

        #region Public variables
        public Vector2 Position //this is the GLOBAL position (relative to the entire map)
        {
            get { return position; }
        }
        protected Vector2 position;

        //is this needed for an Item?
        public Rectangle BoundingRectangle //this is the LOCAL bounding rectangle (relative to the current tile)
        {
            get
            {
                Tile t = map.GetTileByIndex(CurrentTile);
                float newX = Position.X - t.Coords.Left; //this should work
                return new Rectangle((int)newX, (int)Position.Y, Width, Height);
            }
        }

        public ItemType Type { get; protected set; }
        public Texture2D Texture { get; protected set; }
        public string Name { get; protected set; }

        public int CurrentTile { get; protected set; }
        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public Entity Owner { get; protected set; }
        public bool IsPlaced { get; protected set; }

        #endregion

        #region Private/Protected variables
        //Instance variables
        protected Map map;

        #endregion

        #region Public methods
        public Item(Map map, GraphicsDevice device, ItemType type, Vector2 position, int currentTile, int width, int height, bool placed)
        {
            this.map = map;
            Type = type;
            this.position = position;
            CurrentTile = currentTile;
            Width = width;
            Height = height;
            IsPlaced = placed;
            GetName();
            CreateTexture(device);
        }

        public void Update(GameTime gameTime)
        {
            AlignWithBlockBottom();
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (IsPlaced) //Only draw the item if it's not held by any entity
            {
                Vector2 namePos = new Vector2(Position.X - (Name.Length / 2) * 4, Position.Y - 22);
                spriteBatch.DrawString(Game.font, Name, namePos, Color.Red, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
                spriteBatch.Draw(Texture, position, Color.White);
            }
        }

        public Item PickUp(Entity picker)
        {
            if (Owner == null)
            {
                Owner = picker;
                IsPlaced = false;

                return this;
            }

            else //this is not supposed to happen, but if it does, item will be given to whoever came first
                return null;
        }

        public void PutDown(int Direction)
        {
            //Put down right next to the entity in the direction the entity is looking
            if (Direction == 1)
                position = new Vector2(Owner.Position.X + Owner.Width, Owner.Position.Y);
            else
                position = new Vector2(Owner.Position.X - Width, Owner.Position.Y);

            Owner = null;
            IsPlaced = true;
        }
        #endregion

        #region Private methods
        private void GetName()
        {
            if (Type == ItemType.Flop)
                Name = "Flop";
        }

        private void CreateTexture(GraphicsDevice device)
        {
            if (Type == ItemType.Flop)
                Texture = CreateFlopTexture(device);
        }

        private Texture2D CreateFlopTexture(GraphicsDevice device)
        {
            var texture = new Texture2D(device, Width, Height);
            GraphicsHelper.DrawRectangle(texture, Color.White);
            return texture;
        }

        //ensure item is horizontally aligned with blocks
        private void AlignWithBlockBottom()
        {
            if ((Position.Y + 16) % 32 != 0)
            {
                position.Y += 32 - ((Position.Y + 16) % 32);
            }
        }
        #endregion
    }
}