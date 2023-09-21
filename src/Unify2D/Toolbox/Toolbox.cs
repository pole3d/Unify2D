using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unify2D.Toolbox
{
    /// <summary>
    /// Base class for a tool window in the game editor
    /// </summary>
    abstract class Toolbox
    {
        public abstract void Show();

        public abstract void Initialize(GameEditor editor);

        internal virtual void Reset()
        {
        }
    }
}
