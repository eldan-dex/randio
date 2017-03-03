using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2
{
    //Item - holds ability bonuses, also can be a quest object (bring item X to Y)
    class Item
    {
        #region Public enums
        //Item types (bonus types)
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
        private Vector2 position;

        public ItemType Type { get; private set; }
        public Texture2D Texture { get; private set; }
        public ItemProperties Properties { get; private set; }
        public int CurrentTile { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Entity Owner { get; private set; }
        public bool IsPlaced { get; private set; }

        #endregion

        #region Private variables
        private Map map;
        #endregion

        #region Public methods
        //Default ctor
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

        //Checks whether the item is out of the map (and places it back on the map), applies gravity to the item (item falls down)
        public void Update(GameTime gameTime)
        {
            if (position.Y > Game.HEIGHT)
                position.Y = 0;

            ItemGravity();
        }

        //Draws the item
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (IsPlaced) //Only draw the item if it's not held by any entity
            {
                Vector2 namePos = new Vector2(Position.X - (Properties.Name.Length / 2) * 4, Position.Y - 22);
                spriteBatch.DrawString(Game.font, Properties.Name, namePos, Color.Red, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
                spriteBatch.Draw(Texture, position, Color.White);
            }
        }

        //Item is being picked up by an entity
        public Item PickUp(Entity picker)
        {
            if (Owner == null)
            {
                Owner = picker;
                IsPlaced = false;

                return this;
            }

            else
                return null;
        }

        //Put the Item down in the direction the player is looking (and don't put it inside blocks)
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
        //Initializes Item properties
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
                        Properties.SpeedBonus = (potence - 1f) / 2;
                        break;
                    case ItemType.HP:
                        Properties.Name = "MORE HP";
                        Properties.Adjective = "Healthly";
                        Properties.HPBonus = (int)(10 * potence);
                        break;
                }
            }
        }

        //Creates the item texture (small white square)
        private void CreateTexture(GraphicsDevice device)
        {
            var texture = new Texture2D(device, Width, Height);
            GraphicsHelper.FillRectangle(texture, Color.White);
            Texture = texture;
        }

        //Ensure item is horizontally aligned with blocks and that it falls down when dropped
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