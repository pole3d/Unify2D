using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unify2D.Tools
{
    /// <summary>
    /// The <see cref="ToolsEditor"/> class is a base class that represents
    /// an In-Editor Tool. This class serves as a foundation
    /// for creating specialized <see cref="Tools"/> with shared functionality.
    /// </summary>
    internal class ToolsEditor
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
