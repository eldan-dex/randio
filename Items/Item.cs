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
            HP,
            Weapon,
            Armor,
            Speed
        }
        public const int TypeCount = 4;
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
        public ItemProperties Properties { get; protected set; }

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
        public Item(Map map, GraphicsDevice device, ItemType type, int index, Vector2 position, int currentTile, int width, int height, bool placed = true, Entity owner = null, ItemProperties properties = null)
        {
            this.map = map;
            Type = type;
            this.position = position;
            CurrentTile = currentTile;
            Width = width;
            Height = height;
            IsPlaced = placed;
            Owner = owner;
            Properties = properties;
            InitializeItem();
            Properties.Name += " " + index.ToString();
            CreateTexture(device);
        }

        public void Update(GameTime gameTime)
        {
            ItemGravity();
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (IsPlaced) //Only draw the item if it's not held by any entity
            {
                Vector2 namePos = new Vector2(Position.X - (Properties.Name.Length / 2) * 4, Position.Y - 22);
                spriteBatch.DrawString(Game.font, Properties.Name, namePos, Color.Red, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
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
            Vector2 newPos;
            if (Direction == 1)
                newPos = new Vector2(Owner.Position.X + Owner.Width, Owner.Position.Y);
            else
                newPos = new Vector2(Owner.Position.X - Width, Owner.Position.Y);

            //If we're trying to place the item into a block, place it where we stand instead
            if (map.IsBlock(newPos))
                newPos = map.Player.Position;

            position = newPos;

            Owner = null;
            IsPlaced = true;
        }
        #endregion

        #region Private methods
        private void InitializeItem()
        {
            if (!IsPlaced && Owner == null)
                throw new ArgumentException("Cannot create an unobtainable item");

            if (Properties == null)
            {
                Properties = new ItemProperties();
                float potence = (map.TileCount) / (map.TileCount - Math.Max(1f, CurrentTile));
                switch (Type)
                {
                    case ItemType.Armor:
                        Properties.Name = "BETTER RESISTANCE";
                        Properties.Adjective = "Resilient";
                        Properties.ArmorBonus = 2f * potence;
                        break;
                    case ItemType.Weapon:
                        Properties.Name = "STRONGER PUNCH";
                        Properties.Adjective = "Strong";
                        Properties.StrengthBonus = 4f * potence;
                        break;
                    case ItemType.Speed:
                        Properties.Name = "FASTER MOVEMENT";
                        Properties.Adjective = "Fast";
                        Properties.SpeedBonus = 2f * potence;
                        break;
                    case ItemType.HP:
                        Properties.Name = "MORE HP";
                        Properties.Adjective = "Healthly";
                        Properties.HPBonus = (int)(10 * potence);
                        break;
                }
            }
        }

        private void CreateTexture(GraphicsDevice device)
        {
            //if (Type == ItemType.Flop)
                Texture = CreateFlopTexture(device);
        }

        private Texture2D CreateFlopTexture(GraphicsDevice device)
        {
            var texture = new Texture2D(device, Width, Height);
            GraphicsHelper.DrawRectangle(texture, Color.White);
            return texture;
        }

        //ensure item is horizontally aligned with blocks
        private void ItemGravity()
        {
            if (!map.IsBlock(position + new Vector2(0, Height)))
                position.Y += Height / 2;

            else if (!map.IsBlock(position + new Vector2(0, 1)))
                position.Y += Block.Height - position.Y % Block.Height - Height;
        }
        #endregion
    }
}