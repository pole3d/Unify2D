using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Microsoft.Xna.Framework;
using Unify2D.Core;
using Unify2D.Physics;

namespace UnifyCore.Core.Physics
{
    internal class CapsuleCollider : Component
    {
        public float Height { get{ return m_height; } set { m_height = value; } }
        public float Radius { get { return m_radius; } set { m_radius = value; } }

        private Vector2 m_size;
        private Body staticBody;

        private float m_radius = 0.5f, m_height = 1f;

        public override void Load(Game game, GameObject go)
        {
            Rigidbody rb = _gameObject.GetComponent<Rigidbody>();

            if (rb == null)
            {
                staticBody = BodyFactory.CreateCapsule(PhysicsSettings.World, m_height * _gameObject.Scale.Y, m_radius * _gameObject.Scale.X, 1, _gameObject.Position, _gameObject.Rotation, BodyType.Static);
            }
        }

        public override void Update(GameCore game)
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
