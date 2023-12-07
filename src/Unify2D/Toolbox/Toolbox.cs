namespace Unify2D.Toolbox
{
    /// <summary>
    /// Base class for a tool window in the game editor
    /// </summary>
    public abstract class Toolbox
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

        public virtual void Update()
        {
        }
    }
}
