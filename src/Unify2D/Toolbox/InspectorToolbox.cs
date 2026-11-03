using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unify2D.Assets;
using Unify2D.Core;
using Unify2D.Core.Graphics;

namespace Unify2D.Toolbox
{
    /// <summary>
    /// The <see cref="InspectorToolbox"/> class,
    /// is a specialized toolbox designed to provide a user interface for inspecting details of a
    /// specific <see cref="GameAsset"> or <see cref="GameObject"> within the editor environment.
    /// </summary>
    public class InspectorToolbox : Toolbox
    {
        internal PrefabAssetContent CurrentPrefabAsset { get; set; }
        public GameEditor Editor => _editor;
        public GameObject GameObject { get; internal set; }


        private Asset _asset;

        private List<TextureBound> _texturesBound = new List<TextureBound>();
        private List<TextureBound> _texturesToUnbind = new List<TextureBound>();

        private Dictionary<Type, PropertyViewer> _propertyViewers = new Dictionary<Type, PropertyViewer>();


        private const string SavePrefabButtonLabel = "Save Prefab";
        private const string ApplyPrefabButtonLabel = "Apply to Prefab";

        private bool _isLocked = false;
        private Texture2D _lockTexture;
        private Texture2D _unlockTexture;
        private IntPtr _lockTexturePtr = IntPtr.Zero;
        private IntPtr _unlockTexturePtr = IntPtr.Zero;

        public override void Initialize(GameEditor editor)
        {
            _editor = editor;

            _propertyViewers.Add(typeof(Color), new ColorPropertyViewer());
            _propertyViewers.Add(typeof(int), new IntPropertyViewer());
            _propertyViewers.Add(typeof(float), new FloatPropertyViewer());
            _propertyViewers.Add(typeof(string), new StringPropertyViewer());
            _propertyViewers.Add(typeof(Vector2), new Vector2PropertyViewer());
            _propertyViewers.Add(typeof(GameAsset), new GameAssetPropertyViewer());
            _propertyViewers.Add(typeof(Enum), new EnumPropertyViewer());

            //_propertyViewers.Add(typeof(SpriteFont), new SpriteFontPropertyViewer());
            _propertyViewers.Add(typeof(Texture2D), new Texture2DPropertyViewer());
        }

        public void SetObject(object obj)
        {
            if (_isLocked) return;
            SetObjectInternal(obj);
        }

        private void SetObjectInternal(object obj)
        {
            UnSelect();

            _asset = null;
            GameObject = null;
            CurrentPrefabAsset = null;

            if (obj is GameObject)
                GameObject = obj as GameObject;
            else if (obj is Asset)
                _asset = obj as Asset;
        }

        private void UnSelect()
        {
            foreach (var item in _texturesBound)
            {
                _texturesToUnbind.Add(item);
            }

            _texturesBound.Clear();
        }

        internal void ReleaseTextureBindings()
        {
            UnSelect();
            foreach (var item in _texturesToUnbind)
                _editor.GuiRenderer.UnbindTexture(item.IntPtr);
            _texturesToUnbind.Clear();
        }

        public override void Draw()
        {
            foreach (var item in _texturesToUnbind)
            {
                GameEditor.Instance.GuiRenderer.UnbindTexture(item.IntPtr);
            }

            _texturesToUnbind.Clear();

            ImGui.Begin("Inspector");

            if (GameObject != null)
            {
                ShowGameObject();
            }
            else if (_asset != null)
            {
                ShowAsset();
            }

            ImGui.End();
        }

        private void ShowAsset()
        {
            if (_asset.AssetContent == null) return;

            // Load the prefab asset content if not already loaded
            if (_asset.AssetContent.IsLoaded == false)
                _asset.AssetContent.Load();

            _asset.AssetContent.Show(this);

            _asset.AssetContent.Unload();
        }

