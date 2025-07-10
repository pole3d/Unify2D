using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Microsoft.Xna.Framework;
using Unify2D.Core;
using Unify2D.Physics;

namespace UnifyCore.Core.Physics
{
    //OUTDATED, NEEDS TO BE COMPLETELY REFACTORED

    internal class CircleCollider: Component
    {
        public float Radius{ get { return _radius; } set { _radius = value; } }
        public Vector2 Offset { get { return _offset; } set { _offset = value; } }

        private Vector2  _offset;
        private Body _body;
        private float _radius = 1f;

        public override void Load(Game game, GameObject go)
        {
            Rigidbody rb = _gameObject.GetComponent<Rigidbody>();

            if (rb == null)
            {
                float size = _gameObject.Scale.Y;

                if (_gameObject.Scale.X > size)
                    size = _gameObject.Scale.X;

                _body = BodyFactory.CreateCircle(PhysicsSettings.World, _radius * size, 1, _gameObject.Position + Offset, 0, BodyType.Static);
            }
        }

        public override void PhysicsUpdate(GameCore game)
        {
            if (_body != null)
            {
                _body.Position = _gameObject.Position / PhysicsSettings.UnitToPixelRatio;
                _body.Rotation = _gameObject.Rotation;
            }
        }

    }
}
