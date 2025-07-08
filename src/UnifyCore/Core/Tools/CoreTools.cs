namespace Unify2D
{

    /// <summary>
    /// Tools used in both editor and game
    /// </summary>
    public class CoreTools
    {
        public static string CombinePath(string a, string b)
        {
            if (string.IsNullOrEmpty(a))
            {
                if (string.IsNullOrEmpty(b))
                    return string.Empty;

                return b;
            }
            else if (string.IsNullOrEmpty(b))
            {
                return a;
            }

            a = a.TrimEnd('/', '\\');
            b = b.TrimStart('/', '\\');

            if ( b.StartsWith("./"))
                b = b.Substring(2,b.Length-2);

            return a + "\\" + b;

        }
    }
}
