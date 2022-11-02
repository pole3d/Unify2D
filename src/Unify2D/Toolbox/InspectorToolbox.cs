using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Assets;
using Unify2D.Core;
using Unify2D.Tools;

namespace Unify2D.Toolbox
{
    internal class InspectorToolbox : Toolbox
    {
        GameEditor _editor;
        GameObject _gameObject;

        public override void Initialize(GameEditor editor)
        {
            _editor = editor;
        }

        public void SetGameObject(GameObject go)
        {
            _gameObject = go;
        }

        public override void Show()
        {
            ImGui.Begin("Inspector");
            {
                if (_gameObject != null)
                {
                    string name = _gameObject.Name;

                    ImGui.InputText("name", ref name, 40);
                    _gameObject.Name = name;
                    System.Numerics.Vector2 position = new System.Numerics.Vector2(_gameObject.Position.X, _gameObject.Position.Y);
                    ImGui.InputFloat2("position", ref position);
                    _gameObject.Position = new Vector2(position.X, position.Y);
                    foreach (var component in _gameObject.Components)
                    {
                        ImGui.SetNextItemOpen(true, ImGuiCond.Once);
                        if (ImGui.TreeNode(component.GetType().Name))
                        {
                            ShowComponent(component);
                            ImGui.TreePop();

                        }
                    }
                }
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Add Component"))
            {
                foreach (var item in _editor.Scripting.GetTypes())
                {
                    if ( ImGui.Button(item.Name))
                    {
                        var component = Activator.CreateInstance(item);
                        _gameObject.AddComponent(component as Component);
                    }
                }
            }

            ImGui.End();
        }

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
            }
        }
    }
}
