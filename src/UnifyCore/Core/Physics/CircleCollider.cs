using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Core;
using Unify2D.Physics;

namespace UnifyCore.Core.Physics
{
    internal class CircleCollider: Component
    {
        public float Radius{ get { return m_radius; } set { m_radius = value; } }

        private Vector2 m_size;

        private Body staticBody;

        private float m_radius = 1f;

        public override void Load(Game game, GameObject go)
        {
            Rigidbody rb = _gameObject.GetComponent<Rigidbody>();

            if (rb == null)
            {
                float size = _gameObject.Scale.Y;

                if (_gameObject.Scale.X > size)
                    size = _gameObject.Scale.X;

                staticBody = BodyFactory.CreateCircle(PhysicsSettings.World, m_radius * size, 1, _gameObject.Position, 0, BodyType.Static);
            }
        }

        public override void Update(GameCore game)
        {

        }
        /*

        public override void DrawGizmoOnSelected(ImDrawListPtr drawList)
        {
            drawList.AddRect(new System.Numerics.Vector2(m_width, m_height), new System.Numerics.Vector2(m_width, m_height), ToColor32(Color.Green.R, Color.Green.G, Color.Green.B, Color.Green.A));
        }

        //REMOVE THIS SHIT
        static uint ToColor32(byte r, byte g, byte b, byte a) { uint ret = a; ret <<= 8; ret += b; ret <<= 8; ret += g; ret <<= 8; ret += r; return ret; }
        */
    }
}
