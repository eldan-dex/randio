using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2 {
    class Entity {
        public Tile ParentTile { get; private set; }

        public Entity(Tile parentTile) {
            ParentTile = parentTile;

        }

        public void Update(GameTime gameTime) {

        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch) {

        }
    }
}