        public void ShowGameObject()
        {
            ImGui.BeginGroup();


            /// Add button lock/unlock inspector
            
            CheckLockTextureButton();
            IntPtr lockIcon = _isLocked ? _lockTexturePtr : _unlockTexturePtr;
            if (lockIcon != IntPtr.Zero)
            {
                if (ImGui.ImageButton(lockIcon, new System.Numerics.Vector2(16, 16)))
                {
                    _isLocked = !_isLocked;
                }
            }

            if (CurrentPrefabAsset != null)
            {
                // Add a button to save the prefab
                if (ImGui.Button(SavePrefabButtonLabel))
                {
                    CurrentPrefabAsset.SavePrefab(GameObject);

                    Debug.Log($"Prefab {GameObject.Name} saved!");
                    Console.WriteLine($"Prefab {GameObject.Name} saved!");
                }

                ImGui.Separator();
            }
            else if (GameObject.Tag is PrefabAssetContent)
            {
                if (ImGui.Button(ApplyPrefabButtonLabel))
                {
                    SavePrefabFromHierarchy(GameObject);
                }
            }

            string name = GameObject.Name;

            if (ImGui.InputText("name", ref name, 40)) GameObject.Name = name;
            System.Numerics.Vector2 position =
                new System.Numerics.Vector2(GameObject.LocalPosition.X, GameObject.LocalPosition.Y);
            float rotation = MathHelper.ToDegrees(GameObject.LocalRotation);
            System.Numerics.Vector2 scale = new System.Numerics.Vector2(GameObject.Scale.X, GameObject.Scale.Y);
            if (ImGui.InputFloat2("position", ref position))
                GameObject.LocalPosition = new Vector2(position.X, position.Y);
            if (ImGui.InputFloat("rotation", ref rotation))
                GameObject.LocalRotation = MathHelper.ToRadians(rotation);
            if (ImGui.InputFloat2("scale", ref scale))
                GameObject.Scale = new Vector2(scale.X, scale.Y);

            List<Component> toRemove = new List<Component>();
            foreach (var component in GameObject.Components)
            {
                ImGui.SetNextItemOpen(true, ImGuiCond.Once);
                if (ImGui.TreeNode(component.GetType().Name))
                {
                    ShowComponent(component);
                    ImGui.PushStyleColor(ImGuiCol.Button, ToolsUI.ToColor32(230, 50, 60, 255));
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ToolsUI.ToColor32(250, 70, 80, 255));
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, ToolsUI.ToColor32(255, 90, 100, 255));

                    if (ImGui.Button("Delete"))
                    {
                        toRemove.Add(component);
                    }

                    ImGui.PopStyleColor(3);

                    ImGui.SameLine();

                    if (component is SpriteRenderer)
                    {
                        if (ImGui.Button("Sprite Editor"))
                        {
                            PropertyInfo[] properties = component.GetType().GetProperties();

                            foreach (PropertyInfo property in properties)
                            {
                                try
                                {
                                    _propertyViewers[property.PropertyType].Draw(property, component);
                                }
                                catch
                                {
                                }
                            }

                            GameEditor.Instance.SpriteEditorToolbox.Open(component);

                            Debug.Log("Sprite Editor is open !");
                        }
                    }


                    ImGui.TreePop();
                }

                ImGui.Separator();
            }

            foreach (var item in toRemove)
            {
                GameObject.RemoveComponent(item);
            }

            if (GameObject.Components.Count() == 0)
                ImGui.Separator();

