using ImGuiNET;
using Unify2D.Toolbox;

namespace Unify2D.Assets
{
    public abstract class AssetContent
    {
        public bool IsLoaded { get; set; }
        public Asset Asset => _asset;
        protected Asset _asset;

        public object RawAsset;

        public AssetContent(Asset asset)
        {
            _asset = asset;
        }

        public virtual void Load()
        {
            IsLoaded = true;
        }

        public virtual void Unload()
        {
            IsLoaded = false;
        }

        public virtual void Show(InspectorToolbox inspectorToolbox)
        {
            inspectorToolbox.CurrentPrefabAsset = null;
        }

        public virtual void OnDragDroppedInGame(GameEditor editor) { }
    }
}
