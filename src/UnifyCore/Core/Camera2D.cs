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
        protected Vector2 _position;
        private bool _hasChanged = false;

        protected GraphicsDevice _graphicsDevice;
        public Camera2D(GraphicsDevice graphicsDevice, Vector2 position, float zoom = 1, float rotation = 0)
        {
            _graphicsDevice = graphicsDevice;
            _position = position;
            _zoom = zoom;
            _rotation = rotation;

            UpdateMatrix();
        }

        public float Zoom
        {
            get { return _zoom; }
            set 
            { 
                _zoom = value;
                if (_zoom < 0.1f) _zoom = 0.1f;
                _hasChanged = true;
            }
        }
        public float Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                _hasChanged = true;
            }
        }
        public float RotationEuleur
        {
            get { return _rotation * 180 / MathF.PI; }
            set
            {
                _rotation = value * MathF.PI / 180;
                _hasChanged = true;
            }
        }
        public Vector2 Position
        {
            get { return _position; }
            set
            { 
                _position = value;
                _hasChanged = true;
            }
        }
        public Matrix Matrix
        {
            get
            {
                if (_hasChanged)
                {
                    _hasChanged = false;
                    UpdateMatrix();
                }
                return _matrix;
            }
        }
        public void Move(Vector2 amount)
        {
            _position += amount;
            _hasChanged = true;
        }

        private void UpdateMatrix()
        {
            _matrix =
            Matrix.CreateTranslation(new Vector3(-_position.X, -_position.Y, 0)) *
            Matrix.CreateRotationZ(Rotation) *
            Matrix.CreateScale(new Vector3(Zoom, Zoom, 1));// *
            //Matrix.CreateTranslation(new Vector3(_graphicsDevice.Viewport.Width * 0.5f, _graphicsDevice.Viewport.Height * 0.5f, 0));

        }

        public Vector2 LocalToWorld(System.Numerics.Vector2 local)
        {
            local /= Zoom;
            
            // on divise par le zoom car Matrix.Up est calculé avec.
            Vector3 up = Matrix.Up / Zoom;
            float sin = -up.X;
            float cos = up.Y;

            Vector2 worldPosition = new Vector2(
                 (local.X * cos) + (local.Y * sin),
               -((local.X * sin) - (local.Y * cos)));

            worldPosition += Position;

            return worldPosition;
        }
        public System.Numerics.Vector2 WorldToLocal(Vector2 world)
        {
            world -= Position;

            Vector3 up = Matrix.Up;
            float sin = -up.X;
            float cos = up.Y;

            System.Numerics.Vector2 local = new System.Numerics.Vector2(
                (world.X * cos) - (world.Y * sin),
                (world.X * sin) + (world.Y * cos));

            // on multiplie pas par le zoom car Matrix.Up en prends deja compte
            //local *= Zoom;


            return local;
        }
    }
}
