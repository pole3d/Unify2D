using ChipmunkSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Core;

namespace Unify2D.Physics
{
    internal class Rigidbody : Component 
    {
        public cpBodyType Type { get { return m_type; } set { m_type = value; } }
        public float Mass { get { return m_mass; } set { m_mass = value; } }
        public float Damper { get { return m_damper; } set { m_damper = value; } }
        public bool UseGravity { get { return m_useGravity; } set { m_useGravity = value; } }
        public bool IsKinematic { get { return m_isKinematic; } set { m_isKinematic = value; } }
        public float Velocity { get { return m_velocity; } }

        private cpBodyType m_type;
        private float m_mass;
        private float m_damper;

        private bool m_useGravity;
        private bool m_isKinematic;

        private float m_velocity;
        private cpBody m_cpBody;

        public override void Load(Game game, GameObject go)
        {
            m_cpBody = new cpBody(m_mass, m_velocity);

            m_cpBody.SetBodyType(m_type);
            m_cpBody.SetMass(m_mass);
            m_cpBody.SetPosition(ChipmunkConverter.Vector2ToCpVect(_gameObject.Position));
        }

        public override void Update(GameCore game)
        {
            float deltaTime = (float)game.GameTime.ElapsedGameTime.TotalSeconds;

            Vector2 gravity = Vector2.Zero;

            if (m_useGravity)
                gravity = PhysicsSettings.Gravity;

            cpVect cpGravity = ChipmunkConverter.Vector2ToCpVect(gravity);

            m_cpBody.UpdateVelocity(cpGravity, m_damper, deltaTime);
            //m_cpBody.UpdatePosition(deltaTime);


            _gameObject.Position = ChipmunkConverter.CpVectToVector2(m_cpBody.GetPosition());
        }

        public void AddForce(Vector2 force)
        {
            cpVect cpForce = ChipmunkConverter.Vector2ToCpVect(force);
            cpVect cpPos = new cpVect(_gameObject.Position.X, _gameObject.Position.Y);

            m_cpBody.ApplyForce(cpForce, cpPos);
        }

        public void AddForceAtPosition(Vector2 force, Vector2 position)
        {
            cpVect cpForce = ChipmunkConverter.Vector2ToCpVect(force);
            cpVect cpPos = ChipmunkConverter.Vector2ToCpVect(position);

            m_cpBody.ApplyForce(cpForce, cpPos);
        }
    }
}
