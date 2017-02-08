using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2
{
    class TemplateBG : Background
    {
        #region Private variables
        //Put any global variable declarations here.
        #endregion

        #region Public methods
        public TemplateBG(GraphicsDevice device, SpriteBatch batch, int width, int height)
        {
            CreateBackgroundTexture(device, batch, width, height);
            CreateBlockTexture(device, batch, Block.Width, Block.Height);
            CreateTopmostBlockTexture(device, batch, Block.Width, Block.Height);
        }

        public void CreateBackgroundTexture(GraphicsDevice device, SpriteBatch batch, int width, int height)
        {
            Texture = new RenderTarget2D(device, width, height);

            //Initialize whatever you need here
            
            device.SetRenderTarget(Texture);
            device.Clear(Color.Black);
            batch.Begin();

            //Draw onto batch here

            batch.End();
            device.SetRenderTarget(null);
        }

        public void CreateBlockTexture(GraphicsDevice device, SpriteBatch batch, int width, int height)
        {
            var texture = new Texture2D(device, width, height);
            
            //While tile background is RenderTarget2D, Blocks are Texture2D. If that doesn't suit you, you can use RenderTarget2D and return .GetTexture() in the end

            BlockTexture = texture;
        }

        //This function will generate a separate texture for all blocks, which are the topmost of a stack of blocks.
        public void CreateTopmostBlockTexture(GraphicsDevice device, SpriteBatch batch, int width, int height)
        {
            BlockTopmostTexture = BlockTexture; //Leave this here for topmost bloks to be identical to normal blocks. Replace with own texture generation to make them different.
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