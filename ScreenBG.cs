using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2
{
    class ScreenBG : Background
    {
        #region Private variables
        //Put any global variable declarations here.
        #endregion

        #region Public methods
        public ScreenBG(GraphicsDevice device, SpriteBatch batch, int width, int height)
        {
            CreateBackgroundTexture(device, batch, width, height);
            CreateBlockTexture(device, batch, Block.Width, Block.Height);
            BlockTopmostTexture = BlockTexture;
        }

        public void CreateBackgroundTexture(GraphicsDevice device, SpriteBatch batch, int width, int height)
        {
            Texture = new RenderTarget2D(device, width, height);

            //Initialize whatever you need here
            
            device.SetRenderTarget(Texture);
            device.Clear(Color.Beige);
            batch.Begin();

            //Draw onto batch here

            batch.End();
            device.SetRenderTarget(null);
        }

        public void CreateBlockTexture(GraphicsDevice device, SpriteBatch batch, int width, int height)
        {
            var texture = new Texture2D(device, width, height);

            GraphicsHelper.DrawRectangle(texture, Color.Red);

            BlockTexture = texture;
        }
        #endregion

        #region Private methods
        //Any additional methods (for this class at least) are to be placed here
        #endregion

        #region Nested classes
        //If you need to crate any additional classes, nest them here.
        #endregion
    }
}


/* 
 * For geometry, use the GeometryHelper class
 * Drawing can be simplified by making use of what's in the GrapicsHelper
 * If you needed to work with text, StringHelper is here for you
 * And AlgorithmHelper is here to provide you with random and stuff
 * - for the sake of consistency, don't spawn new Random instances. Use AlgorithmHelper.GetRandom(int min, int max) or GetBiasedRandom (if you want your randoms to be more dense towards one end of the spectrum)
 * Feel free to add your own methods to the helper classes, if you think they might be useful for more than just creating this background
 * For reference, check ShapesBG or LSystemBG
 * Good luck!
 */