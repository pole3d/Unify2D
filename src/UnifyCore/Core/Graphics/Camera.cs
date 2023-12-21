using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Core.Tools;
using Num = System.Numerics;

namespace Unify2D.Core.Graphics
{
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
                _zoomLevel = MathF.Max(value, 0.05f);
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
            Gizmo.DrawWireSquare(TopLeft, BottomRight, (int)MathF.Max(5 * ZoomLevel, 5));
        }

        //some shortcuts
        public Vector2 TopLeft => ((ICamera2D)this).LocalToWorld(new Vector2(0, 0));
        public Vector2 BottomRight => ((ICamera2D)this).LocalToWorld(Viewport);
    }
}
