using Microsoft.Xna.Framework;
using System;

namespace Unify2D.Core
{
    /// <summary>
    /// The <see cref="CameraEditor"/> class, is only meant to be used in the editor.
    /// It implements the <see cref="ICamera2D"/> interface. This class
    /// provides properties for background color, zoom, rotation, viewport size, position,
    /// and a derived transformation matrix. It enables 2D camera functionality within the <see cref="Toolbox.GameToolbox"/>.
    /// </summary>
    public class CameraEditor : ICamera2D
    {
        protected Matrix _matrix;

        protected float _zoom;
        protected float _rotation;
        protected Vector2 _position;
        protected Vector2 _viewport;

        public CameraEditor(Vector2 viewport, Vector2 position, float zoom = 1, float rotation = 0)
        {
            _viewport = viewport;
            _position = position;
            ZoomLevel = zoom;
            _rotation = rotation;

            ((ICamera2D)this).UpdateMatrix();
        }
        public bool HasChanged { get; set; }
        public Color Background { get; set; } = Color.CornflowerBlue;
        private float _zoomLevel;
        public float ZoomLevel
        {
            get { return _zoomLevel; }
            set { 
                _zoomLevel = MathF.Max(value, 0.05f);
                Zoom = 1 / _zoomLevel;
            }
        }
        public float Zoom
        {
            get { return _zoom; }
            set 
            { 
                _zoom = value;
                HasChanged = true;
            }
        }
        public float Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                HasChanged = true;
            }
        }
        public float RotationEuleur
        {
            get { return _rotation * 180 / MathF.PI; }
            set
            {
                _rotation = value * MathF.PI / 180;
                HasChanged = true;
            }
        }
        public Vector2 Viewport
        {
            get { return _viewport; }
            set
            {
                _viewport = value;
                HasChanged = true;
            }
        }
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                HasChanged = true;
            }
        }
        public Matrix Matrix
        {
            get
            {
                if (HasChanged)
                {
                    ((ICamera2D)this).UpdateMatrix();
                }
                return _matrix;
            }
            set { _matrix = value; }
        }

        internal Vector2 LocalToWorld(Vector2 mousePosition)
        {
            return ((ICamera2D)this).LocalToWorld(mousePosition);
        }

        internal Vector2 WorldToViewport(Vector2 world)
        {
            return ((ICamera2D)this).WorldToViewport(world);
        }

        //some shortcuts
        public Vector2 TopLeft => ((ICamera2D)this).LocalToWorld(new Vector2(0, 0));
        public Vector2 BottomRight => ((ICamera2D)this).LocalToWorld(new Vector2(Viewport.X, Viewport.Y));
    }
}
