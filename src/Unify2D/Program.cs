namespace Unify2D
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using (var game = new GameEditor()) game.Run();
        }
    }
}