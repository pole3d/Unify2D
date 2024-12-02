using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
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
        private GameObject _gameObject;
        private Asset _asset;

        private List<TextureBound> _texturesBound = new List<TextureBound>();
        private List<TextureBound> _texturesToUnbind = new List<TextureBound>();

        private Dictionary<Type, PropertyViewer> _propertyViewers = new Dictionary<Type, PropertyViewer>();

        /// <summary>
        /// WORKAROUND : Add one frame delay to avoid modifying another gameobject when
        /// switching between gameobjects
        /// </summary>
        private int _changeCount = 0;

        private PrefabAssetContent _currentPrefabAsset;
        
        private const string SavePrefabButtonLabel = "Save Prefab";
        private const string ApplyPrefabButtonLabel = "Apply to Prefab";

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

            _propertyViewers.Add(typeof(SpriteFont), new SpriteFontPropertyViewer());
            _propertyViewers.Add(typeof(Texture2D), new Texture2DPropertyViewer());
        }

        public void SetObject(object obj)
        {
            UnSelect();

            _asset = null;
            _gameObject = null;
            _currentPrefabAsset = null;

            if (obj is GameObject)
                _gameObject = obj as GameObject;
            else if (obj is Asset)
                _asset = obj as Asset;
        }

        private void UnSelect()
        {
            _changeCount = 1;

            foreach (var item in _texturesBound)
            {
                _texturesToUnbind.Add(item);
            }

            _texturesBound.Clear();
        }

        public override void Draw()
        {
            foreach (var item in _texturesToUnbind)
            {
                GameEditor.Instance.GuiRenderer.UnbindTexture(item.IntPtr);
            }

            _texturesToUnbind.Clear();

            ImGui.Begin("Inspector");

            if (_changeCount <= 0)
            {
                if (_gameObject != null)
                {
                    ShowGameObject();
                }
                else if (_asset != null)
                {
                    ShowAsset();
                }
            }
            else
                _changeCount--;

            ImGui.End();
        }

        private void ShowAsset()
        {
            if (_asset.AssetContent is ScriptAssetContent scriptAsset)
            {
                if (scriptAsset.IsLoaded == false)
                    scriptAsset.Load();

                ImGui.InputTextMultiline("##source", ref scriptAsset.Content, ushort.MaxValue,
                    new System.Numerics.Vector2(340, 550));
                if (ImGui.Button("Save"))
                {
                    scriptAsset.Save();
                    _editor.Scripting.Reload();
                }
            }

            if (_asset.AssetContent is PrefabAssetContent prefabAsset)
            {
                // Load the prefab asset content if not already loaded
                if (!prefabAsset.IsLoaded)
                    prefabAsset.Load();

                // Set _gameObject to the instantiated prefab game object to show its properties
                _gameObject = prefabAsset.InstantiatedGameObject;

                ShowGameObject();

                _currentPrefabAsset = prefabAsset;
            }
            else
            {
                _currentPrefabAsset = null;
            }
        }

        private void ShowGameObject()
        {
            if (_currentPrefabAsset != null)
            {
                // Add a button to save the prefab
                if (ImGui.Button(SavePrefabButtonLabel))
                {
                    _currentPrefabAsset.SavePrefab(_gameObject);
                    
                    Debug.Log($"Prefab {_gameObject.Name} saved!");
                    Console.WriteLine($"Prefab {_gameObject.Name} saved!");
                }

                ImGui.Separator();
            }
            else if (_gameObject.Tag is PrefabAssetContent)
            {
                if (ImGui.Button(ApplyPrefabButtonLabel))
                {
                    SavePrefabFromHierarchy(_gameObject);
                }
            }

            string name = _gameObject.Name;

            ImGui.InputText("name", ref name, 40);
            _gameObject.Name = name;
            System.Numerics.Vector2 position =
                new System.Numerics.Vector2(_gameObject.Position.X, _gameObject.Position.Y);
            float rotation = MathHelper.ToDegrees(_gameObject.Rotation);
            System.Numerics.Vector2 scale = new System.Numerics.Vector2(_gameObject.Scale.X, _gameObject.Scale.Y);
            ImGui.InputFloat2("position", ref position);
            ImGui.InputFloat("rotation", ref rotation);
            ImGui.InputFloat2("scale", ref scale);

            _gameObject.Position = new Vector2(position.X, position.Y);
            _gameObject.Rotation = MathHelper.ToRadians(rotation);
            _gameObject.Scale = new Vector2(scale.X, scale.Y);

            List<Component> toRemove = new List<Component>();
            foreach (var component in _gameObject.Components)
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
                _gameObject.RemoveComponent(item);
            }

            if (_gameObject.Components.Count() == 0)
                ImGui.Separator();

            if (ImGui.CollapsingHeader("Add Component"))
            {
                foreach (var item in _editor.Scripting.GetTypes())
                {
                    if (ImGui.Button(item.Name))
                    {
                        var component = Activator.CreateInstance(item);
                        _gameObject.AddComponent(component as Component);
                    }
                }
            }
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
                Console.WriteLine($"Prefab {gameObject.Name} saved from hierarchy!");
            }
        }
    }

    public class TextureBound
    {
        public Texture2D Texture { get; set; }
        public IntPtr IntPtr { get; set; }
    }
}