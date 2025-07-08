using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Unify2D.Core;
using UnifyCore.Core.Physics;

namespace Unify2D.Physics
{
    internal class Rigidbody : Component
    {
        [JsonIgnore]
        public BodyType Type { get { return _type; } set { _type = value; } }
        public float Mass { get { return _mass; } set { _mass = value; } }
        public float LinearDamper { get { return _linearDamper; } set { _linearDamper = value; } }
        public float GravityScale { get { return _gravityScale; } set { _gravityScale = value; } }
        public bool IsKinematic
        {
            get
            {
                return (_type == BodyType.Kinematic);
            }

            set
            {
                if (value)
                {
                    _type = BodyType.Kinematic;
                }
                else
                {
                    _type = BodyType.Dynamic;
                }
            }
        }

        private float _mass = 1f;
        private float _linearDamper = 0.1f;
        private BodyType _type = BodyType.Dynamic;
        private float _gravityScale = 1f;
        private Body _body;

        public override void Load(Game game, GameObject go)
        {
            Collider col = _gameObject.GetComponent<Collider>();
            if (col != null)
            {
                _body = col.Init();
                _body.Mass = _mass;
                _body.LinearDamping = _linearDamper;
                _body.GravityScale = _gravityScale;
                _body.AngularDamping = 1;
                _body.BodyType = _type;
            }
            else
            {
                _body = BodyFactory.CreateBody(PhysicsSettings.World, _gameObject.Position / PhysicsSettings.UnitToPixelRatio, _gameObject.Rotation, _type);
                _body.Mass = _mass;
            }
        }

        public override void PhysicsUpdate(GameCore game)
        {
            if (_body.BodyType == BodyType.Dynamic)
            {
                /*
                if (_gameObject.PositionUpdated)
                {
                    _body.Position = _gameObject.Position / PhysicsSettings.UnitToPixelRatio;
                    _body.GravityScale = 0;
                }
                else
                {
                    _gameObject.Position = _body.Position * PhysicsSettings.UnitToPixelRatio;
                    _body.GravityScale = _gravityScale;
                }

                if (_gameObject.RotationUpdated)
                    _body.Rotation = _gameObject.Rotation;
                else
                    _gameObject.Rotation = _body.Rotation;
                */
                _gameObject.Position = _body.Position * PhysicsSettings.UnitToPixelRatio;
                _gameObject.Rotation = _body.Rotation;
            }
            else
            {
                _body.Position = _gameObject.Position / PhysicsSettings.UnitToPixelRatio;
                _body.Rotation = _gameObject.Rotation;
            }
        }

        public void AddForce(Vector2 force)
        {
            _body.ApplyForce(force);
        }

        public void AddForceAtPosition(Vector2 force, Vector2 position)
        {
            _body.ApplyLinearImpulse(force, position);
        }

    }
}
