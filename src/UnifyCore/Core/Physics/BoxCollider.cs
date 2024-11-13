using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Microsoft.Xna.Framework;
using System;
using Unify2D.Core;
using Unify2D.Core.Tools;
using Unify2D.Physics;

namespace UnifyCore.Core.Physics
{
    internal class BoxCollider : Collider
    {
        public float Width { get { return _width ; } set { _width = value; } }
        public float Height { get { return _height ; } set { _height = value; } }

        private float _width = 1f, _height = 1f;

        public override Body Init()
        {
            _body = BodyFactory.CreateRectangle(PhysicsSettings.World, _width * _gameObject.Scale.X, _height * _gameObject.Scale.Y, 1, (_gameObject.Position + _offset) / PhysicsSettings.UnitToPixelRatio, _gameObject.Rotation, BodyType.Kinematic);
            return _body;
        }

        public override void LateLoad(Game game, GameObject go)
        {
            if (_body == null)
            {
                _body = BodyFactory.CreateRectangle(PhysicsSettings.World, _width * _gameObject.Scale.X, _height * _gameObject.Scale.Y, 1, (_gameObject.Position + _offset) / PhysicsSettings.UnitToPixelRatio, _gameObject.Rotation, BodyType.Kinematic);
                _standalone = true;
            }
        }

        public override void PhysicsUpdate(GameCore game)
        {
            if (_standalone)
            {
                _body.Position = _gameObject.Position / PhysicsSettings.UnitToPixelRatio;
                _body.Rotation = _gameObject.Rotation;
            }
        }

        internal override void DrawGizmo()
        {
            int pixelsWidth  = (int)Math.Round(_width * _gameObject.Scale.X * PhysicsSettings.UnitToPixelRatio);
            int pixelsHeight = (int)Math.Round(_height * _gameObject.Scale.Y * PhysicsSettings.UnitToPixelRatio);

            float sin = (float) Math.Sin(_gameObject.Rotation);
            float cos = (float) Math.Cos(_gameObject.Rotation);

            Vector2 offsettedPosition = new Vector2((_offset.X * cos) + (_offset.Y * sin), ((_offset.X * sin) - (_offset.Y * cos)));

            Gizmo.DrawWireSquare(_gameObject.Position + offsettedPosition, new Vector2(pixelsWidth, pixelsHeight), 2, _gameObject.Rotation, Color.LightGreen);    
        }

    }
}
