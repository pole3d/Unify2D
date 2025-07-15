
namespace Unify2D
{
    public class SceneInfo
    {
        public SceneInfo(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public int BuildIndex { get; set; }

    }
}
