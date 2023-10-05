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
        public object Tag { get => _tag; set => _tag = value; }

        protected GameEditor _editor;
        protected object _tag;

        public abstract void Draw();

        public virtual void Initialize(GameEditor editor)
        {
            _editor = editor;
        }

        internal virtual void Reset()
        {
        }
    }
}
