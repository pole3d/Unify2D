using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unify2D.Tools
{
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

            return a + "/" + b;

        }
    }
}
