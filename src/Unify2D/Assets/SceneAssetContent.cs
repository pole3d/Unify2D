using ImGuiNET;
using Unify2D.Toolbox;

namespace Unify2D.Assets
{
    internal class SceneAssetContent : AssetContent
    {
        public SceneAssetContent() : base(null)
        {
        }

        public SceneAssetContent(Asset asset) : base(asset)
        {
        }

        public override void Show(InspectorToolbox inspectorToolbox)
        {
            ImGui.Text("Scene : " + _asset.Name);
        }
    }
}