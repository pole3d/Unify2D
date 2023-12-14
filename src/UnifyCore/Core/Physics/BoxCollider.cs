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
using Unify2D.Core.Graphics;
using Unify2D.Core.Tools;
using Unify2D.Physics;

namespace UnifyCore.Core.Physics
{
    internal class BoxCollider : Component
    {
        public float Width { get { return m_width ; } set { m_width = value; } }
        public float Height { get { return m_height ; } set { m_height = value; } }
        public Vector2 Offset { get { return m_offset; } set { m_offset = value; } }

        private Body staticBody;

        private float m_width = 1f, m_height = 1f;

        private Vector2 m_offset;

        public override void Load(Game game, GameObject go)
        {
            Rigidbody rb = _gameObject.GetComponent<Rigidbody>();

            if (rb == null)
            {
                staticBody = BodyFactory.CreateRectangle(PhysicsSettings.World, m_width * _gameObject.Scale.X, m_height * _gameObject.Scale.Y, 1, (_gameObject.Position + m_offset) / PhysicsSettings.UnitToPixelRatio, _gameObject.Rotation, BodyType.Static);
            }
        }

        public override void Update(GameCore game)
        {

        }

        internal override void DrawGizmo()
        {
            Vector2 offsettedPosition = _gameObject.Position + m_offset;

            int pixelsWidth  = (int)Math.Round(m_width * _gameObject.Scale.X * PhysicsSettings.UnitToPixelRatio);
            int pixelsHeight = (int)Math.Round(m_height * _gameObject.Scale.Y * PhysicsSettings.UnitToPixelRatio);

            /*
            Gizmo.DrawSquare(_gameObject.Position, new Vector2(_gameObject.Position.X - pixelsWidth / 2, _gameObject.Position.Y - pixelsHeight / 2),
                new Vector2(_gameObject.Position.X + pixelsWidth / 2, _gameObject.Position.Y + pixelsHeight / 2), _gameObject.Rotation) ;
            */
            Gizmo.DrawWireSquare(new Vector2(offsettedPosition.X - pixelsWidth / 2, offsettedPosition.Y - pixelsHeight / 2), 
                new Vector2(offsettedPosition.X + pixelsWidth / 2, offsettedPosition.Y + pixelsHeight / 2), 3, Color.LightGreen);
            
        }

    }
}
