using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Num = System.Numerics;

namespace Unify2D.Core
{
    public class Camera2D
    {
        protected Matrix _matrix;

        public Color Background = Color.CornflowerBlue;
        protected float _zoom;
        protected float _rotation;
        protected Vector2 _position;
        protected Vector2 _resolution;
        private bool _hasChanged = false;

        public Camera2D(Vector2 resolution, Vector2 position, float zoom = 1, float rotation = 0)
        {
            _resolution = resolution;
            _position = position;
            ZoomLevel = zoom;
            _rotation = rotation;

            UpdateMatrix();
        }

        private float _zoomLevel;
        public float ZoomLevel
        {
            get { return _zoomLevel; }
            set { 
                _zoomLevel = value;
                Zoom = _zoomLevel >= 1 ? 1 / _zoomLevel : 2 - _zoomLevel;
            }
        }
        public float Zoom
        {
            get { return _zoom; }
            private set 
            { 
                _zoom = value;
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
        public Vector2 Resolution
        {
            get { return _resolution; }
            set
            {
                _resolution = value;
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
            _hasChanged = false;

            _matrix =
            Matrix.CreateTranslation(new Vector3(-_position.X, -_position.Y, 0)) *
            Matrix.CreateRotationZ(Rotation) *
            Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
            Matrix.CreateTranslation(new Vector3(_resolution.X * 0.5f, _resolution.Y * 0.5f, 0));

        }

        public Vector2 LocalToWorld(Num.Vector2 local)
        {
            local -= new Num.Vector2(_resolution.X * 0.5f, _resolution.Y * 0.5f);

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
        public Num.Vector2 WorldToViewport(Vector2 world)
        {
            world -= Position;

            Vector3 up = Matrix.Up;
            float sin = -up.X;
            float cos = up.Y;

            Num.Vector2 local = new Num.Vector2(
                (world.X * cos) - (world.Y * sin),
                (world.X * sin) + (world.Y * cos));

            // on multiplie pas par le zoom car Matrix.Up en prends deja compte
            //local *= Zoom;

            local += new Num.Vector2(_resolution.X * 0.5f, _resolution.Y * 0.5f);

            return local;
        }
    }
}
