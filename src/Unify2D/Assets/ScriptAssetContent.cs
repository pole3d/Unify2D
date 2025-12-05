using ImGuiNET;
using System;
using System.IO;
using System.Text;
using Unify2D.Core;
using Unify2D.Toolbox;

namespace Unify2D.Assets
{
    internal class ScriptAssetContent : AssetContent
    {
        public string Content = String.Empty;
        public string Path => CoreTools.CombinePath(GameEditor.Instance.AssetsPath, _asset.FullPath);
        public ScriptAssetContent(Asset asset) : base(asset) { }

        public override void Load()
        {
            base.Load();

            try
            {
                using (FileStream stream = new FileStream(Path, FileMode.Open, FileAccess.Read))
                {
                    using var sr = new StreamReader(stream, Encoding.UTF8);

                    Content = sr.ReadToEnd();
                }

            }
            catch (Exception)
            {

            }


        }
        
        public override void OnDragDroppedInGame(GameEditor editor)
        {
            GameObject go = new GameObject() { Name = _asset.Name };
            // GameCore.Current.AddGameObjectImmediate(go);
            //TODO : Add component to the gameObject
            Selection.SelectObject(go);
        }
        internal void Save()
        {
            string path = CoreTools.CombinePath(GameEditor.Instance.AssetsPath, _asset.FullPath);
            File.WriteAllText(path, Content);
        }

        public override void Show(InspectorToolbox inspectorToolbox)
        {
            ImGui.InputTextMultiline("##source", ref Content, ushort.MaxValue,
                new System.Numerics.Vector2(340, 550));
            if (ImGui.Button("Save"))
            {
                Save();
                inspectorToolbox.Editor.Scripting.Reload();
            }
        }
    }
}
