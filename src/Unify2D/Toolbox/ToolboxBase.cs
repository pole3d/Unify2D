using ImGuiNET;
using Microsoft.Xna.Framework;
using Unify2D.Assets;

namespace Unify2D.Toolbox
{
    /// <summary>
    /// The <see cref="Toolbox"/> class, as an abstract base class, serves as the foundation
    /// for creating in-editor windows using <see cref="ImGui"/>. Toolboxes are specialized interfaces within
    /// the editor environment. It's meant to be inherited to create more specific toolboxes, 
    /// implementing the necessary functionalities.
    /// </summary>
    public abstract class ToolboxBase
    {
        public object Tag { get => _tag; }

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

        public virtual void Update(GameTime gameTime)
        {
        }
        
        public virtual void SetCore(GameCoreViewer coreViewer)
        {
            _tag = coreViewer;
        }
    }
}
