using Genbox.VelcroPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Unify2D.Physics
{
    public class PhysicsSettings
    {
        private static readonly PhysicsSettings Fallback = new PhysicsSettings();
        private static PhysicsSettings Current => Unify2D.Core.GameCore.Current?.PhysicsSettings ?? Fallback;
        private int _solverIterations;
        private Vector2 _gravity;
        private float _unitToPixelRatio = 100;
        private World _world;

        public static int SolverIterations { get => Current._solverIterations; set => Current._solverIterations = value; }
        public static Vector2 Gravity { get => Current._gravity; set => Current._gravity = value; }
        public static float UnitToPixelRatio { get => Current._unitToPixelRatio; set => Current._unitToPixelRatio = value; }
        public static World World { get => Current._world; set => Current._world = value; }

        public static void Init()
        {
            SolverIterations = 6;
            Gravity = new Vector2(0, 9.81f);

            World = new World(Gravity);
  
            World.Enabled = true;
        }
    }
}
