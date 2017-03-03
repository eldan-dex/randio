using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2
{
    //Background of Screen tiles, solid color
    class ScreenBG : Background
    {
        #region Public methods
        //Default ctor
        public ScreenBG(GraphicsDevice device, SpriteBatch batch, int width, int height, Color[] palette)
        {
            this.palette = palette;
            CreateBackgroundTexture(device, batch, width, height);
            CreateBlockTexture(device, batch, Block.Width, Block.Height);
            BlockTopmostTexture = BlockTexture;
        }

        //Generates the background texture
        public void CreateBackgroundTexture(GraphicsDevice device, SpriteBatch batch, int width, int height)
        {
            Texture = new RenderTarget2D(device, width, height);

            device.SetRenderTarget(Texture);
            device.Clear(palette[0]);
            device.SetRenderTarget(null);
        }

        //Generates texture of individual blocks
        public void CreateBlockTexture(GraphicsDevice device, SpriteBatch batch, int width, int height)
        {
            var texture = new Texture2D(device, width, height);

            GraphicsHelper.FillRectangle(texture, palette[1]);

            BlockTexture = texture;
        }
        #endregion
    }
}