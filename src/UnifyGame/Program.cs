using System.IO;
using Unify2D.Core;
using static System.Net.Mime.MediaTypeNames;

namespace UnifyGame
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using (var game = new UnifyGame()) game.Run();
        }
    }
}