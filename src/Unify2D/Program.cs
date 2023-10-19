

namespace Unify2D
{
    /// <summary>
    /// Entry point of the Game Editor
    /// </summary>
    public static class Program
    {
        public static void Main(string[] args)
        {
            using (var game = new GameEditor()) game.Run();
        }
    }
}