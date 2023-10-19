using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Core;
using Unify2D.Physics;

namespace UnifyCore.Core.Physics
{
    internal class BoxCollider : Component
    {
        public float Width { get { return m_width ; } set { m_width = value; } }
        public float Height { get { return m_height ; } set { m_height = value; } }

        private Vector2 m_size;

        private Body staticBody;

        private float m_width = 2f, m_height = 2f;

        public override void Load(Game game, GameObject go)
        {
            Rigidbody rb = _gameObject.GetComponent<Rigidbody>();

            if (rb == null)
            {
                staticBody = BodyFactory.CreateRectangle(PhysicsSettings.World, m_width, m_height, 1, _gameObject.Position / PhysicsSettings.UnitToPixelRatio, 0, BodyType.Static);
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
