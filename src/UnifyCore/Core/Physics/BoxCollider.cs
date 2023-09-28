using Genbox.VelcroPhysics.Factories;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Core;
using Unify2D.Physics;

namespace UnifyCore.Core.Physics
{
    internal class BoxCollider : Component
    {
        public int IT_DOES_NOT_WORK_YET_DONT_USE_OR_DIE { get; set; }
        public Vector2 Size { get { return m_size; } set { m_size = value; } }
        private Vector2 m_size;

        public override void Load(Game game, GameObject go)
        {
            //Check for existence of rigidbody, if it exists, override the spawn to a boxbody with the relative size. Find a way to make sure Load is happening before rigidbody.Load()

            //If no rigidbody, BECOME THE RIGIDBODY
        }

        public override void Update(GameCore game)
        {
            //Nothing here if there is a rigidbody, else, mirror the rigidbody update
        }
    }
}
