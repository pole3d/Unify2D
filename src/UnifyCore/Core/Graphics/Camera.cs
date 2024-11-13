using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Unify2D.Core.Tools;

namespace Unify2D.Core.Graphics
{
    /// <summary>
    /// The <see cref="Camera"/> class extends the functionality of a <see cref="GameObject"/>'s
    /// <see cref="Component"/> by implementing the <see cref="ICamera2D"/> interface. This class
    /// provides properties for background color, zoom, rotation, viewport size, position,
    /// and a derived transformation matrix. It can be attached to a game object to enable 
    /// 2D camera functionality within the game world.
    /// </summary>
    public class Camera : Component, ICamera2D
    {
        protected Matrix _matrix;

        private float _zoomLevel = 1;
        protected float _zoom = 1;
        protected float _rotation = 0;
        protected Vector2 _viewport = new Vector2(1920, 1080);
        protected Vector2 _lastPosition;
        protected bool _hasChanged = false;

        [JsonIgnore]
        public bool HasChanged 
        { 
            get { return _hasChanged || _lastPosition != _gameObject.Position; } 
            set {
                if (_gameObject != null)
                {
                    _lastPosition = _gameObject.Position;
                }
                _hasChanged = value; 
            }
        }
        public Color Background { get; set; } = Color.CornflowerBlue;
        public float ZoomLevel
        {
            get { return _zoomLevel; }
            set
            {
                _zoomLevel = Math.Max(value, 0.05f);
                Zoom = 1 / _zoomLevel;
            }
        }
        [JsonIgnore]
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
        [JsonIgnore]
        public float RotationEuleur
        {
            get { return _rotation * 180 / (float) Math.PI; }
            set
            {
                _rotation = value * (float) Math.PI / 180;
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
        [JsonIgnore]
        public Vector2 Position
        {
            get { return _gameObject.Position; }
            set
            {
                _gameObject.Position = value;
                _lastPosition = value;
                HasChanged = true;
            }
        }
        [JsonIgnore]
        public Matrix Matrix
        {
            get
            {
                if (HasChanged)
                {
                    _lastPosition = _gameObject.Position;
                    ((ICamera2D)this).UpdateMatrix();
                }
                return _matrix;
            }
            set { _matrix = value; }
        }

        public static Camera Main;

        public void UpdateMatrix()
        {
            HasChanged = false;
        
            Matrix =
            Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
            Matrix.CreateRotationZ(Rotation) *
            Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
            Matrix.CreateTranslation(new Vector3(Viewport.X * 0.5f, Viewport.Y * 0.5f, 0));
        }

        Vector2 ICamera2D.LocalToWorld(Vector2 local)
        {
            local -= Viewport * 0.5f;
        
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

        Vector2 ICamera2D.WorldToViewport(Vector2 world)
        {
            world -= Position;
        
            Vector3 up = Matrix.Up;
            float sin = -up.X;
            float cos = up.Y;
        
            Vector2 local = new Vector2(
                (world.X * cos) - (world.Y * sin),
                (world.X * sin) + (world.Y * cos));
        
            // on multiplie pas par le zoom car Matrix.Up en prends deja compte
            //local *= Zoom;
        
            local += Viewport * 0.5f;
        
            return local;
        }

        internal Vector2 LocalToWorld(Vector2 mousePosition)
        {
            return ((ICamera2D)this).LocalToWorld(mousePosition);
        }

        internal Vector2 WorldToViewport(Vector2 world)
        {
            return ((ICamera2D)this).WorldToViewport(world);
        }

        public override void Initialize(GameObject go)
        {
            base.Initialize(go);

            if (Main == null) Main = this;
            
            ((ICamera2D)this).UpdateMatrix();
        }


        internal override void DrawGizmo()
        {
            Gizmo.DrawWireSquare(TopLeft, BottomRight, (int)Math.Max(5 * ZoomLevel, 5));
        }

        //some shortcuts
        public Vector2 TopLeft => ((ICamera2D)this).LocalToWorld(new Vector2(0, 0));
        public Vector2 BottomRight => ((ICamera2D)this).LocalToWorld(Viewport);
    }
}
