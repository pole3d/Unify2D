using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unify2D.Toolbox
{
    abstract class Toolbox
    {
        protected GameEditor _editor;

        public abstract void Show();

        public virtual void Initialize(GameEditor editor)
        {
            _editor = editor;
        }

        internal virtual void Reset()
        {
        }
    }
}
