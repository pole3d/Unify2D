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
        public float Radius{ get { return m_radius; } set { m_radius = value; } }
        public Vector2 Offset { get { return m_offset; } set { m_offset = value; } }

        private Vector2 m_size, m_offset;

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

                staticBody = BodyFactory.CreateCircle(PhysicsSettings.World, m_radius * size, 1, _gameObject.Position + Offset, 0, BodyType.Static);
            }
        }

        public override void PhysicsUpdate(GameCore game)
        {
            if (staticBody != null)
            {
                staticBody.Position = _gameObject.Position / PhysicsSettings.UnitToPixelRatio;
                staticBody.Rotation = _gameObject.Rotation;
            }
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
