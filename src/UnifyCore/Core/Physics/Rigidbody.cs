using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Core;



namespace Unify2D.Physics
{


    internal class Rigidbody : Component 
    {
        public BodyType Type { get{ return m_type; } set { m_type = value; } }
        public float Mass { get { return m_mass; } set { m_mass = value; } }
        public float LinearDamper { get { return m_linearDamper; } set { m_linearDamper = value; } }
        public float GravityScale{ get { return m_gravityScale; } set { m_gravityScale = value; } }

        public bool IsKinematic { 
            get 
            { 
                return (m_type == BodyType.Kinematic);
            }  

            set
            {
                if (value) 
                {
                    m_type = BodyType.Kinematic;
                }
                else
                {
                    m_type = BodyType.Dynamic;
                }
            }
        }

        private float m_mass = 1f;
        private float m_linearDamper = 0.1f;
        private BodyType m_type = BodyType.Dynamic;
        
        private float  m_gravityScale = 1;

        private Body m_velcroBody;

        public override void Load(Game game, GameObject go)
        {
            m_velcroBody = BodyFactory.CreateBody(PhysicsSettings.World, _gameObject.Position, 0, m_type);
            m_velcroBody.Mass = m_mass;
            m_velcroBody.LinearDamping = m_linearDamper;
            m_velcroBody.Restitution = 1;
            m_velcroBody.GravityScale = m_gravityScale;
        }

        public override void Update(GameCore game)
        {
            _gameObject.Position = m_velcroBody.Position;
        }

        public void AddForce(Vector2 force)
        {
            m_velcroBody.ApplyForce(force);
        }

        public void AddForceAtPosition(Vector2 force, Vector2 position)
        {
            m_velcroBody.ApplyLinearImpulse(force, position);
        }
    }
}
