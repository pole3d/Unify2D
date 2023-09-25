using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unify2D.Core
{
    public class Camera2D
    {
        protected Matrix _matrix;

        protected float _zoom;
        protected float _rotation;
        protected Vector2 _pos;

        //protected GraphicsDevice _graphicsDevice;
        public Camera2D(Vector2 position, float zoom = 1, float rotation = 0)
        {
            //_graphicsDevice = graphicsDevice;
            _pos = position;
            _zoom = zoom;
            _rotation = rotation;

            UpdateMatrix();
        }

        public float Zoom
        {
            get { return _zoom; }
            set { _zoom = value; if (_zoom < 0.1f) _zoom = 0.1f; }
        }
        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }
        public Matrix Matrix
        {
            get { return _matrix; }
        }

        // Auxiliary function to move the camera
        public void Move(Vector2 amount)
        {
            _pos += amount;
        }
        // Get set position
        public Vector2 Pos
        {
            get { return _pos; }
            set { _pos = value; }
        }

        private void UpdateMatrix()
        {
            _matrix =       // Thanks to o KB o for this solution
            Matrix.CreateTranslation(new Vector3(-_pos.X, -_pos.Y, 0)) *
            Matrix.CreateRotationZ(Rotation) *
            Matrix.CreateScale(new Vector3(Zoom, Zoom, 1));// *
            //Matrix.CreateTranslation(new Vector3(_graphicsDevice.Viewport.Width * 0.5f, _graphicsDevice.Viewport.Height * 0.5f, 0));
        }
    }
}
