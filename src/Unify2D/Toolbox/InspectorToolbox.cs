using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unify2D.Assets;
using Unify2D.Core;

namespace Unify2D.Toolbox
{
    internal class InspectorToolbox : Toolbox
    {
        GameEditor _editor;
        GameObject _gameObject;
        Asset _asset;

        List<TextureBound> _texturesBound = new List<TextureBound>();

        public override void Initialize(GameEditor editor)
        {
            _editor = editor;
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

            foreach (var item in _texturesBound)
            {
                GameEditor.Instance.Renderer.UnbindTexture(item.IntPtr);
            }

            _texturesBound.Clear();

        }

        public override void Show()
        {
            ImGui.Begin("Inspector");

            if (_gameObject != null)
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
            ImGui.InputFloat2("position", ref position);
            _gameObject.Position = new Vector2(position.X, position.Y);


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

        IntPtr handle;
        private void ShowComponent(Component component)
        {
            PropertyInfo[] properties = component.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType == typeof(Color))
                {
                    Color color = (Color)property.GetValue(component);
                    System.Numerics.Vector4 vector = new System.Numerics.Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

                    if (ImGui.ColorEdit4(property.Name, ref vector))
                    {
                        color = new Color(vector.X, vector.Y, vector.Z, vector.W);
                        property.SetValue(component, color);
                    }

                }
                else if (property.PropertyType == typeof(int))
                {
                    int value = (int)property.GetValue(component);
                    if (ImGui.InputInt(property.Name, ref value))
                    {
                        property.SetValue(component, value);
                    }
                }
                else if (property.PropertyType == typeof(float))
                {
                    float value = (float)property.GetValue(component);
                    if (ImGui.InputFloat(property.Name, ref value))
                    {
                        property.SetValue(component, value);
                    }
                }
                else if (property.PropertyType == typeof(GameAsset))
                {
                    GameAsset value = property.GetValue(component) as GameAsset;
                    string name = "none";

                    if (value != null)
                    {
                        name = value.Name;
                        ImGui.InputText("path", ref name, 50);
                    }

                    if (value != null)
                    {
                        Texture2D texture = value.Asset as Texture2D;
                        TextureBound textureBound = GetTextureBound(texture);
                        if (textureBound == null)
                        {
                            IntPtr ptr = GameEditor.Instance.Renderer.BindTexture(texture);

                            textureBound = new TextureBound { IntPtr = ptr, Texture = texture };
                            _texturesBound.Add(textureBound);
                        }

                        ImGui.Image(textureBound.IntPtr, new System.Numerics.Vector2(40, 40));
                    }
                }
            }
        }

        TextureBound GetTextureBound(Texture2D texture)
        {
            foreach (var item in _texturesBound)
            {
                if (item.Texture == texture)
                    return item;
            }

            return null;
        }

        class TextureBound
        {
            public Texture2D Texture { get; set; }
            public IntPtr IntPtr { get; set; }
        }

    }
}
