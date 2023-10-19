
using Genbox.VelcroPhysics.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unify2D.Physics
{
    public class PhysicsSettings
    {
        public static int SolverIterations;
        public static Vector2 Gravity;
        public static float UnitToPixelRatio = 100;
        public static World World;

        public static void Init()
        {
            SolverIterations = 6;
            Gravity = new Vector2(0, 9.81f);

            World = new World(Gravity);
  
            World.Enabled = true;
        }
    }
}
