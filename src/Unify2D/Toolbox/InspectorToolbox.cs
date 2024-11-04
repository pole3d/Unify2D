using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unify2D.Assets;
using Unify2D.Core;

namespace Unify2D.Toolbox
{
    /// <summary>
    /// The <see cref="InspectorToolbox"/> class,
    /// is a specialized toolbox designed to provide a user interface for inspecting details of a
    /// specific <see cref="GameAsset"> or <see cref="GameObject"> within the editor environment.
    /// </summary>
    public class InspectorToolbox : Toolbox
    {
        GameObject _gameObject;
        Asset _asset;

        List<TextureBound> _texturesBound = new List<TextureBound>();
        List<TextureBound> _texturesToUnbind = new List<TextureBound>();

        Dictionary<Type,PropertyViewer> _propertyViewers = new Dictionary<Type,PropertyViewer>();

        /// <summary>
        /// WORKAROUND : Add one frame delay to avoid modifying another gameobject when
        /// switching between gameobjects
        /// </summary>
        int _changeCount = 0;

        public override void Initialize(GameEditor editor)
        {
            _editor = editor;

            _propertyViewers.Add(typeof(Color), new ColorPropertyViewer());
            _propertyViewers.Add(typeof(int), new IntPropertyViewer());
            _propertyViewers.Add(typeof(float), new FloatPropertyViewer());
            _propertyViewers.Add(typeof(string), new StringPropertyViewer());
            _propertyViewers.Add(typeof(Vector2), new Vector2PropertyViewer());
            _propertyViewers.Add(typeof(GameAsset), new GameAssetPropertyViewer());
            _propertyViewers.Add(typeof(SpriteFont), new SpriteFontPropertyViewer());
        }

        public void SetObject(object obj)
        {
            UnSelect();

            _asset = null;
            _gameObject = null;

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

                ImGui.InputTextMultiline("##source", ref scriptAsset.Content, ushort.MaxValue, new System.Numerics.Vector2(340, 550));
                if (ImGui.Button("Save"))
                {
                    scriptAsset.Save();
                    _editor.Scripting.Reload();
                }
            }
        }

        private void ShowGameObject()
        {
            string name = _gameObject.Name;

            ImGui.InputText("name", ref name, 40);
            _gameObject.Name = name;
            System.Numerics.Vector2 position = new System.Numerics.Vector2(_gameObject.Position.X, _gameObject.Position.Y);
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
                    _propertyViewers[property.PropertyType].Draw(property, component);
                }
                catch { }
            }
        }

        public void AddTextureBound( TextureBound textureBound)
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

        public class TextureBound
        {
            public Texture2D Texture { get; set; }
            public IntPtr IntPtr { get; set; }
        }
    }
}