            if (ImGui.CollapsingHeader("Add Component"))
            {
                if (ImGui.Button("Refresh Scripts"))
                    _editor.Scripting.Reload();
                foreach (var item in _editor.Scripting.GetTypes().OrderBy(type => type.Name))
                {
                    if (ImGui.Button($"{item.Name}##{item.FullName}"))
                    {
                        _editor.Scripting.TryAddComponent(GameObject, item);
                    }
                }
            }
            // Extend the group over the empty space so the whole inspector content accepts scripts.
            var remainingSpace = ImGui.GetContentRegionAvail();
            ImGui.Dummy(new System.Numerics.Vector2(Math.Max(0, remainingSpace.X), Math.Max(0, remainingSpace.Y)));
            ImGui.EndGroup();
            GameToolbox.TryDropScript(_editor, GameObject);
        }

        private void ShowComponent(Component component)
        {
            PropertyInfo[] properties = component.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                //if (Attribute.IsDefined(property, typeof(JsonIgnoreAttribute)))
                //{
                //    continue;
                //}

                try
                {
                    if (_propertyViewers.TryGetValue(property.PropertyType, out PropertyViewer viewer))
                    {
                        viewer.Draw(property, component);
                    }
                    else if (property.PropertyType.BaseType != null &&
                             _propertyViewers.TryGetValue(property.PropertyType.BaseType,
                                 out PropertyViewer baseClassViewer))
                    {
                        baseClassViewer.Draw(property, component);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                }
            }
        }

        public void AddTextureBound(TextureBound textureBound)
        {
            _texturesBound.Add(textureBound);
        }

        /// <summary>
        /// TODO : move bound textures in a dedicated class
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public TextureBound GetTextureBound(Texture2D texture)
        {
            foreach (var item in _texturesBound)
            {
                if (item.Texture == texture)
                    return item;
            }

            return null;
        }

        private void SavePrefabFromHierarchy(GameObject gameObject)
        {
            if (gameObject.Tag is PrefabAssetContent prefabAssetContent)
            {
                if (prefabAssetContent != null)
                {
                    prefabAssetContent.SavePrefab(gameObject);
                }

                Debug.Log($"Prefab {GameObject.Name} saved from hierarchy!");
                Console.WriteLine($"Prefab {gameObject.Name} saved from hierarchy!");
            }
        }

        public string GetImagePath(string filename)
        {
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", filename);
            if (File.Exists(basePath)) return basePath;

            string secondaryPath = Path.Combine(Directory.GetCurrentDirectory(), "Images", filename);
            if (File.Exists(secondaryPath)) return secondaryPath;

            string lastPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "Debug", "Unify2D", "Images", filename);
            if (File.Exists(lastPath)) return lastPath;

            return basePath;
        }

        private void CheckLockTextureButton()
        {
            if (_lockTexturePtr != IntPtr.Zero || _editor?.GuiRenderer == null) return;

            var gd = _editor.GraphicsDevice ?? GameCore.Current?.GraphicsDevice;

            if (gd == null) return;

            try
            {
                string lockPath = GetImagePath("LockInspector.png");
                if (File.Exists(lockPath))
                {
                    using var lockImage = File.OpenRead(lockPath);
                    _lockTexture = Texture2D.FromStream(gd, lockImage);
                    _lockTexturePtr = _editor.GuiRenderer.BindTexture(_lockTexture);
                }

                string unlockPath = GetImagePath("UnlockInspector.png");
                if (File.Exists(unlockPath))
                {
                    using var unlockImage = File.OpenRead(unlockPath);
                    _unlockTexture = Texture2D.FromStream(gd, unlockImage);
                    _unlockTexturePtr = _editor.GuiRenderer.BindTexture(_unlockTexture);
                }

                if (_unlockTexturePtr == IntPtr.Zero && _lockTexturePtr != IntPtr.Zero) _unlockTexturePtr = _lockTexturePtr;
                if (_lockTexturePtr == IntPtr.Zero && _unlockTexturePtr != IntPtr.Zero) _lockTexturePtr = _unlockTexturePtr;
            }
            catch (Exception ex)
            {
                Debug.Log($"ERROR texture (lock/unlock button)");
            }
        }
    }

    public class TextureBound
    {
        public Texture2D Texture { get; set; }
        public IntPtr IntPtr { get; set; }
    }
}
