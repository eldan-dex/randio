using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2 {
    public class Camera {

        #region Public variables
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public float Zoom { get; set; }
        public Vector2 Origin { get; set; }
        #endregion 

        #region Private variables
        private readonly Viewport _viewport;
        #endregion 

        #region Public methods
        public Camera(Viewport viewport) {
            _viewport = viewport;

            Rotation = 0;
            Zoom = 1;
            Origin = new Vector2(viewport.Width / 2f, viewport.Height / 2f);
            Position = Vector2.Zero;
        }

        public void CenterXTo(Rectangle rect) {
            float selfX = Position.X;
            float rectCenter = rect.X + (rect.Width / 2);
            selfX = rectCenter - (_viewport.Width / 2);

            if (selfX < 0)
                selfX = 0;

            Position = new Vector2(selfX, Position.Y);
        }

        public Matrix GetViewMatrix() {
            return
                Matrix.CreateTranslation(new Vector3(-Position, 0.0f)) *
                Matrix.CreateTranslation(new Vector3(-Origin, 0.0f)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(Zoom, Zoom, 1) *
                Matrix.CreateTranslation(new Vector3(Origin, 0.0f));
        }
        #endregion
    }
}
