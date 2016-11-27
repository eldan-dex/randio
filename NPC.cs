using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2 {
    class NPC : Entity {
        //Public variables
        //********************************************************************************//
        public Tile ParentTile { get; private set; }


        //Public methods
        //********************************************************************************//
        public NPC(GraphicsDevice graphicsDevice, Map map, Vector2 position, int parentTile, int width, int height) : base(map, position, parentTile, width, height) {
            ParentTile = map.GetTileByIndex(parentTile);
            Texture = CreateTexture(graphicsDevice);
        }

        //Public methods
        //********************************************************************************//
        private Texture2D CreateTexture(GraphicsDevice graphicsDevice) {
            var texture = new Texture2D(graphicsDevice, Width, Height);

            //Temporary placeholder code, will call texture generation here
            GraphicsHelper.FillRectangle(texture, Color.Yellow);
            GraphicsHelper.OutlineRectangle(texture, Color.Brown, 2);

            return texture;
        }
    }
}
