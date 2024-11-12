using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using Unify2D.Assets;
using Unify2D.Core;
using Unify2D.Tools;

namespace Unify2D.Toolbox
{
    /// <summary>
    /// The <see cref="AssetsToolbox"/> class,
    /// is a specialized toolbox designed to provide a user interface to visualise and select <see cref="Asset">s.
    /// </summary>
    internal class AssetsToolbox : Toolbox
    {
        string _path;
        bool[] _selected;
        List<Asset> _assets = new List<Asset>();
        HashSet<string> _extensionsToIgnore;

        public override void Initialize(GameEditor editor)
        {
            base.Initialize(editor);

            _extensionsToIgnore = new HashSet<string>{ ".csproj", ".dll" , ".sln" };

            Reset();
        }

        public Asset GetAssetFromPath(string path)
        {
            foreach (var asset in _assets)
            {
                if (path == asset.FullPath)
                    return asset;
            }
            return null;
        }

        internal override void Reset()
        {
            _editor.AssetManager.RefreshDatabase();
            _assets = _editor.AssetManager.Assets;
            _selected = new bool[_editor.AssetManager.NbOfFiles];
        }

        public override void Draw()
        {
            ImGui.Begin("Assets");

            if (ImGui.Button("Show Explorer", new System.Numerics.Vector2(-1, 0)))
            {
                ShowExplorer();
            }
            if (ImGui.Button("Create Script", new System.Numerics.Vector2(-1, 0)))
            {
                CreateScript();
            }

            for (int n = 0; n < _assets.Count; n++)
            {
                if (ImGui.Selectable(_assets[n].ToString(), _selected[n]))
                {
                    // Clear selection when CTRL is not held
                    if (!ImGui.GetIO().KeyCtrl)
                    {
                        for (int i = 0; i < _assets.Count; i++)
                        {
                            _selected[i] = false;
                        }
                    }

                    Selection.SelectObject(_assets[n]);
                    _selected[n] = !_selected[n];
                }
                
                if (ImGui.BeginPopupContextItem())
                {
                    // Avoid opening popup without selecting the item             
                    _selected[n] = true;

                    if (_assets[n].AssetContent is PrefabAssetContent prefabContent)
                    {
                        if (ImGui.Button("Open Prefab"))
                        {
                            //GameEditor.Instance.OpenPrefab(prefabContent);
                            ImGui.CloseCurrentPopup();
                        }

                        if (ImGui.Button("Instantiate as GameObject"))
                        {
                            // Logic to instantiate the prefab as a GameObject
                            PrefabInstance prefabInstance = new PrefabInstance($"{prefabContent.Asset.FullPath}");
                            GameObject instantiatedGameObject = prefabInstance.InstantiateAndLinkGameObject();
                            
                            // Add GameObject to the scene
                            SceneManager.Instance.CurrentScene.AddRootGameObject(instantiatedGameObject);

                            _selected[n] = false;
                            ImGui.CloseCurrentPopup();
                        }
                    }
                    if (ImGui.Button("Delete"))
                    {
                        DeleteSelectedAssets();
                    }
                    ImGui.EndPopup();
                }
            }
            
            ImGui.EndChild();
            
            ImGui.End();
        }

        private void CreateScript()
        {
            string newFile = "newScript.cs";

            using (StreamWriter sw = File.CreateText(Path.Combine(_path, newFile)))
            {
                string defaultScript = "using Unify2D.Core;\r\nusing Input = Microsoft.Xna.Framework.Input;\r\n\r\nnamespace Game\r\n{\r\n    class NewScript : Component\r\n    {\r\n        public override void Update(GameCore game)\r\n        {\r\n\r\n        }\r\n    }\r\n}";
                sw.WriteLine(defaultScript);
            }

            Reset();
        }

        private static void ShowExplorer()
        {
            string path = GameEditor.Instance.AssetsPath + Path.DirectorySeparatorChar;

            if (Directory.Exists(path) == false)
                Directory.CreateDirectory(path);

            System.Diagnostics.Process.Start("explorer.exe", path);
        }
        
        private void DeleteSelectedAssets()
        {
            for (int n = 0; n < _assets.Count; n++)
            {
                if (_selected[n])
                {
                    File.Delete(GameEditor.Instance.AssetsPath + _assets[n].FullPath);
                }
            }
            Reset();
        }
    }
}
