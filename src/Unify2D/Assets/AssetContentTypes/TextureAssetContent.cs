using Unify2D.Core;
using Unify2D.Core.Graphics;

namespace Unify2D.Assets
{
    internal class TextureAssetContent : AssetContent
    {
        public TextureAssetContent(Asset asset) : base(asset) { }
        
        public override void OnDragDroppedInGame(GameEditor editor)
        {
            GameObject go = new GameObject() { Name = _asset.Name };
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.Initialize(editor, go, _asset.FullPath);
            GameCore.Current.AddGameObject(go);
            editor.SelectObject(go);
        }
    }
}