using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Unify2D.Assets;
using Unify2D.Builder;
using Unify2D.Core;
using Unify2D.Core.Graphics;
using Unify2D.ImGuiRenderer;
using Unify2D.Toolbox;
using Unify2D.Toolbox.Popup;
using Unify2D.Tools;
using Num = System.Numerics;

namespace Unify2D
{
    /// <summary>
    /// 
    /// </summary>
    public static class Selection
    {
        enum SelectedState
        {
            None,
            Select,
            Drag
        }
        private static SelectedState _selectState = SelectedState.None;

        private static object Selected 
        {
            set
            {
                _gameObject = value as GameObject;
                _asset = value as Asset;
            }
        }
        static GameObject _gameObject;
        static Asset _asset;
        static Vector2 _dragOffset;
        private static float _timeAtLastClick;

        public static bool TryGameObject(out GameObject go)
        {
            go = _gameObject;
            return go != null;
        }
        internal static bool TryAsset(out Asset asset)
        {
            asset = _asset;
            return asset != null;
        }
        public static void SelectObject(object obj)
        {
            Selected = obj;

            if (obj == null)
            {
                UnSelectObject();
                return;
            }

            if (_asset != null)
            {
                if (_asset.AssetContent is ScriptAssetContent)
                {
                    GameEditor.Instance.ScriptToolbox.SetObject(_asset);
                    return;
                }
            }

            if (_gameObject != null)
            {

            }

            if (GameEditor.Instance.InspectorToolbox != null)
            {
                GameEditor.Instance.InspectorToolbox.SetObject(obj);
            }
        }
        public static void UnSelectObject()
        {
            Selected = null;

            if (GameEditor.Instance.InspectorToolbox != null)
            {
                GameEditor.Instance.InspectorToolbox.SetObject(null);
            }
        }

        internal static void Update(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();

            if (_selectState == SelectedState.None && mouseState.LeftButton == ButtonState.Pressed && GameEditor.Instance.IsMouseInGameWindow())
            {
                Vector2 worldPosition = GameEditor.Instance.GetWorldMousePosition();

                foreach (var item in GameCore.Current.GameObjects)
                {
                    if (worldPosition.X >= item.Position.X - item.BoundingSize.X / 2 && worldPosition.X <= item.Position.X + item.BoundingSize.X / 2
                        && worldPosition.Y >= item.Position.Y - item.BoundingSize.Y / 2 && worldPosition.Y <= item.Position.Y + item.BoundingSize.Y / 2)
                    {
                        if(item == _gameObject && gameTime.TotalGameTime.Seconds - _timeAtLastClick < 0.5f)
                        {// Double click, focus
                            GameEditor.Instance.GameToolbox.GoTo(item.Position);
                        }

                        SelectObject(item);
                        _selectState = SelectedState.Drag;
                        _dragOffset = GameEditor.Instance.GetWorldMousePosition() - item.Position;

                        _timeAtLastClick = gameTime.TotalGameTime.Seconds;
                        break;
                    }
                }
            }
            else if (_selectState == SelectedState.Drag)
            {
                if (mouseState.LeftButton == ButtonState.Pressed && _gameObject != null)
                {
                    _gameObject.Position = GameEditor.Instance.GetWorldMousePosition() - _dragOffset;
                }
                if (mouseState.LeftButton == ButtonState.Released)
                {
                    _selectState = SelectedState.None;
                }
            }
        }

        public static void CircleSelected()
        {
            if (! TryGameObject(out GameObject go)) return;

            var p0 = ImGui.GetItemRectMin();
            var p1 = ImGui.GetItemRectMax();

            var drawList = ImGui.GetWindowDrawList();
            drawList.PushClipRect(p0, p1);

            uint color = ToolsUI.ToColor32(50, 255, 50, 255);

            if (_selectState == SelectedState.Drag)
            {
                color = ToolsUI.ToColor32(255, 255, 50, 255);
            }

            drawList.AddCircle(GameEditor.Instance.GameToolbox.WorldToUI(go.Position),
                      8, color, 64, 3);
            drawList.PopClipRect();
        }
    }
}



